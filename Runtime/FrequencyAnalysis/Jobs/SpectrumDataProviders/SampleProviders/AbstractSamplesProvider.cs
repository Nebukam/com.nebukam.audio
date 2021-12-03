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

    public interface IChannelSamplesJob : IJobParallelFor
    {
        SpectrumInfos spectrumInfos { set; }
        NativeArray<float> inputRawSamples { set; }
        NativeArray<float> outputSamples { set; }
    }

    public interface IChannelSamplesProvider : IProcessor
    {
        
        Bins frequencyBins { get; set; }
        AudioClip clip { get; set; }
        float time { get; set; }

        SpectrumInfos spectrumInfos { get; }
        NativeArray<float> outputRawSamples { get; }
        NativeArray<float> outputSamples { get; }

    }

    [BurstCompile]
    public abstract class AbstractSamplesProvider<T> : ParallelProcessor<T>, IChannelSamplesProvider 
        where T : struct, IChannelSamplesJob
    {

        protected float[] m_rawSampleData = new float[0];

        protected internal NativeArray<float> m_outputRawSamples = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputRawSamples { get { return m_outputRawSamples; } }

        protected internal NativeArray<float> m_outputSamples = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputSamples { get { return m_outputSamples; } }

        public Bins frequencyBins { get; set; } = Bins.length512;

        protected internal AudioClip m_lockedClip;
        protected internal AudioClip m_clip;
        public AudioClip clip
        {
            get { return m_clip; }
            set { m_clip = value; }
        }

        protected internal float m_lockedTime = 0f;
        protected internal float m_time = 0f;
        public float time
        {
            get { return m_time; }
            set { m_time = value; }
        }

        protected internal SpectrumInfos m_spectrumInfos;
        public SpectrumInfos spectrumInfos{ get { return m_spectrumInfos; } }

        protected override void InternalLock() 
        {
            m_lockedClip = m_clip;
            m_lockedTime = m_time;
        }

        protected override int Prepare(ref T job, float delta)
        {

            m_spectrumInfos = new SpectrumInfos(frequencyBins, m_lockedClip);
            m_spectrumInfos.EnsureCoverage(ref m_rawSampleData);

            MakeLength(ref m_outputSamples, m_spectrumInfos.pointCount);

            m_lockedClip.GetData(m_rawSampleData, spectrumInfos.TimeIndex(m_time));

            Copy(ref m_rawSampleData, ref m_outputRawSamples);

            job.inputRawSamples = m_outputRawSamples;
            job.outputSamples = m_outputSamples;

            return (int)m_spectrumInfos.frequencyBins;

        }

        protected override void Apply(ref T job)
        {

        }

        protected override void InternalUnlock()
        {
            m_lockedClip = null;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_rawSampleData = null;
            m_outputRawSamples.Dispose();
            m_outputSamples.Dispose();
        }

    }
}
