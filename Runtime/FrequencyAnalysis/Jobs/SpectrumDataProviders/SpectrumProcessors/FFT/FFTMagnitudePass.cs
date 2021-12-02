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

        protected FFTCoefficients m_inputFFTCoefficients;
        protected IComplexProvider m_inputComplexProvider;
        protected ISpectrumDataProvider m_inputSpectrumDataProvider;

        #endregion

        protected override void InternalLock() { }

        protected override int Prepare(ref FFTMagnitudeJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputFFTCoefficients)
                    || !TryGetFirstInGroup(out m_inputComplexProvider)
                    || !TryGetFirstInGroup(out m_inputSpectrumDataProvider))
                {
                    throw new System.Exception("FFTCoefficients or IChannelSamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            job.m_inputComplexFloats = m_inputComplexProvider.outputComplexFloats;
            job.m_inputScaleFactor = m_inputFFTCoefficients.outputScaleFactor;
            job.m_outputSpectrum = m_inputSpectrumDataProvider.outputSpectrum;

            return m_inputSpectrumDataProvider.outputSpectrum.Length;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FFTMagnitudeJob job) { }

    }
}
