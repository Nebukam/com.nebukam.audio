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

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FFTCPreparationJob : IJobParallelFor
    {

        public bool m_recompute;

        public int numSamples;
        public uint FFTLogN;

        public NativeArray<FFTCElement> m_outputFFTElements;

        public void Execute(int index)
        {

            if (!m_recompute) { return; }

            FFTCElement e;
            e = m_outputFFTElements[index];

            e.index = index;
            e.next = index == numSamples - 1 ? -1 : index + 1; // Set up "next" pointers.
            e.revIndex = BitReverse((uint)index, FFTLogN); // Specify target for bit reversal re-ordering.

            m_outputFFTElements[index] = e;

        }

        /// <summary>
        /// Do bit reversal of specified number of places of an int
        /// For example, 1101 bit-reversed is 1011
        /// 
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
