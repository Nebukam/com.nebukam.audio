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
