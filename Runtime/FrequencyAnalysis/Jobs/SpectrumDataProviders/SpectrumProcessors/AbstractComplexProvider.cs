using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IComplexProvider : IProcessor
    {
        NativeArray<ComplexFloat> outputComplexFloats { get; }
    }

    [BurstCompile]
    public abstract class AbstractComplexProvider<T> : Processor<T>, IComplexProvider
        where T : struct, IJob, IComplexJob
    {

        protected NativeArray<ComplexFloat> m_outputComplexFloats = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> outputComplexFloats { get { return m_outputComplexFloats; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected internal IChannelSamplesProvider m_channelSamplesProvider;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref T job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_channelSamplesProvider))
                {
                    throw new System.Exception("IChannelSamplesProvider missing");
                }

                m_inputsDirty = false;

            }

            int spectrumLength = (int)m_channelSamplesProvider.spectrumInfos.frequencyBins;

            MakeLength(ref m_outputComplexFloats, spectrumLength);

            job.complexFloats = m_outputComplexFloats;

        }

        protected override void InternalUnlock() { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_outputComplexFloats.Dispose();
        }

    }

    public abstract class ComplexProviderParallel<T> : ParallelProcessor<T>
        where T : struct, IJobParallelFor, IComplexJob
    {

        protected NativeArray<ComplexFloat> m_complexFloats = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> complexFloats { get { return m_complexFloats; } }

        protected override void InternalLock() { }

        protected override int Prepare(ref T job, float delta)
        {
            ISpectrumDataProvider spectrumData;
            if (!TryGetFirstInGroup(out spectrumData))
            {
                throw new System.Exception("ISpectrumDataProvider missing");
            }

            int spectrumLength = (int)spectrumData.spectrumInfos.frequencyBins;
            if (m_complexFloats.Length != spectrumLength)
                m_complexFloats = new NativeArray<ComplexFloat>(spectrumLength, Allocator.Persistent);

            job.complexFloats = m_complexFloats;

            return spectrumLength;
        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref T job) { }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_complexFloats.Dispose();
        }

    }
}
