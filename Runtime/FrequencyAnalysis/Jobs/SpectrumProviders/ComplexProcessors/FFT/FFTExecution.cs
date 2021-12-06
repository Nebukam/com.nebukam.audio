using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    
    [BurstCompile]
    public class FFTExecution : Processor<FFTExecutionJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_inputParams;
        protected internal ISamplesProvider m_inputSamplesProvider;
        protected internal FFTPreparation m_inputFFTPreparation = null;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref FFTExecutionJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider, true)
                    || !TryGetFirstInCompound(out m_inputFFTPreparation, true))
                {
                    throw new System.Exception("ISamplesProvider or FFTPreparation missing.");
                }

                m_inputsDirty = false;

            }

            job.m_params = m_inputParams.outputParams;
            job.m_inputComplexFloatsFull = m_inputFFTPreparation.outputComplexFloatsFull;
            job.complexFloats = m_inputFFTPreparation.outputComplexFloats;
            job.m_inputFFTElements = m_inputFFTPreparation.outputFFTElements;
            job.m_inputSamples = m_inputSamplesProvider.outputSamples;

        }

        protected override void Apply(ref FFTExecutionJob job)
        {
            
        }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }

}
