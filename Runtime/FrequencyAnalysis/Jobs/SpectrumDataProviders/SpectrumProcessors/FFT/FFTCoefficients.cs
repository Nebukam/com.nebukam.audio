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

        protected FFTWindowType m_FFTType = FFTWindowType.BlackmanHarris;
        public FFTWindowType FFTType
        {
            get { return m_FFTType; }
            set
            {
                if (m_FFTType == value) { return; }
                m_FFTType = value;
                m_recompute = true;
            }
        }

        protected float m_outputScaleFactor = 1f;
        public float outputScaleFactor { get { return m_outputScaleFactor; } }

        protected NativeArray<float> m_outputCoefficients = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputCoefficients { get { return m_outputCoefficients; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        IChannelSamplesProvider m_inputChannelSamplesProvider;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref FFTCoefficientsJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputChannelSamplesProvider))
                {
                    throw new System.Exception("IChannelSamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            int bins = (int)m_inputChannelSamplesProvider.spectrumInfos.frequencyBins;

            m_recompute = !MakeLength(ref m_outputCoefficients, bins);

            job.m_recompute = m_recompute;

            m_recompute = false;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FFTCoefficientsJob job)
        {
            m_outputScaleFactor = job.m_scaleFactor;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_outputCoefficients.Dispose();
        }

    }

}
