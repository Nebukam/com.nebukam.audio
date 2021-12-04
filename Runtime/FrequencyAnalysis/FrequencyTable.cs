using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
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

        public int start256;
        public int start512;
        public int start1024;
        public int start2048;
        public int start4096;

        public int length256;
        public int length512;
        public int length1024;
        public int length2048;
        public int length4096;

        public FrequencyRange(int hz)
        {

            Hz = hz;
            width = 0;
            normalizedCoverage = 0f;
            linearCoverageEnd = 0f;

            start256 = 0;
            start512 = 0;
            start1024 = 0;
            start2048 = 0;
            start4096 = 0;

            length256 = 0;
            length512 = 0;
            length1024 = 0;
            length2048 = 0;
            length4096 = 0;

        }

        public int Start(Bins bins)
        {
            if (bins == Bins.length256) return start256;
            if (bins == Bins.length512) return start512;
            if (bins == Bins.length1024) return start1024;
            if (bins == Bins.length2048) return start2048;
            if (bins == Bins.length4096) return start4096;
            return 0;
        }

        public void Start(Bins bins, int length)
        {
            if (bins == Bins.length256) { start256 = length; }
            if (bins == Bins.length512) { start512 = length; }
            if (bins == Bins.length1024) { start1024 = length; }
            if (bins == Bins.length2048) { start2048 = length; }
            if (bins == Bins.length4096) { start4096 = length; }
        }

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

    public struct FrequencyTableData
    {
        int start;
        int end;
        int count;
    }

    [ExecuteAlways]
    [System.Serializable]
    [CreateAssetMenu(fileName = "Frequency Table", menuName = "N:Toolkit/Audio/Frequency Table", order = 0)]
    public class FrequencyTable : ScriptableObject
    {

        #region Preset tables

        internal static FrequencyTable m_tableFull = null;
        public static FrequencyTable tableFull
        {
            get
            {
                if (m_tableFull == null)
                    m_tableFull = Resources.Load<FrequencyTable>("FrequencyTables/NAudio - FrequencyTable - Full");

                return m_tableFull;
            }
        }

        internal static FrequencyTable m_tableCommon = null;
        public static FrequencyTable tableCommon
        {
            get
            {
                if (m_tableCommon == null)
                    m_tableCommon = Resources.Load<FrequencyTable>("FrequencyTables/NAudio - FrequencyTable - Common");

                return m_tableCommon;
            }
        }

        #endregion

        #region Static

        internal static bool m_staticInit = false;

        internal static List<FrequencyTable> loadedFrequencyTableList;
        internal int m_staticIndex = -1;

        internal static NativeList<FrequencyTableData> globalFrequencyTableDataList;
        internal static NativeList<FrequencyRange> globalFrequencyRangeInline;

        internal static Array __bandTypes = Enum.GetValues(typeof(Bands));
        internal static Array __binsTypes = Enum.GetValues(typeof(Bins));

        internal static int m_maxHz = 20000;
        public static int maxHz { get { return m_maxHz; } }

        #endregion

        public bool UseRawDistribution = false;

        public List<int> Brackets;

        protected FrequencyRange[] m_ranges;
        protected BandInfosPair[] m_bandInfos;

        public int Count { get { return m_ranges.Length; } }
        public FrequencyRange[] ranges { get { return m_ranges; } }

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

            if (Brackets != null)
            {
                numRanges = Brackets.Count + 1;
                Brackets.Sort();
            }

            // Initialize range data

            m_ranges = new FrequencyRange[numRanges];

            FrequencyRange
                po = new FrequencyRange(0),
                o;

            for (int i = 0; i < numRanges; i++)
            {

                o = m_ranges[i];
                o.Hz = i == numRanges - 1 ? m_maxHz : Brackets[i];

                if (i != 0)
                    o.width = o.Hz - po.Hz;
                else
                    o.width = o.Hz;

                o.normalizedCoverage = (float)o.width / (float)m_maxHz;
                o.linearCoverageEnd = (float)(i + 1) / (float)numRanges;

                foreach (Bins bin in __binsTypes)
                {
                    o.Start(bin, math.clamp(po.Start(bin) + po.Length(bin), 0, (int)bin - 1));
                    o.Length(bin, (int)((float)o.normalizedCoverage * (float)(int)bin));
                }

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
            Cleanup();
        }

        private void Cleanup()
        {
            for (int i = 0; i < m_bandInfos.Length; i++)
                m_bandInfos[i].Dispose();
        }

        #region Static registry

        internal static void InitStaticRegistry()
        {

            if (m_staticInit) { return; }
            m_staticInit = true;

            loadedFrequencyTableList = new List<FrequencyTable>();
            globalFrequencyTableDataList = new NativeList<FrequencyTableData>(100, Allocator.Persistent);
            globalFrequencyRangeInline = new NativeList<FrequencyRange>(100, Allocator.Persistent);

        }

        internal static void CleanupStaticRegistry()
        {

            if (!m_staticInit) { return; }
            m_staticInit = false;

            globalFrequencyTableDataList.Dispose();
            globalFrequencyRangeInline.Dispose();
            loadedFrequencyTableList.Clear();
            loadedFrequencyTableList = null;

        }

        void OnEnable()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
#endif
            BuildTable();
            InitStaticRegistry();

            if (!loadedFrequencyTableList.Contains(this))
            {
                loadedFrequencyTableList.Add(this);
                m_staticIndex = loadedFrequencyTableList.Count - 1;
            }

        }

        void OnDisable()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
#endif
            Cleanup();

            if (loadedFrequencyTableList != null)
            {
                int index = loadedFrequencyTableList.IndexOf(this);
                if (index != -1)
                {
                    m_staticIndex = -1;
                    loadedFrequencyTableList.RemoveAt(index);

                    for (int i = 0; i < loadedFrequencyTableList.Count; i++)
                        loadedFrequencyTableList[i].m_staticIndex = i;

                }
            }

        }


#if UNITY_EDITOR

        public void OnBeforeAssemblyReload()
        {
            Cleanup();
            CleanupStaticRegistry();
        }

        public void OnAfterAssemblyReload()
        {

        }

#endif

        #endregion

    }

    public class BandInfosPair
    {

        internal Bands bands;

        protected BandInfos[] m_bandInfos;
        public BandInfos[] bandInfos { get { return m_bandInfos; } }

        protected bool m_nativeCollectionsInitialized = false;
        protected NativeArray<BandInfos> m_nativeBandInfos;
        public NativeArray<BandInfos> nativeBandInfos
        {
            get
            {

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

        internal void Initialize(FrequencyRange[] inputRanges, bool useRawDistribution = false)
        {

            m_bandInfos = new BandInfos[(int)bands];

            m_binsPeakIterations = new int[(int)bands];

            int
                numBands = (int)bands,
                numRanges = inputRanges.Length;

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
                currentRangeLength = inputRanges[0].linearCoverageEnd,
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
                        currentRange = inputRanges[rangeIndex];

                    for (rangeIndex = 0; rangeIndex < numRanges; rangeIndex++)
                    {
                        currentRange = inputRanges[rangeIndex];
                        if (currentCoverage < currentRange.linearCoverageEnd)
                            break;
                    }

                    if (rangeIndex != lastRange) // Start a new range
                    {
                        currentRangeLength = currentRange.linearCoverageEnd - inputRanges[rangeIndex - 1].linearCoverageEnd;
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
            if (m_nativeCollectionsInitialized)
                m_nativeBandInfos.Dispose();
        }

    }

}
