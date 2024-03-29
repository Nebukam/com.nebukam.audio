﻿// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
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
using static Unity.Mathematics.math;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTCPrune : Processor<FFTCPruneJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected internal ISamplesProvider m_inputSamplesProvider;
        protected internal FFTCPreparation m_inputFFTPreparation = null;

        #endregion

        protected override void Prepare(ref FFTCPruneJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_FFTParams)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider, true)
                    || !TryGetFirstInCompound(out m_inputFFTPreparation, true))
                {
                    throw new System.Exception("ISamplesProvider or FFTPreparation missing.");
                }

                m_inputsDirty = false;

            }

            job.m_params = m_FFTParams.outputParams;
            job.m_inputComplexFloatsFull = m_inputFFTPreparation.outputComplexSamples;
            job.m_outputComplexFloats = m_inputFFTPreparation.outputComplexSpectrum;

        }

    }

    [BurstCompile]
    public struct FFTCPruneJob : Unity.Jobs.IJob
    {

        [ReadOnly]
        public NativeArray<float> m_params;

        public NativeArray<ComplexFloat> m_outputComplexFloats;
        public NativeArray<ComplexFloat> m_inputComplexFloatsFull;

        public void Execute()
        {

            int numBins = (int)m_params[FFTParams.NUM_BINS];
            NativeArray<ComplexFloat>.Copy(m_inputComplexFloatsFull, m_outputComplexFloats, numBins);

            // DC and Fs/2 Points are scaled differently, since they have only a real part
            m_outputComplexFloats[0] = new ComplexFloat(m_outputComplexFloats[0].real / sqrt(2));
            m_outputComplexFloats[numBins - 1] = new ComplexFloat(m_outputComplexFloats[numBins - 1].real / sqrt(2));

        }

    }

}
