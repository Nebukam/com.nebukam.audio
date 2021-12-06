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
    public class FFTPreparation : AbstractComplexProvider<FFTPreparationJob>
    {

        protected bool m_recompute = true;

        protected NativeArray<ComplexFloat> m_outputComplexFloatsFull = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> outputComplexFloatsFull { get { return m_outputComplexFloatsFull; } }

        protected NativeArray<FFTElement> m_outputFFTElements = new NativeArray<FFTElement>(0, Allocator.Persistent);
        public NativeArray<FFTElement> outputFFTElements { get { return m_outputFFTElements; } }

        protected internal uint m_outputFFTLogN = 0;
        public uint outputFFTLogN { get { return m_outputFFTLogN; } }

        #region Inputs

        protected FFTParams m_inputParams;
        protected ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override void Prepare(ref FFTPreparationJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputParams)
                    || !TryGetFirstInCompound(out m_inputSamplesProvider, true))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

            }

            int pointCount = m_inputSamplesProvider.outputSamples.Length;

            m_recompute = !MakeLength(ref m_outputFFTElements, pointCount);
            MakeLength(ref m_outputComplexFloatsFull, pointCount);
            
            job.m_recompute = m_recompute;
            job.m_params = m_inputParams.outputParams;
            job.complexFloats = m_outputComplexFloats;
            job.m_outputFFTElements = m_outputFFTElements;

            base.Prepare(ref job, delta);

        }

        protected override void Apply(ref FFTPreparationJob job)
        {

        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_outputComplexFloatsFull.Dispose();
            m_outputFFTElements.Dispose();
        }
    }

}
