using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioListenerSpectrumDataProvider : AbstractSpectrumDataProvider<Unemployed>
    {

        public int channel { get; set; } = 0;

        protected FFTWindow m_FFTWindowType = FFTWindow.Hanning;
        public FFTWindow FFTWindowType
        {
            get { return m_FFTWindowType; }
            set { m_FFTWindowType = value; }
        }

        protected override SpectrumInfos GetSpectrumInfos()
        {
            SpectrumInfos sinfos = new SpectrumInfos();
            sinfos.frequencyBins = frequencyBins;
            sinfos.frequency = 22050; //TODO : Need to verify this 
            sinfos.numSamples = 0;
            sinfos.numChannels = 1;
            sinfos.pointCount = (int)frequencyBins * 2;
            sinfos.coverage = sinfos.pointCount; // We only extract a single channel of data with GetSpectrumData
            sinfos.sampleFrequency = sinfos.frequency / (int)sinfos.frequencyBins;
            return sinfos;
        }

        protected override void FetchSpectrumData()
        {
            AudioListener.GetSpectrumData(m_rawSpectrum, channel, m_FFTWindowType);
        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

    }
}
