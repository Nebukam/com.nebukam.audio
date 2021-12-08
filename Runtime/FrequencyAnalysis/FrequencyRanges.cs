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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

namespace Nebukam.Audio.FrequencyAnalysis
{

   

    [BurstCompile]
    internal static class FrequencyRanges
    {

        internal static int m_maxHz = AudioSettings.outputSampleRate / 2;

        internal static float GetBinWidth(Bins bins) { return m_maxHz / (float)(int)bins; }

        internal static FrequencyRange[] m_referenceRanges = new FrequencyRange[]
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
            new FrequencyRange( m_maxHz ) // & Up end of audible range
        };

        internal static FrequencyRange[] m_mainRanges = new FrequencyRange[]
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
            new FrequencyRange( m_maxHz ) // 20000-MAX : --

        };

        private static BandInfos[] m_bands8 = RemapBands(new BandInfos[8]);
        private static BandInfos[] m_bands16 = RemapBands(new BandInfos[16]);
        private static BandInfos[] m_bands32 = RemapBands(new BandInfos[32]);
        private static BandInfos[] m_bands64 = RemapBands(new BandInfos[64]);
        private static BandInfos[] m_bands128 = RemapBands(new BandInfos[128]);

        private static bool m_init = false;

        private static void Init()
        {

            if (m_init) { return; }
            m_init = true;

            FrequencyRange
                po = new FrequencyRange(0),
                o;

            // Ref ranges

            int numRanges = m_referenceRanges.Length;

            for (int i = 0; i < numRanges; i++)
            {
                o = m_referenceRanges[i];

                if (i != 0)
                    o.width = o.Hz - po.Hz;
                else
                    o.width = o.Hz;

                o.normalizedCoverage = (float)o.width / (float)m_maxHz;
                o.linearCoverageEnd = (float)(i + 1) / (float)numRanges;
                m_referenceRanges[i] = o;
                po = o;
            }

            // Main ranges

            numRanges = m_mainRanges.Length;

            for (int i = 0; i < numRanges; i++)
            {
                o = m_mainRanges[i];

                if (i != 0)
                    o.width = o.Hz - po.Hz;
                else
                    o.width = o.Hz;

                o.normalizedCoverage = (float)o.width / (float)m_maxHz;
                o.linearCoverageEnd = (float)(i + 1) / (float)(numRanges);
                m_mainRanges[i] = o;
                po = o;
            }

        }

        internal static BandInfos[] GetBandInfos(int band)
        {
            if (band == 8) return m_bands8;
            if (band == 16) return m_bands16;
            if (band == 32) return m_bands32;
            if (band == 64) return m_bands64;
            return m_bands128;
        }

        internal static BandInfos[] GetBandInfos(Bands band)
        {
            return GetBandInfos((int)band);
        }

        internal static BandInfos[] RemapBands(BandInfos[] bands)
        {

            Init();

            FrequencyRange[] ranges = m_mainRanges;

            int
                numBands = bands.Length,
                numRanges = ranges.Length;

            // Each bands cover a frequency range
            // Based on bin size, we know how many Hz an individual bin covers
            // We want to aggregate a variety of ranges to shorter bands
            // |---|---|---|---|---|---| 
            // |||||||||||||||||||||||||

            // Precompute each band normalizedIndex
            for (int i = 0; i < numBands; i++)
            {
                BandInfos band = bands[i];
                band.normalizedIndex = (float)i / (float)numBands;
                bands[i] = band;
            }

            //////

            // First we need to find what frequency width each band has to cover
            // The frequencies values are non-linear so each band doesn't cover the same width
            float rangeWeight = 1f / numRanges; // The normalized size of a single range
            float bandWidth = 1f / numBands; // The normalized size of a single band

            float currentCoverage = 0f;
            int lastRange = 0;
            float currentRangeLength = ranges[0].linearCoverageEnd;
            float rangeProgress = 0f;

            // Go over each band
            for (int b = 0; b < numBands; b++)
            {

                BandInfos currentBand = bands[b];

                float 
                    bandLimit = ((float)b + 1f) * bandWidth,
                    bandRemainder = bandWidth;

                //Debug.Log("START >>>> Band " + (b+1) + "/" + numBands + "[ "+ bandNStart + " | "+ bandNEnd + " ]");

                // If current range has not been fully covered yet
                // *
                // v--v
                // |------|------|
                //
                //   v--*-v
                // |------|------|
                //
                //   v--*-v
                // |------|------|


                //Find which range(s) cover the current band
                //     v------v
                // |------|------|

                while (currentCoverage < bandLimit)
                {

                    int rangeIndex = 0;
                    FrequencyRange currentRange = ranges[rangeIndex];
                    for (rangeIndex = 0; rangeIndex < numRanges; rangeIndex++)
                    {
                        currentRange = ranges[rangeIndex];
                        if (currentCoverage < currentRange.linearCoverageEnd)
                            break;                        
                    }

                    if (rangeIndex != lastRange)
                    {
                        // Start a new range
                        currentRangeLength = currentRange.linearCoverageEnd - ranges[rangeIndex - 1].linearCoverageEnd;
                        rangeProgress = 0f;
                    }

                    // This is how much we can squeeze out of this range before moving to the next
                    //     v--x
                    // |------|------|
                    float available = currentRange.linearCoverageEnd - currentCoverage;

                    float coveredLength = math.min(available, bandRemainder);
                    /*
                    if (available < bandRemainder)
                    {
                        //     v--x--v
                        // |------|------|
                        //Available amount is not enough to fill current band
                        coveredLength = available;
                    }
                    else
                    {
                        //   v--v x  
                        // |------|------|
                        //Available amount is more than required to fill current band length
                        coveredLength = bandRemainder;
                    }
                    */
                    //Advance coverage
                    currentCoverage += coveredLength;
                    rangeProgress += coveredLength;
                    bandRemainder -= coveredLength;

                    //Compute amount of Hz covered by this band
                    currentBand.width += currentRange.width * ((coveredLength / currentRangeLength));

                    bands[b] = currentBand;

                }

            }

            //////////

            // Compute indices for each bin size

            IEnumerable<Bins> bins = Enum.GetValues(typeof(Bins)).Cast<Bins>();

            foreach (Bins bin in bins)
            {
                BandInfos band;

                int iterationRemainder = (int)bin;
                int covered = 0;
                float binSize = math.floor((float)m_maxHz / (float)iterationRemainder);

                string str = "";

                for (int i = 0; i < numBands; i++)
                {
                    band = bands[i];
                    int iterationCount = (int)math.max(math.floor((float)band.width / binSize), 1f);
                    iterationCount = math.min(iterationRemainder, iterationCount);

                    band.Start(bin, covered);
                    band.Length(bin, iterationCount);

                    covered += iterationCount;
                    iterationRemainder -= iterationCount;

                    bands[i] = band;
                    
                }

                if(iterationRemainder != 0)
                {
                    band = bands[numBands-1];
                    band.Length(bin, band.Length(bin) + iterationRemainder);
                    bands[numBands-1] = band;
                }

            }

            return bands;

        }

    }
}
