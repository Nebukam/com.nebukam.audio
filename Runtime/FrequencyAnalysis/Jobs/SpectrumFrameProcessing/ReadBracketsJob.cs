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
using static Unity.Mathematics.math;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct ReadBracketsJob : IJobParallelFor, IFrameReadJob
    {

        [ReadOnly]
        private NativeArray<SpectrumFrameData> m_inputFrameData;
        public NativeArray<SpectrumFrameData> inputFrameData { set { m_inputFrameData = value; } }

        [WriteOnly]
        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Sample> m_outputFrameSamples;
        public NativeArray<Sample> outputFrameSamples { set { m_outputFrameSamples = value; } }

        [ReadOnly]
        public NativeArray<BracketData> m_inputBrackets;

        public void Execute(int index)
        {
            SpectrumFrameData frame = m_inputFrameData[index];

            if (frame.extraction != FrequencyExtraction.Bracket) { return; }

            int
                numSamples = max(1, frame.frequenciesBracket.y),
                startIndex = frame.frequenciesBracket.x,
                endIndex = clamp(frame.frequenciesBracket.x + numSamples, 0, m_inputBrackets.Length),
                reached = 0;

            float
                ampBegin = frame.amplitude.x,
                ampEnd = ampBegin + frame.amplitude.y,
                inputScale = frame.inputScale,
                outputScale = frame.outputScale,
                average = 0f,
                peak = 0f,
                sum = 0f;

            for (int i = startIndex; i < endIndex; i++)
            {

                BracketData bracket = m_inputBrackets[i];
                //TODO : Give more agency to user when using brackets
                float value = bracket.average * inputScale;

                if (value >= ampBegin) { reached++; }

                value = clamp(value, ampBegin, ampEnd);
                float nrmValue = map(value, ampBegin, ampEnd, 0f, 1f);

                //use bracket peak 
                peak = max(peak, nrmValue); 
                sum += nrmValue;

            }

            if (frame.tolerance == Tolerance.Strict && reached != numSamples)
            {
                peak = 0f;
                sum = 0f;
            }
            else
            {
                average = sum / numSamples;
            }

            m_outputFrameSamples[index] = new Sample()
            {
                output = frame.output,
                average = average * outputScale,
                peak = peak * outputScale,
                sum = sum * outputScale,
                trigger = (peak > 0f ? 1f : 0f)
            };


        }

        internal float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

    }
}
