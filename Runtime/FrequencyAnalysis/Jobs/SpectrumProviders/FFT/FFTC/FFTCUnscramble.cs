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

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTCUnscramble : ParallelProcessor<FFTCUnscrambleJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected internal FFTCPreparation m_inputFFTPreparation;

        #endregion

        protected override int Prepare(ref FFTCUnscrambleJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_FFTParams)
                    || !TryGetFirstInCompound(out m_inputFFTPreparation, true))
                {
                    throw new System.Exception("ISamplesProvider or FFTPreparation missing.");
                }

                m_inputsDirty = false;

            }

            job.m_params = m_FFTParams.outputParams;
            job.m_inputComplexFloatsFull = m_inputFFTPreparation.outputComplexSamples;
            job.m_inputFFTElements = m_inputFFTPreparation.outputFFTElements;

            return m_FFTParams.numSamples;

        }

    }

    [BurstCompile]
    public struct FFTCUnscrambleJob : Unity.Jobs.IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> m_params;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<ComplexFloat> m_inputComplexFloatsFull;

        [ReadOnly]
        public NativeArray<FFTCElement> m_inputFFTElements;

        public void Execute(int index)
        {

            float FFTScale = m_params[FFTParams.NATURAL_SCALE];

            FFTCElement ffte = m_inputFFTElements[index];
            m_inputComplexFloatsFull[(int)ffte.revIndex] = new ComplexFloat(ffte.re * FFTScale, ffte.im * FFTScale);

        }

    }

}
