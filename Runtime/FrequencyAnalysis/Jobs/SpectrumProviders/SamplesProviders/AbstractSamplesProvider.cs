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

    public interface ISamplesExtractionJob : IJobParallelFor
    {
        int inputNumChannels { set; }
        NativeArray<float> inputMultiChannelSamples { set; }
        NativeArray<float> outputSamples { set; }
    }

    public interface ISamplesProvider : IProcessor
    {

        Bins frequencyBins { get; set; }
        AudioClip audioClip { get; set; }
        float time { get; set; }

        NativeArray<float> outputMultiChannelSamples { get; }
        NativeArray<float> outputSamples { get; }

    }

    [BurstCompile]
    public abstract class AbstractSamplesProvider<T> : ParallelProcessor<T>, ISamplesProvider
        where T : struct, ISamplesExtractionJob
    {

        protected float[] m_multiChannelSamples = new float[0];

        protected internal NativeArray<float> m_outputMultiChannelSamples = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputMultiChannelSamples { get { return m_outputMultiChannelSamples; } }

        protected internal NativeArray<float> m_outputSamples = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputSamples { get { return m_outputSamples; } }

        public Bins frequencyBins { get; set; } = Bins.length512;

        protected internal AudioClip m_lockedAudioClip;
        protected internal AudioClip m_audioClip;
        public AudioClip audioClip
        {
            get { return m_audioClip; }
            set { m_audioClip = value; }
        }

        protected internal int m_offsetSamples = 0;
        protected internal float m_time = 0f;
        public float time
        {
            get { return m_time; }
            set { m_time = value; }
        }

        protected override void InternalLock()
        {
            m_lockedAudioClip = m_audioClip;
        }

        protected override int Prepare(ref T job, float delta)
        {
#if UNITY_EDITOR
            if (m_lockedAudioClip == null)
            {
                throw new System.Exception("Clip is not set.");
            }
#endif

            m_offsetSamples = (int)((float)m_audioClip.frequency * m_time);

            int numChannels = m_audioClip.channels;
            int pointCount = (int)frequencyBins * 2 * numChannels;
            
            if (m_multiChannelSamples == null
                || m_multiChannelSamples.Length != pointCount)
                m_multiChannelSamples = new float[pointCount];

            m_lockedAudioClip.GetData(m_multiChannelSamples, m_offsetSamples);

            MakeLength(ref m_outputSamples, (int)frequencyBins);
            Copy(ref m_multiChannelSamples, ref m_outputMultiChannelSamples);

            Debug.Log("m_multiChannelSamples l = "+ m_multiChannelSamples.Length + " / m_offsetSamples = "+ m_offsetSamples + " / m_outputMultiChannelSamples L = "+ m_outputMultiChannelSamples.Length+ " // numChannels = "+ numChannels);

            job.inputNumChannels = numChannels;
            job.inputMultiChannelSamples = m_outputMultiChannelSamples;
            job.outputSamples = m_outputSamples;

            return (int)frequencyBins;

        }

        protected override void Apply(ref T job)
        {

        }

        protected override void InternalUnlock()
        {
            m_lockedAudioClip = null;
        }

        protected override void InternalDispose()
        {
            m_multiChannelSamples = null;
            m_outputMultiChannelSamples.Dispose();
            m_outputSamples.Dispose();
        }

    }
}
