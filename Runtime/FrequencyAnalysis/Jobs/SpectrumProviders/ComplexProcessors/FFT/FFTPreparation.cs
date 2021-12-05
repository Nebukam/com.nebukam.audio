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

        // Need a complex array the size of the sample points to feed the complex spectrum
        protected NativeArray<ComplexFloat> m_outputComplexFloatsFull = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        protected NativeArray<FFTElement> m_outputFFTElements = new NativeArray<FFTElement>(0, Allocator.Persistent);

        public NativeArray<ComplexFloat> outputComplexFloatsFull { get { return m_outputComplexFloatsFull; } }
        public NativeArray<FFTElement> outputFFTElements { get { return m_outputFFTElements; } }

        protected internal uint m_outputFFTLogN = 0;
        public uint outputFFTLogN { get { return m_outputFFTLogN; } }

        #region Inputs

        ISamplesProvider m_inputSamplesProvider;

        #endregion

        protected override void Prepare(ref FFTPreparationJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputSamplesProvider, true))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

            }

            int pointCount = m_inputSamplesProvider.outputMultiChannelSamples.Length;

            MakeLength(ref m_outputComplexFloatsFull, pointCount);
            MakeLength(ref m_outputFFTElements, pointCount);

            job.m_inputComplexFloats = m_outputComplexFloatsFull;
            job.m_outputFFTElements = m_outputFFTElements;

            base.Prepare(ref job, delta);

        }

        protected override void Apply(ref FFTPreparationJob job)
        {
            m_outputFFTLogN = job.m_outputFFTLogN;
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_outputComplexFloatsFull.Dispose();
            m_outputFFTElements.Dispose();
        }
    }

}
