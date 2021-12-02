using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public abstract class RawAudioSpectrumDataProcessor<T> : ProcessorChain, ISpectrumDataProvider
        where T : IChannelSamplesProvider, new()
    {

        public int points { get; set; } = 0;
        public int channel { get; set; } = 0;

        protected T m_channelSamplesProvider;
        protected FFTProcessor m_FFTProcessor;

        public T channelSamplesProvider { get { return m_channelSamplesProvider; } }
        public FFTProcessor FFTProcessor { get { return m_FFTProcessor; } }

        #region ISpectrumDataProvider

        public Bins frequencyBins { 
            get { return m_channelSamplesProvider.frequencyBins; } 
            set { m_channelSamplesProvider.frequencyBins = value; } 
        }
        public SpectrumInfos spectrumInfos{ get { return m_channelSamplesProvider.spectrumInfos; } }
        public NativeArray<float> outputSpectrum { get { return m_channelSamplesProvider.outputSamples; } }

        #endregion

        public RawAudioSpectrumDataProcessor()
        {
            Add(ref m_channelSamplesProvider);
            Add(ref m_FFTProcessor);
        }

    }
}
