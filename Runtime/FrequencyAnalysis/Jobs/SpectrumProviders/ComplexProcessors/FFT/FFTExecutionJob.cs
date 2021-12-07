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
    public struct FFTExecutionJob : IJob, IComplexJob
    {

        [ReadOnly]
        public NativeArray<float> m_params;

        public NativeArray<ComplexFloat> m_outputComplexFloats;
        public NativeArray<ComplexFloat> complexFloats { set { m_outputComplexFloats = value; } }

        public NativeArray<ComplexFloat> m_inputComplexFloatsFull;

        public NativeArray<FFTElement> m_inputFFTElements;

        [ReadOnly]
        public NativeArray<float> m_inputSamples;

        public void Execute()
        {

            int
                FFTLogN = (int)m_params[FFTParams.LOG_N],
                pointCount = m_inputFFTElements.Length,
                numFlies = pointCount >> 1,
                span = (int)numFlies,
                spacing = pointCount,
                wIndexStep = 1;

            float
                FFTScale = sqrt(2) / (float)pointCount, // Natural FFT Scale Factor
                TAU_INV = -2.0f * PI;

            // Copy data into linked complex number objects
            FFTElement ffte = m_inputFFTElements[0];
            /*
            for (int i = 0; i < pointCount; i++)
            {
                ffte.re = m_inputSamples[i];
                ffte.im = 0.0f;

                m_inputFFTElements[ffte.index] = ffte;

                if (ffte.next != -1)
                    ffte = m_inputFFTElements[ffte.next];

            }
            */

            for (int i = 0; i < pointCount; i++)
            {
                ffte = m_inputFFTElements[i];

                ffte.re = m_inputSamples[i];
                ffte.im = 0.0f;

                m_inputFFTElements[i] = ffte;
            }

            for (int stage = 0; stage < FFTLogN; stage++)
            {

                float 
                    wAngleInc = wIndexStep * TAU_INV / (float)pointCount,
                    wMulRe = cos(wAngleInc),
                    wMulIm = sin(wAngleInc);

                for (int start = 0; start < pointCount; start += spacing)
                {

                    FFTElement 
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

            // The algorithm leaves the result in a scrambled order.
            // Unscramble while copying values from the complex
            // linked list elements to a complex output vector & properly apply scale factors.

            ffte = m_inputFFTElements[0];
            bool run = true;
            while (run)
            {
                m_inputComplexFloatsFull[(int)ffte.revTgt]
                    = new ComplexFloat(ffte.re * FFTScale, ffte.im * FFTScale);

                if (ffte.next == -1)
                    run = false;
                else
                    ffte = m_inputFFTElements[ffte.next];
            }


            int mLengthHalf = m_outputComplexFloats.Length;
            NativeArray<ComplexFloat>.Copy(m_inputComplexFloatsFull, m_outputComplexFloats, mLengthHalf);

            // DC and Fs/2 Points are scaled differently, since they have only a real part
            m_outputComplexFloats[0] = new ComplexFloat(m_outputComplexFloats[0].real / sqrt(2));
            m_outputComplexFloats[mLengthHalf - 1] = new ComplexFloat(m_outputComplexFloats[mLengthHalf - 1].real / sqrt(2));

        }

    }
}
