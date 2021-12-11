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
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IComplexProvider : IProcessor
    {
        NativeArray<ComplexFloat> outputComplexSpectrum { get; }
    }

    [BurstCompile]
    public class FFTCPreparation : ParallelProcessor<FFTCPreparationJob>, IComplexProvider
    {

        protected bool m_recompute = true;

        protected NativeArray<ComplexFloat> m_outputComplexSpectrum = default;
        public NativeArray<ComplexFloat> outputComplexSpectrum { get { return m_outputComplexSpectrum; } }

        protected NativeArray<ComplexFloat> m_outputComplexSamples = default;
        public NativeArray<ComplexFloat> outputComplexSamples { get { return m_outputComplexSamples; } }

        protected NativeArray<FFTCElement> m_outputFFTElements = default;
        public NativeArray<FFTCElement> outputFFTElements { get { return m_outputFFTElements; } }

        protected internal uint m_outputFFTLogN = 0;
        public uint outputFFTLogN { get { return m_outputFFTLogN; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_inputParams;
        protected ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override int Prepare(ref FFTCPreparationJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            int numSamples = m_inputParams.numSamples;

            m_recompute = !MakeLength(ref m_outputFFTElements, numSamples);
            MakeLength(ref m_outputComplexSpectrum, m_inputParams.numBins);
            MakeLength(ref m_outputComplexSamples, numSamples);
            
            job.m_recompute = m_recompute;
            job.numSamples = numSamples;
            job.FFTLogN = (uint)m_inputParams.FFTLogN;
            job.m_outputFFTElements = m_outputFFTElements;            

            return numSamples;

        }

        protected override void Apply(ref FFTCPreparationJob job)
        {

        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_outputComplexSpectrum.Release();
            m_outputComplexSamples.Release();
            m_outputFFTElements.Release();
        }
    }

}
