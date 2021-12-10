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
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTCoefficients : Processor<FFTCoefficientsJob>
    {

        protected bool m_recompute = true;

        protected Nebukam.Audio.FrequencyAnalysis.FFTWindow m_window =
            Nebukam.Audio.FrequencyAnalysis.FFTWindow.BlackmanHarris;
        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_window; }
            set
            {
                if (m_window == value) { return; }
                m_window = value;
                m_recompute = true;
            }
        }

        protected NativeArray<float> m_outputCoefficients = default;
        public NativeArray<float> outputCoefficients { get { return m_outputCoefficients; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_inputParams;

        #endregion

        protected override void Prepare(ref FFTCoefficientsJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            m_recompute = !MakeLength(ref m_outputCoefficients, m_inputParams.numSamples);

            job.m_recompute = m_recompute;
            job.m_params = m_inputParams.outputParams;
            job.m_windowType = m_window;
            job.m_outputCoefficients = m_outputCoefficients;

            m_recompute = false;

        }

        protected override void Apply(ref FFTCoefficientsJob job)
        {
            m_inputParams.scaleFactor = m_inputParams.outputParams[FFTParams.SCALE_FACTOR];
        }

        protected override void InternalDispose()
        {
            m_outputCoefficients.Dispose();
        }

    }

}
