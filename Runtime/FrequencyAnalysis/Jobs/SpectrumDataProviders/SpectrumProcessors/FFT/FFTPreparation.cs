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

        protected bool m_inputsDirty = true;

        IChannelSamplesProvider m_inputChannelSamplesProvider;
        FFTCoefficients m_inputFFTCoefficients;

        #endregion

        protected override void Prepare(ref FFTPreparationJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputChannelSamplesProvider, true)
                    || !TryGetFirstInGroup(out m_inputFFTCoefficients, true))
                {
                    throw new System.Exception("IChannelSamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            int channelDataLength = m_inputChannelSamplesProvider.spectrumInfos.pointCount;

            MakeLength(ref m_outputComplexFloatsFull, channelDataLength);
            MakeLength(ref m_outputFFTElements, channelDataLength);

            job.m_outputFFTElements = m_outputFFTElements;

            base.Prepare(ref job, delta);

        }

        protected override void Apply(ref FFTPreparationJob job)
        {
            m_outputFFTLogN = job.m_outputFFTLogN;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_outputComplexFloatsFull.Dispose();
            m_outputFFTElements.Dispose();
        }
    }

}
