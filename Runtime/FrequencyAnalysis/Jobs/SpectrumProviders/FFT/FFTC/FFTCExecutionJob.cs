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
using Unity.Jobs;
using static Unity.Mathematics.math;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FFTCExecutionJob : IJob
    {

        [ReadOnly]
        public NativeArray<float> m_params;

        [ReadOnly]
        public NativeArray<float> m_inputSamples;

        public NativeArray<FFTCElement> m_inputFFTElements;
        public NativeArray<ComplexFloat> m_outputComplexSpectrum;
        public NativeArray<ComplexFloat> m_inputComplexSamples;
        

        public void Execute()
        {

            int
                FFTLogN = (int)m_params[FFTParams.LOG_N],
                numSamples = (int)m_params[FFTParams.NUM_SAMPLES],
                numFlies = numSamples >> 1,
                span = (int)numFlies,
                spacing = numSamples,
                wIndexStep = 1;

            float
                TAU_INV = -2.0f * PI;

            for (int stage = 0; stage < FFTLogN; stage++)
            {

                float 
                    wAngleInc = wIndexStep * TAU_INV / (float)numSamples,
                    wMulRe = cos(wAngleInc),
                    wMulIm = sin(wAngleInc);

                for (int start = 0; start < numSamples; start += spacing)
                {

                    FFTCElement 
                        xTop = m_inputFFTElements[start],
                        xBot = m_inputFFTElements[start + span];

                    float 
                        wRe = 1.0f,
                        wIm = 0.0f;

                    for (int flyCount = 0; flyCount < numFlies; flyCount++)
                    {

                        float 
                            xTopRe = xTop.re,
                            xTopIm = xTop.im,
                            xBotRe = xBot.re,
                            xBotIm = xBot.im,
                            tRe;

                        xTop.re = xTopRe + xBotRe;
                        xTop.im = xTopIm + xBotIm;

                        xBotRe = xTopRe - xBotRe;
                        xBotIm = xTopIm - xBotIm;
                        xBot.re = xBotRe * wRe - xBotIm * wIm;
                        xBot.im = xBotRe * wIm + xBotIm * wRe;

                        m_inputFFTElements[xTop.index] = xTop;
                        m_inputFFTElements[xBot.index] = xBot;

                        tRe = wRe;
                        wRe = wRe * wMulRe - wIm * wMulIm;
                        wIm = tRe * wMulIm + wIm * wMulRe;

                        if (xTop.next != -1 && xBot.next != -1)
                        {
                            xTop = m_inputFFTElements[xTop.next];
                            xBot = m_inputFFTElements[xBot.next];
                        }

                    }
                }

                numFlies >>= 1;   // Divide by 2 by right shift
                span >>= 1;
                spacing >>= 1;
                wIndexStep <<= 1;     // Multiply by 2 by left shift

            }

        }

    }
}
