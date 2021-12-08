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
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    
    [BurstCompile]
    public class FFTCPreparation : AbstractComplexProvider<FFTCPreparationJob>
    {

        protected bool m_recompute = true;

        protected NativeArray<ComplexFloat> m_outputComplexFloatsFull = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> outputComplexFloatsFull { get { return m_outputComplexFloatsFull; } }

        protected NativeArray<FFTCElement> m_outputFFTElements = new NativeArray<FFTCElement>(0, Allocator.Persistent);
        public NativeArray<FFTCElement> outputFFTElements { get { return m_outputFFTElements; } }

        protected internal uint m_outputFFTLogN = 0;
        public uint outputFFTLogN { get { return m_outputFFTLogN; } }

        #region Inputs

        protected FFTParams m_inputParams;
        protected ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override void Prepare(ref FFTCPreparationJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider, true))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

            }

            m_recompute = !MakeLength(ref m_outputFFTElements, m_inputParams.numPoints);
            MakeLength(ref m_outputComplexFloatsFull, m_inputParams.numPoints);
            
            job.m_recompute = m_recompute;
            job.m_params = m_inputParams.outputParams;
            job.complexFloats = m_outputComplexFloats;
            job.m_outputFFTElements = m_outputFFTElements;
            job.m_inputSamples = m_inputSamplesProvider.outputSamples;

            base.Prepare(ref job, delta);

        }

        protected override void Apply(ref FFTCPreparationJob job)
        {

        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_outputComplexFloatsFull.Dispose();
            m_outputFFTElements.Dispose();
        }
    }

}
