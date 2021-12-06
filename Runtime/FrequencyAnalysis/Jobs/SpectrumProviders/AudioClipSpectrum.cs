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
    public class AudioClipSpectrum<T_SAMPLES_PROVIDER> : ProcessorChain, ISpectrumProvider
        where T_SAMPLES_PROVIDER : class, ISamplesProvider, new()
    {

        protected T_SAMPLES_PROVIDER m_channelSamplesProvider;
        public T_SAMPLES_PROVIDER channelSamplesProvider { get { return m_channelSamplesProvider; } }

        protected FFTProcessor m_FFTProcessor;
        public IFFT FFTProcessor { get { return m_FFTProcessor; } }
        
        public AudioClip audioClip
        {
            get { return m_channelSamplesProvider.audioClip; }
            set { m_channelSamplesProvider.audioClip = value; }
        }

        public float time
        {
            get { return m_channelSamplesProvider.time; }
            set { m_channelSamplesProvider.time = value; }
        }

        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_FFTProcessor.window; }
            set { m_FFTProcessor.window = value; }
        }

        #region ISpectrumProvider

        public Bins frequencyBins { 
            get { return m_channelSamplesProvider.frequencyBins; } 
            set { m_channelSamplesProvider.frequencyBins = value; } 
        }

        public NativeArray<float> outputSpectrum { get { return m_channelSamplesProvider.outputSpectrum; } }

        #endregion

        public AudioClipSpectrum()
        {
            Add(ref m_channelSamplesProvider);
            Add(ref m_FFTProcessor);
        }


        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
