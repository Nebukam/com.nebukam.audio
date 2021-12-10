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

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct ReadBandsJob : IJobParallelFor, IFrameReadJob
    {

        [ReadOnly]
        private NativeArray<float> m_FFTparams;
        public NativeArray<float> inputParams { set { m_FFTparams = value; } }

        [ReadOnly]
        private NativeArray<SpectrumFrameData> m_inputFrameData;
        public NativeArray<SpectrumFrameData> inputFrameData { set { m_inputFrameData = value; } }

        [WriteOnly]
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Sample> m_outputFrameSamples;
        public NativeArray<Sample> outputFrameSamples { set { m_outputFrameSamples = value; } }

        [ReadOnly]
        public NativeArray<float> m_inputBands8;
        [ReadOnly]
        public NativeArray<float> m_inputBands16;
        [ReadOnly]
        public NativeArray<float> m_inputBands32;
        [ReadOnly]
        public NativeArray<float> m_inputBands64;
        [ReadOnly]
        public NativeArray<float> m_inputBands128;

        public void Execute(int index)
        {
            SpectrumFrameData currentFrame = m_inputFrameData[index];
            if(currentFrame.extraction != FrequencyExtraction.Bands) { return; }
        }

    }
}
