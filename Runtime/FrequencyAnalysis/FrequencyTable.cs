// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Nebukam.JobAssist;
using Nebukam;
using Nebukam.Collections;
using static Nebukam.JobAssist.Extensions;
using Nebukam.Signals;

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

        public int Start(int bins)
        {
            if (bins == 256) return start256;
            if (bins == 512) return start512;
            if (bins == 1024) return start1024;
            if (bins == 2048) return start2048;
            if (bins == 4096) return start4096;
            return 0;
        }

        #region start values

        public int start256;
        public int start512;
        public int start1024;
        public int start2048;
        public int start4096;

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

        #endregion

        #region length values

        public int length256;
        public int length512;
        public int length1024;
        public int length2048;
        public int length4096;

        public int Length(int bins)
        {
            if (bins == 256) return length256;
            if (bins == 512) return length512;
            if (bins == 1024) return length1024;
            if (bins == 2048) return length2048;
            if (bins == 4096) return length4096;
            return 0;
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

        #endregion

    }

    public struct BandInfos
    {

        public float normalizedIndex;
        public float width; // Width of the band in Hz

        #region start values

        public int start256;
        public int start512;
        public int start1024;
        public int start2048;
        public int start4096;

        public int Start(int bins)
        {
            if (bins == 256) return start256;
            if (bins == 512) return start512;
            if (bins == 1024) return start1024;
            if (bins == 2048) return start2048;
            if (bins == 4096) return start4096;
            return 0;
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

        #endregion

        #region length values

        public int length256;
        public int length512;
        public int length1024;
        public int length2048;
        public int length4096;

        public int Length(int bins)
        {
            if (bins == 256) return length256;
            if (bins == 512) return length512;
            if (bins == 1024) return length1024;
            if (bins == 2048) return length2048;
            if (bins == 4096) return length4096;
            return 0;
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

        #endregion

    }

    public struct FrequencyTableData
    {
        int start;
        int end;
        int count;
    }

    public struct BracketData
    {
        public float min;
        public float max;
        public float average;
        public int width;
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

        internal static Bands[] __bandTypes;
        internal static Bins[] __binsTypes;

        internal static int m_maxHz = 20000;
        public static int maxHz { get { return m_maxHz; } }

        static FrequencyTable()
        {
            __bandTypes = new Bands[] { Bands.band8, Bands.band16, Bands.band32, Bands.band64, Bands.band128 };
            __binsTypes = new Bins[] { Bins.length256, Bins.length512, Bins.length1024, Bins.length2048, Bins.length4096 };
        }

        #endregion

        public bool UseRawDistribution = false;

        public List<int> Brackets;

        protected FrequencyRange[] m_ranges;
        protected BandInfosPair[] m_bandInfos;

        public int Count { get { return m_ranges.Length; } }
        public FrequencyRange[] ranges { get { return m_ranges; } }

        public BandInfosPair GetBandInfosPair(Bands bands)
        {
            if (bands == Bands.band8) return m_bandInfos[0];
            if (bands == Bands.band16) return m_bandInfos[1];
            if (bands == Bands.band32) return m_bandInfos[2];
            if (bands == Bands.band64) return m_bandInfos[3];
            if (bands == Bands.band128) return m_bandInfos[4];
            return null;
        }

        public BandInfos[] GetBandInfos(Bands bands)
        {
            return GetBandInfosPair(bands).bandInfos;
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

        #region Static registry

#if UNITY_EDITOR

        internal static bool __staticInit = false;

        public static List<FrequencyTable> __loadedFrequencyTableList;
        public static Signal<FrequencyTable> __onFrequencyTableAdded = new Signal<FrequencyTable>();
        public static Signal<FrequencyTable> __onFrequencyTableRemoved = new Signal<FrequencyTable>();

        internal static void __InitStaticRegistry(FrequencyTable table)
        {

            if (!__staticInit)
            {
                __staticInit = true;
                __loadedFrequencyTableList = new List<FrequencyTable>();
            }

            __loadedFrequencyTableList.AddOnce(table);
            __onFrequencyTableAdded.Dispatch(table);

        }

        internal static void __CleanupStaticRegistry()
        {

            if (!__staticInit) { return; }
            __staticInit = false;

            foreach (FrequencyTable table in __loadedFrequencyTableList)
                __onFrequencyTableRemoved.Dispatch(table);

            __loadedFrequencyTableList.Clear();
            __loadedFrequencyTableList = null;

        }

#endif

        void OnEnable()
        {

            BuildTable();

#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += __OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += __OnAfterAssemblyReload;

            __InitStaticRegistry(this);
#endif
        }

#if UNITY_EDITOR
        void OnDisable()
        {

            AssemblyReloadEvents.beforeAssemblyReload -= __OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= __OnAfterAssemblyReload;

            __loadedFrequencyTableList?.TryRemove(this);
            __onFrequencyTableRemoved.Dispatch(this);

        }

        public void __OnBeforeAssemblyReload()
        {
            __CleanupStaticRegistry();
        }

        public void __OnAfterAssemblyReload()
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

        public int[] m_binsPeakIterations;

        public BandInfosPair(Bands b)
        {
            bands = b;
        }

        public int GetPeak(Bins bins)
        {
            return m_binsPeakIterations[Array.IndexOf(FrequencyTable.__binsTypes, bins)];
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

            }

            //////////

            // Compute indices for each bin size

            int binIndex = 0;
            foreach (Bins bin in FrequencyTable.__binsTypes)
            {
                BandInfos band;

                int
                    covered = 0,
                    maxIterations = 0,
                    iterationRemainder = (int)bin;

                float
                    binSize = math.floor((float)maxHz / (float)iterationRemainder);

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
                    band.Start(bin, covered);

                    iterationRemainder -= iterationCount;
                    covered += iterationCount;
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

                m_binsPeakIterations[binIndex] = maxIterations;
                binIndex++;

            }

        }

        #region Backup frequency ranges

        /*
        
        internal static FrequencyRange[] ___referenceRanges = new FrequencyRange[]
            {
            new FrequencyRange( 20 ), // Start of audible range
            new FrequencyRange( 25 ),
            new FrequencyRange( 31 ),
            new FrequencyRange( 40 ),
            new FrequencyRange( 50 ),
            new FrequencyRange( 63 ),
            new FrequencyRange( 80 ),
            new FrequencyRange( 100 ),
            new FrequencyRange( 125 ),
            new FrequencyRange( 160 ),
            new FrequencyRange( 200 ),
            new FrequencyRange( 250 ),
            new FrequencyRange( 315 ),
            new FrequencyRange( 400 ),
            new FrequencyRange( 500 ),
            new FrequencyRange( 630 ),
            new FrequencyRange( 800 ),
            new FrequencyRange( 1000 ),
            new FrequencyRange( 1250 ),
            new FrequencyRange( 1600 ),
            new FrequencyRange( 2000 ),
            new FrequencyRange( 2500 ),
            new FrequencyRange( 3150 ),
            new FrequencyRange( 4000 ),
            new FrequencyRange( 5000 ),
            new FrequencyRange( 6300 ),
            new FrequencyRange( 8000 ),
            new FrequencyRange( 10000 ),
            new FrequencyRange( 12500 ),
            new FrequencyRange( 16000 ),
            new FrequencyRange( FrequencyTable.maxHz ) // & Up end of audible range
            };

        internal static FrequencyRange[] ___mainRanges = new FrequencyRange[]
        {
            // https://www.teachmeaudio.com/mixing/techniques/audio-spectrum

            //new FRange( 20 ), // 0-20 : --
            new FrequencyRange( 60 ), // 20-60 : Sub-bass
            new FrequencyRange( 250 ), // 60-250 : Bass
            new FrequencyRange( 500 ), // 250-500 : Low Midrange
            new FrequencyRange( 2000 ), // 500-2000 : Midrange
            new FrequencyRange( 4000 ), // 2000-4000 : Upper Midrange
            new FrequencyRange( 6000 ), // 4000-6000 : Presence
            new FrequencyRange( 20000 ), // 6000-20000 : Brilliance 
            new FrequencyRange( FrequencyTable.maxHz ) // 20000-MAX : --

        };

        */

        #endregion

    }

}
