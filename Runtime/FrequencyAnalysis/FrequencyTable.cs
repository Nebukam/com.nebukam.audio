using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using static Nebukam.JobAssist.CollectionsUtils;


namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum Bins
    {
        length256 = 256,
        length512 = 512,
        length1024 = 1024,
        length2048 = 2048,
        length4096 = 4096
    }

    public struct FrequencyRange
    {
        public int Hz;
        public int width;
        public float normalizedCoverage;
        public float linearCoverageEnd;

        public int width256;
        public int width512;
        public int width1024;
        public int width2048;
        public int width4096;

        public FrequencyRange(int hz)
        {

            Hz = hz;
            width = 0;
            normalizedCoverage = 0f;
            linearCoverageEnd = 0f;

            width256 = 0;
            width512 = 0;
            width1024 = 0;
            width2048 = 0;
            width4096 = 0;

        }

        public int Width(Bins bins)
        {
            if (bins == Bins.length256) return width256;
            if (bins == Bins.length512) return width512;
            if (bins == Bins.length1024) return width1024;
            if (bins == Bins.length2048) return width2048;
            if (bins == Bins.length4096) return width4096;
            return 0;
        }

        public void Width(Bins bins, int length)
        {
            if (bins == Bins.length256) { width256 = length; }
            if (bins == Bins.length512) { width512 = length; }
            if (bins == Bins.length1024) { width1024 = length; }
            if (bins == Bins.length2048) { width2048 = length; }
            if (bins == Bins.length4096) { width4096 = length; }
        }

    }

    public struct BandInfos
    {

        public float normalizedIndex;
        public float width; // Width of the band in Hz

        public int length256;
        public int length512;
        public int length1024;
        public int length2048;
        public int length4096;
        
        public int Length(Bins bins)
        {
            if (bins == Bins.length256) return length256;
            if (bins == Bins.length512) return length512;
            if (bins == Bins.length1024) return length1024;
            if (bins == Bins.length2048) return length2048;
            if (bins == Bins.length4096) return length4096;
            return 0;
        }

        public void Length(Bins bins, int length)
        {
            if (bins == Bins.length256) { length256 = length; }
            if (bins == Bins.length512) { length512 = length; }
            if (bins == Bins.length1024) { length1024 = length; }
            if (bins == Bins.length2048) { length2048 = length; }
            if (bins == Bins.length4096) { length4096 = length; }
        }

    }

    public struct SpectrumInfos
    {

        public int frequency;
        public Bins frequencyBins;
        public int sampleFrequency;
        public int numChannels;
        public int numSamples;
        public int pointCount;

        public int coverage; // Number of samples required to cover all channels

        public SpectrumInfos(Bins bins, AudioClip clip)
        {
            frequencyBins = bins;
            frequency = clip.frequency;
            numSamples = clip.samples;
            numChannels = clip.channels;
            pointCount = (int)bins * 2;
            coverage = pointCount * numChannels; // Signals are inlined like triangle data
            sampleFrequency = frequency / (int)bins;
        }

        public void EnsureCoverage(ref float[] array)
        {
            if (array.Length != coverage) { array = new float[coverage]; }
        }

        public int TimeIndex(float time)
        {
            return (int)(frequency * time);
        }

    }

    [ExecuteAlways]
    [System.Serializable]
    [CreateAssetMenu(fileName = "Frequency Table", menuName = "N:Toolkit/Audio/Frequency Table", order = 0)]
    public class FrequencyTable : ScriptableObject
    {

        internal static int m_maxHz = 20000;

        public bool UseRawDistribution = false;

        public List<int> FrequencyBrackets;

        protected FrequencyRange[] m_ranges;
        protected BandInfosPair[] m_bandInfos;

        private void Awake()
        {
            BuildTable();
        }

        internal static Array __bandTypes = Enum.GetValues(typeof(Bands));
        internal static Array __binsTypes = Enum.GetValues(typeof(Bins));

        public BandInfosPair GetBandInfosPair(Bands bands)
        {
            return m_bandInfos[Array.IndexOf(__bandTypes, bands)];
        }

        public void GetBandInfos(out BandInfos[] array, Bands bands)
        {
            array = m_bandInfos[Array.IndexOf(__bandTypes, bands)].bandInfos;
        }

        public void GetBandInfos(out NativeArray<BandInfos> array, Bands bands)
        {
            array = m_bandInfos[Array.IndexOf(__bandTypes, bands)].nativeBandInfos;
        }

        public void BuildTable()
        {

            int numRanges = 1;

            if (FrequencyBrackets != null)
            {
                numRanges = FrequencyBrackets.Count + 1;
                FrequencyBrackets.Sort();
            }

            // Initialize range data

            m_ranges = new FrequencyRange[numRanges];

            FrequencyRange
                po = new FrequencyRange(0),
                o;

            for (int i = 0; i < numRanges; i++)
            {

                o = m_ranges[i];
                o.Hz = i == numRanges - 1 ? m_maxHz : FrequencyBrackets[i];

                if (i != 0)
                    o.width = o.Hz - po.Hz;
                else
                    o.width = o.Hz;

                o.normalizedCoverage = (float)o.width / (float)m_maxHz;
                o.linearCoverageEnd = (float)(i + 1) / (float)numRanges;
                m_ranges[i] = o;
                po = o;

            }

            // Initialize bands infos

            // Dispose previous pairs
            if (m_bandInfos != null)
            {
                foreach (BandInfosPair pair in m_bandInfos)
                {
                    pair.Dispose();
                }
            }

            m_bandInfos = new BandInfosPair[__bandTypes.Length];

            BandInfosPair infosPair;
            int index = 0;

            foreach (Bands b in __bandTypes)
            {
                infosPair = new BandInfosPair(b);
                infosPair.Initialize(m_ranges, UseRawDistribution);
                m_bandInfos[index] = infosPair;
                index++;
            }

            // TODO : Compute width indices for each bin size / FRange

        }

        /// <summary>
        /// Release unmanaged ressources
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < m_bandInfos.Length; i++)
                m_bandInfos[i].Dispose();
        }


    }

    public class BandInfosPair
    {

        internal Bands bands;

        protected BandInfos[] m_bandInfos;
        public BandInfos[] bandInfos { get { return m_bandInfos; } }

        protected bool m_nativeCollectionsInitialized = false;
        protected NativeArray<BandInfos> m_nativeBandInfos;
        public NativeArray<BandInfos> nativeBandInfos { 
            get {

                if (!m_nativeCollectionsInitialized)
                {
                    m_nativeCollectionsInitialized = true;
                    m_nativeBandInfos = new NativeArray<BandInfos>(m_bandInfos.Length, Allocator.Persistent);
                    Copy(ref m_bandInfos, ref m_nativeBandInfos);
                }

                return m_nativeBandInfos; 
            }
        }

        public int[] m_binsPeakIterations;

        public BandInfosPair(Bands b)
        {
            bands = b;
        }

        public int GetPeak(Bins bins)
        {
            return m_binsPeakIterations[Array.IndexOf(FrequencyTable.__binsTypes, bins)];
        }

        

        public void InitializeNativeCollections()
        {

        }

        internal void Initialize(FrequencyRange[] ranges, bool useRawDistribution = false)
        {

            m_bandInfos = new BandInfos[(int)bands];

            m_binsPeakIterations = new int[(int)bands];

            int
                numBands = (int)bands,
                numRanges = ranges.Length;

            float
                maxHz = FrequencyTable.m_maxHz;

            // Each bands cover a frequency range
            // Based on bin size, we know how many Hz an individual bin covers
            // We want to aggregate a variety of ranges to shorter bands
            // |---|---|---|---|---|---| 
            // |||||||||||||||||||||||||

            // Precompute each band normalizedIndex
            for (int i = 0; i < numBands; i++)
            {
                BandInfos band = m_bandInfos[i];
                band.normalizedIndex = (float)i / (float)numBands;
                m_bandInfos[i] = band;
            }

            //////

            // First we need to find what frequency width each band has to cover
            // The frequencies values are non-linear so each band doesn't cover the same width
            float
                bandWidth = 1f / numBands, // The normalized size of a single band
                currentCoverage = 0f,
                currentRangeLength = ranges[0].linearCoverageEnd,
                rangeProgress = 0f;

            int
                lastRange = 0;

            // Go over each band
            for (int b = 0; b < numBands; b++)
            {

                BandInfos
                    currentBand = m_bandInfos[b];

                float
                    bandLimit = ((float)b + 1f) * bandWidth,
                    bandRemainder = bandWidth;

                //Debug.Log("START >>>> Band " + (b+1) + "/" + numBands + "[ "+ bandNStart + " | "+ bandNEnd + " ]");

                while (currentCoverage < bandLimit)
                {

                    int
                        rangeIndex = 0;

                    FrequencyRange
                        currentRange = ranges[rangeIndex];

                    for (rangeIndex = 0; rangeIndex < numRanges; rangeIndex++)
                    {
                        currentRange = ranges[rangeIndex];
                        if (currentCoverage < currentRange.linearCoverageEnd)
                            break;
                    }

                    if (rangeIndex != lastRange) // Start a new range
                    {
                        currentRangeLength = currentRange.linearCoverageEnd - ranges[rangeIndex - 1].linearCoverageEnd;
                        rangeProgress = 0f;
                    }

                    float coveredLength = math.min(currentRange.linearCoverageEnd - currentCoverage, bandRemainder);

                    //Advance coverage
                    currentCoverage += coveredLength;
                    rangeProgress += coveredLength;
                    bandRemainder -= coveredLength;

                    currentBand.width += currentRange.width * ((coveredLength / currentRangeLength));

                    m_bandInfos[b] = currentBand;

                }

                //Debug.Log("Band " + (b + 1) + "/" + numBands + " -> " + bands[b].width + "Hz ----------");

            }

            float total = 0f;
            for (int i = 0; i < numBands; i++)
                total += m_bandInfos[i].width;

            //Debug.LogError("---> "+ total);

            //////////

            // Compute indices for each bin size

            int binIndex = 0;
            foreach (Bins bin in FrequencyTable.__binsTypes)
            {
                BandInfos band;

                int maxIterations = 0;
                int iterationRemainder = (int)bin;
                float binSize = math.floor((float)maxHz / (float)iterationRemainder);

                //string str = "";

                for (int i = 0; i < numBands; i++)
                {
                    band = m_bandInfos[i];
                    int iterationCount;

                    if (!useRawDistribution)
                        iterationCount = (int)math.max(math.floor((float)band.width / binSize), 1f);
                    else
                        iterationCount = (int)((float)band.width / binSize);

                    iterationCount = math.min(iterationRemainder, iterationCount);
                    band.Length(bin, iterationCount);
                    //str += ", " + iterationCount;
                    iterationRemainder -= iterationCount;
                    m_bandInfos[i] = band;
                    maxIterations = math.max(maxIterations, iterationCount);
                }

                if (iterationRemainder != 0)
                {
                    band = m_bandInfos[numBands - 1];
                    band.Length(bin, band.Length(bin) + iterationRemainder);
                    m_bandInfos[numBands - 1] = band;
                    maxIterations = math.max(maxIterations, band.Length(bin));
                }

                int ttl = 0;
                for (int i = 0; i < numBands; i++)
                    ttl += m_bandInfos[i].Length(bin);

                m_binsPeakIterations[binIndex] = maxIterations;
                binIndex++;
                //Debug.Log(str + " = "+ttl+"("+ iterationRemainder + ")");

            }

        }

        internal void Dispose()
        {
            m_bandInfos = null;
            if(m_nativeCollectionsInitialized)
                m_nativeBandInfos.Dispose();
        }

    }
}
