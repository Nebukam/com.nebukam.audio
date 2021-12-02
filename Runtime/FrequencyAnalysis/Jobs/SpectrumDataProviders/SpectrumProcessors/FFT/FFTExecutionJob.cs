// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com
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

    [BurstCompile]
    public struct FFTExecutionJob : IJob, IComplexJob
    {

        public NativeArray<ComplexFloat> m_complexFloats;
        public NativeArray<ComplexFloat> complexFloats { set { m_complexFloats = value; } }

        public NativeArray<ComplexFloat> m_complexFloatsFull;

        public NativeArray<FFTElement> m_FFTElements;

        public NativeArray<float> m_rawPoints;

        public uint FFTLogN;

        public void Execute()
        {

            int pointCount = m_FFTElements.Length;
            uint uPointCount = (uint)pointCount;

            float FFTScale = math.sqrt(2) / (float)pointCount; // Natural FFT Scale Factor

            float TAU_INV = -2.0f * PI;
            uint numFlies = uPointCount >> 1;  // Number of butterflies per sub-FFT
            int span = (int)numFlies;      // Width of the butterfly
            int spacing = pointCount;        // Distance between start of sub-FFTs
            uint wIndexStep = 1;               // Increment for twiddle table index

            // Copy data into linked complex number objects
            FFTElement x = m_FFTElements[0];
            int k = 0;
            for (uint i = 0; i < pointCount; i++)
            {
                x.re = m_rawPoints[k];
                x.im = 0.0f;
                x = m_FFTElements[x.next];
                k++;
            }

            // For each stage of the FFT
            for (int stage = 0; stage < FFTLogN; stage++)
            {
                // Compute a multiplier factor for the "twiddle factors".
                // The twiddle factors are complex unit vectors spaced at
                // regular angular intervals. The angle by which the twiddle
                // factor advances depends on the FFT stage. In many FFT
                // implementations the twiddle factors are cached, but because
                // array lookup is relatively slow in C#, it's just
                // as fast to compute them on the fly.
                float wAngleInc = wIndexStep * TAU_INV / (uPointCount);
                float wMulRe = cos(wAngleInc);
                float wMulIm = sin(wAngleInc);

                for (int start = 0; start < pointCount; start += spacing)
                {

                    FFTElement xTop = m_FFTElements[start];
                    FFTElement xBot = m_FFTElements[start + span];

                    float wRe = 1.0f;
                    float wIm = 0.0f;

                    // For each butterfly in this stage
                    for (uint flyCount = 0; flyCount < numFlies; ++flyCount)
                    {
                        // Get the top & bottom values
                        float xTopRe = xTop.re;
                        float xTopIm = xTop.im;
                        float xBotRe = xBot.re;
                        float xBotIm = xBot.im;

                        // Top branch of butterfly has addition
                        xTop.re = xTopRe + xBotRe;
                        xTop.im = xTopIm + xBotIm;

                        // Bottom branch of butterfly has subtraction,
                        // followed by multiplication by twiddle factor
                        xBotRe = xTopRe - xBotRe;
                        xBotIm = xTopIm - xBotIm;
                        xBot.re = xBotRe * wRe - xBotIm * wIm;
                        xBot.im = xBotRe * wIm + xBotIm * wRe;

                        // Copy data back to FFTElements
                        m_FFTElements[xTop.index] = xTop;
                        m_FFTElements[xBot.index] = xBot;


                        // Advance butterfly to next top & bottom positions
                        xTop = m_FFTElements[xTop.next];
                        xBot = m_FFTElements[xBot.next];

                        // Update the twiddle factor, via complex multiply
                        // by unit vector with the appropriate angle
                        // (wRe + j wIm) = (wRe + j wIm) x (wMulRe + j wMulIm)
                        float tRe = wRe;
                        wRe = wRe * wMulRe - wIm * wMulIm;
                        wIm = tRe * wMulIm + wIm * wMulRe;
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

            x = m_FFTElements[0];
            bool end = false;
            while (!end)
            {
                int target = (int)x.revTgt;

                ComplexFloat cf = m_complexFloatsFull[target];
                cf.real = x.re * FFTScale;
                cf.imaginary = x.im * FFTScale;

                m_complexFloatsFull[target] = cf;

                end = x.next == -1;

                if (!end)
                    x = m_FFTElements[x.next];
            }

            // Return 1/2 the FFT result from DC to Fs/2 (The real part of the spectrum)
            int mLengthHalf = m_complexFloats.Length;
            for (int i = 0; i < mLengthHalf; i++)
                m_complexFloats[i] = m_complexFloatsFull[i];

            // DC and Fs/2 Points are scaled differently, since they have only a real part
            m_complexFloats[0] = new ComplexFloat(m_complexFloats[0].real / sqrt(2));
            m_complexFloats[mLengthHalf - 1] = new ComplexFloat(m_complexFloats[mLengthHalf - 1].real / sqrt(2));

        }

    }
}
