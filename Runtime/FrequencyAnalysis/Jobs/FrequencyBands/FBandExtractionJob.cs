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

using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FBandExtractionJob : IJobParallelFor
    {

        [ReadOnly]
        public Bands m_referenceBands;

        [ReadOnly]
        public NativeArray<float> m_inputSpectrum;
        [ReadOnly]
        public NativeArray<BandInfos> m_inputBandInfos;

        public NativeArray<float> m_outputBands;

        public void Execute(int index)
        {

            BandInfos infos = m_inputBandInfos[index];

            int
                bins = m_inputSpectrum.Length,
                count = infos.Start(bins);

            float average = 0;

            for (int s = 0, n = infos.Length(bins); s < n; s++)
            {
                average += m_inputSpectrum[count] * (count + 1);
                count++;
            }

            average /= count;
            m_outputBands[index] = average;


        }

        /*public void Execute()
        {

            int
                bandCount = m_outputBands.Length,
                bins = m_inputSpectrum.Length,
                index = 0;

            for (int i = 0; i < bandCount; i++)
            {
                float average = 0;

                for (int s = 0, n = m_inputBandInfos[i].Length(bins); s < n; s++)
                {
                    average += m_inputSpectrum[index] * (index + 1);
                    index++;
                }

                average /= index;
                m_outputBands[i] = average;

            }

        }*/

    }
}
