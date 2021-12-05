using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTScalePass : ParallelProcessor<FFTScaleJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTCoefficients m_inputFFTCoefficients;
        protected ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override void InternalLock() { }

        protected override int Prepare(ref FFTScaleJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputFFTCoefficients)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider))
                {
                    throw new System.Exception("FFTCoefficients or IChannelSamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            job.m_inputCoefficients = m_inputFFTCoefficients.outputCoefficients;
            job.m_outputSamples = m_inputSamplesProvider.outputSamples;

            return m_inputSamplesProvider.outputSamples.Length;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FFTScaleJob job) { }

        protected override void InternalDispose() { }

    }
}
