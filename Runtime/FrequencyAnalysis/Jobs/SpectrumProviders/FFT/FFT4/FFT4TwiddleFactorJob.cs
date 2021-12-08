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
using static Unity.Mathematics.math;
using UnityEngine;
using Unity.Profiling;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FFT4TwiddleFactorJob : IJob
    {

        public bool m_recompute;

        [ReadOnly]
        public NativeArray<float> m_params;

        public NativeArray<TFactor> m_twiddleFactors;

        public void Execute()
        {

            if (!m_recompute) { return; }

            float 
                TAU_INV = -2f * math.PI;

            int 
                twiddleIndex = 0,
                pointCount = (int)m_params[FFTParams.NUM_POINTS];

            for (int m = 4; m <= pointCount; m <<= 1)
            {
                for (int k = 0; k < pointCount; k += m)
                {
                    for (int j = 0, nj = m / 2; j < nj; j += 2)
                    {

                        m_twiddleFactors[twiddleIndex++] = new TFactor
                        {
                            indices = int2((k + j) / 2, (k + j + m / 2) / 2),
                            wings = cos(TAU_INV / m * float2(j, j + 1))
                        };

                        //twiddleIndex++;

                    }
                }
            }

        }

    }
}
