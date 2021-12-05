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

        protected internal ISamplesProvider m_inputChannelSamplesProvider;
        protected internal FFTPreparation m_inputFFTPreparation = null;

        #endregion

        protected override void Prepare(ref FFTExecutionJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputChannelSamplesProvider, true)
                    || !TryGetFirstInCompound(out m_inputFFTPreparation, true))
                {
                    throw new System.Exception("IChannelSamplesProvider or FFTPreparation missing.");
                }

            }

            base.Prepare(ref job, delta);


            job.outputComplexFloats = m_inputFFTPreparation.outputComplexFloats;
            job.m_inputComplexFloatsFull = m_inputFFTPreparation.outputComplexFloatsFull;
            job.m_inputFFTElements = m_inputFFTPreparation.outputFFTElements;
            job.m_inputSamples = m_inputChannelSamplesProvider.outputSamples;
            job.m_FFTLogN = m_inputFFTPreparation.outputFFTLogN;

        }

        protected override void Apply(ref FFTExecutionJob job)
        {
            
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_fullLengthFloats.Dispose();
        }

    }

}
