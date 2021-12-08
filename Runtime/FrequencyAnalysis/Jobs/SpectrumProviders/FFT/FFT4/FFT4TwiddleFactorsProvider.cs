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

    public struct TFactor
    {

        public int2 indices;
        public float2 wings;

        public int odd { get { return indices.x; } }
        public int even { get { return indices.y; } }

        public float4 pair
        {
            get
            {
                return float4(wings.x, math.sqrt(1 - wings.x * wings.x),
                    wings.y, math.sqrt(1 - wings.y * wings.y));
            }
        }

    }

    public interface ITwiddleFactorsProvider : IProcessor 
    {
        NativeArray<TFactor> outputFactors { get; }
    }

    public class FFT4TwiddleFactorsProvider : Processor<FFT4TwiddleFactorJob>, ITwiddleFactorsProvider
    {

        protected bool m_recompute = true;

        protected NativeArray<TFactor> m_twiddleFactors = new NativeArray<TFactor>(0, Allocator.Persistent);
        public NativeArray<TFactor> outputFactors { get { return m_twiddleFactors; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected ISpectrumProvider m_spectrumProvider;

        #endregion

        protected override void Prepare(ref FFT4TwiddleFactorJob job, float delta)
        {
            if (m_inputsDirty)
            {
                if(!TryGetFirstInCompound(out m_FFTParams)
                    || !TryGetFirstInCompound(out m_spectrumProvider))
                {

                }
            }

            int 
                numBins = m_FFTParams.numBins,
                FFTLogN = m_FFTParams.FFTLogN;

            m_recompute = !MakeLength(ref m_twiddleFactors, (FFTLogN - 1) * (numBins / 2));

            job.m_recompute = m_recompute;
            job.m_params = m_FFTParams.outputParams;
            job.m_twiddleFactors = m_twiddleFactors;

        }

        protected override void InternalDispose()
        {
            m_twiddleFactors.Dispose();
        }

    }

}
