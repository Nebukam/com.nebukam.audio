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
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTMagnitudePass : ParallelProcessor<FFTMagnitudeJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_inputParams;
        protected IComplexProvider m_inputComplexProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion

        protected override void InternalLock() { }

        protected override int Prepare(ref FFTMagnitudeJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams)
                    || !TryGetFirstInCompound(out m_inputComplexProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider))
                {
                    throw new System.Exception("FFTCoefficients or IComplexProvider or ISpectrumProvidermissing.");
                }

                m_inputsDirty = false;

            }

            job.m_params = m_inputParams.outputParams;
            job.complexFloats = m_inputComplexProvider.outputComplexFloats;
            job.m_outputSpectrum = m_inputSpectrumProvider.outputSpectrum;

            return m_inputSpectrumProvider.outputSpectrum.Length;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FFTMagnitudeJob job) { }

        protected override void InternalDispose() { }

    }
}
