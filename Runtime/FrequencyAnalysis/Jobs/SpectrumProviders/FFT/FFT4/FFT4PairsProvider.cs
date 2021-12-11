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

    public interface IFFT4PairsProvider : IProcessor
    {
        NativeArray<float4> outputComplexPair { get; }
    }

    public class FFT4PairsProvider : ParallelProcessor<FFT4PairsJob>, IFFT4PairsProvider
    {

        protected NativeArray<float4> m_outputComplexPair = default;
        public NativeArray<float4> outputComplexPair { get { return m_outputComplexPair; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected ISamplesProvider m_samplesProvider;
        protected IFFT4Permutations m_permutations;

        #endregion

        protected override int Prepare(ref FFT4PairsJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_FFTParams)
                    || !TryGetFirstInCompound(out m_samplesProvider)
                    || !TryGetFirstInCompound(out m_permutations))
                {

                }
            
                m_inputsDirty = false;

            }

            MakeLength(ref m_outputComplexPair, m_FFTParams.numBins);

            job.m_inputSamples = m_samplesProvider.outputSamples;
            job.m_permutationTable = m_permutations.outputPermutations;
            job.m_outputComplexPair = m_outputComplexPair;

            return m_FFTParams.numBins;

        }

        protected override void InternalDispose()
        {
            m_outputComplexPair.Release();
        }

    }

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
