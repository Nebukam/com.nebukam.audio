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
    public struct FFT4PairsJob : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> m_inputSamples;

        [ReadOnly]
        public NativeArray<int2> m_permutationTable;

        [WriteOnly]
        public NativeArray<float4> m_outputComplexPair;

        public void Execute(int index)
        {
            var a1 = m_inputSamples[m_permutationTable[index].x];
            var a2 = m_inputSamples[m_permutationTable[index].y];
            m_outputComplexPair[index] = math.float4(a1 + a2, 0, a1 - a2, 0);
        }

    }
}
