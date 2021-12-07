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
    public struct FFTPreparationJob : IJob, IComplexJob
    {

        public bool m_recompute;

        public NativeArray<float> m_params;

        [ReadOnly]
        private NativeArray<ComplexFloat> m_outputComplexFloats;
        public NativeArray<ComplexFloat> complexFloats { set { m_outputComplexFloats = value; } }

        [ReadOnly]
        public NativeArray<float> m_inputSamples;

        public NativeArray<FFTElement> m_outputFFTElements;

        public void Execute()
        {

            if (!m_recompute) { return; }

            uint FFTLogN = 1;
            int pointCount = m_outputFFTElements.Length;
            
            // Find the power of two for the total FFT size up to 2^32
            bool foundIt = false;
            for (FFTLogN = 1; FFTLogN <= 32; FFTLogN++)
            {
                float n = math.pow(2.0f, FFTLogN);
                if (pointCount == n)
                {
                    foundIt = true;
                    break;
                }
            }

#if UNITY_EDITOR

            if (!foundIt)
            {
                throw new System.Exception("FFTElement length was not an even power of 2! FFT cannot continue.");
            }

#endif
            
            //halfLength = (pointCount / 2) + 1;            

            FFTElement e;
            for (int i = 0; i < pointCount; i++)
            {
                e = m_outputFFTElements[i];            
                
                e.index = i;
                e.next = i == pointCount-1 ? -1 : i + 1; // Set up "next" pointers.
                e.revTgt = BitReverse((uint)i, FFTLogN); // Specify target for bit reversal re-ordering.

                m_outputFFTElements[i] = e;
            }

            m_params[FFTParams.LOG_N] = FFTLogN;

        }

        /// <summary>
        /// Do bit reversal of specified number of places of an int
        /// For example, 1101 bit-reversed is 1011
        /// </summary>
        /// <param name="x">Number to be bit-reverse.</param>
        /// <param name="numBits">Number of bits in the number.</param>
        /// <returns></returns>
        private uint BitReverse(uint x, uint numBits)
        {
            uint y = 0;
            for (uint i = 0; i < numBits; i++)
            {
                y <<= 1;
                y |= x & 0x0001;
                x >>= 1;
            }
            return y;
        }

    }
}
