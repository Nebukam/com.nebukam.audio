using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    
    [BurstCompile]
    public class FFTExecution : AbstractComplexProvider<FFTExecutionJob>
    {

        protected NativeArray<ComplexFloat> m_fullLengthFloats = new NativeArray<ComplexFloat>(0, Allocator.Persistent);

        #region Inputs

        protected bool m_inputsDirty = true;

        protected internal IChannelSamplesProvider m_inputChannelSamplesProvider;
        protected internal FFTPreparation m_inputFFTPreparation = null;

        #endregion

        protected override void Prepare(ref FFTExecutionJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputChannelSamplesProvider, true)
                    || !TryGetFirstInGroup(out m_inputFFTPreparation, true))
                {
                    throw new System.Exception("IChannelSamplesProvider or FFTPreparation missing.");
                }

                m_inputsDirty = false;

            }

            base.Prepare(ref job, delta);

            job.m_complexFloatsFull = m_inputFFTPreparation.outputComplexFloatsFull;
            job.m_FFTElements = m_inputFFTPreparation.outputFFTElements;
            job.FFTLogN = m_inputFFTPreparation.outputFFTLogN;

        }

        protected override void Apply(ref FFTExecutionJob job)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_fullLengthFloats.Dispose();
        }

    }

}
