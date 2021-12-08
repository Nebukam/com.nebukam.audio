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

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FFT4Stage : ParallelProcessor<FFT4StageJob>
    {

        protected NativeSlice<TFactor> m_stageSlice;
        protected int m_stageStartIndex = -1;

        public FFTParams fftParams { get; set; }
        public NativeArray<float4> inputComplexPair { get; set; }
        public NativeArray<TFactor> inputTwiddleFactors { get; set; }

        protected override int Prepare(ref FFT4StageJob job, float delta)
        {

            int numIterations = fftParams.numBins / 2;
            int newStageStart = numIterations * compoundIndex;

            if (m_stageStartIndex != newStageStart)
            {
                m_stageStartIndex = newStageStart;
                m_stageSlice = new NativeSlice<TFactor>(inputTwiddleFactors, m_stageStartIndex);
            }

            job.m_stageSlice = m_stageSlice;
            job.m_outputComplexPair = inputComplexPair;

            return numIterations;

        }

    }

}
