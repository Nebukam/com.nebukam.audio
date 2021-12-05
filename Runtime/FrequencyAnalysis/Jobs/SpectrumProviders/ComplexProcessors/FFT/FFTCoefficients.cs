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

        protected float m_outputScaleFactor = 1f;
        public float outputScaleFactor { get { return m_outputScaleFactor; } }

        protected NativeArray<float> m_outputCoefficients = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputCoefficients { get { return m_outputCoefficients; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref FFTCoefficientsJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputSamplesProvider))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            m_recompute = !MakeLength(ref m_outputCoefficients, (int)m_inputSamplesProvider.frequencyBins);

            job.m_windowType = m_window;
            job.m_recompute = m_recompute;
            job.m_outputCoefficients = m_outputCoefficients;
            job.m_recompute = m_recompute;

            m_recompute = false;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FFTCoefficientsJob job)
        {
            m_outputScaleFactor = job.m_scaleFactor;
        }

        protected override void InternalDispose()
        {
            m_outputCoefficients.Dispose();
        }

    }

}
