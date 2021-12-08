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
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFFT4PermutationsProvider : IProcessor
    {
        NativeArray<int2> outputPermutations { get; }
    }

    public class FFT4PermutationsProvider : Processor<FFT4PermutationsJob>, IFFT4PermutationsProvider
    {

        protected bool m_recompute = true;

        protected NativeArray<int2> m_outputPermutations = new NativeArray<int2>(0, Allocator.Persistent);
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
            m_outputPermutations.Dispose();
        }

    }

}
