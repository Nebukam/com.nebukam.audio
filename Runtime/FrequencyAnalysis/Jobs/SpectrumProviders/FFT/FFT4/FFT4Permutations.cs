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
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFFT4Permutations : IProcessor
    {
        NativeArray<int2> outputPermutations { get; }
    }

    public class FFT4Permutations : Processor<FFT4PermutationsJob>, IFFT4Permutations
    {

        protected bool m_recompute = true;

        protected NativeArray<int2> m_outputPermutations = default;
        public NativeArray<int2> outputPermutations { get { return m_outputPermutations; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected ISpectrumProvider m_spectrumProvider;

        #endregion

        protected override void Prepare(ref FFT4PermutationsJob job, float delta)
        {
            if (m_inputsDirty)
            {
                if(!TryGetFirstInCompound(out m_FFTParams))
                {

                }
            }

            m_recompute = !MakeLength(ref m_outputPermutations, m_FFTParams.numBins);

            job.m_recompute = m_recompute;
            job.m_params = m_FFTParams.outputParams;
            job.m_permutationTable = m_outputPermutations;

        }

        protected override void InternalDispose()
        {
            m_outputPermutations.Release();
        }

    }

    [BurstCompile]
    public struct FFT4PermutationsJob : IJob
    {

        public bool m_recompute;

        [ReadOnly]
        public NativeArray<float> m_params;

        public NativeArray<int2> m_permutationTable;

        public void Execute()
        {

            if (!m_recompute) { return; }

            uint FFTLogN = (uint)m_params[FFTParams.LOG_N];
            int numBins = m_permutationTable.Length;

            for (int i = 0; i < numBins; i++)
            {
                uint firstIndex = (uint)(i * 2),
                    secondIndex = firstIndex + 1;

                m_permutationTable[i] = math.int2((int)BitReverse(firstIndex, FFTLogN), (int)BitReverse(secondIndex, FFTLogN));
            }

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
