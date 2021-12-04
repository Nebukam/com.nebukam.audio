using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioSourceSpectrum : AbstractSpectrumProvider<Unemployed>, ISpectrumProvider
    {

        public int channel { get; set; } = 0;

        protected AudioSource m_audioSource = null;
        public AudioSource audioSource
        {
            get { return m_audioSource; }
            set { m_audioSource = value; }
        }

        protected FFTWindow m_FFTWindowType = FFTWindow.Hanning;
        public FFTWindow FFTWindowType
        {
            get { return m_FFTWindowType; }
            set { m_FFTWindowType = value; }
        }

        protected override SpectrumInfos GetSpectrumInfos()
        {
            return new SpectrumInfos(frequencyBins, m_audioSource.clip);
        }

        protected override void FetchSpectrumData()
        {
            m_audioSource.GetSpectrumData(m_rawSpectrum, channel, m_FFTWindowType);
        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

            if (m_audioSource == null)
            {
                throw new System.Exception("AudioSource is null");
            }

            if (m_audioSource.clip == null)
            {
                throw new System.Exception("AudioSource has no clip set");
            }

            base.Prepare(ref job, delta);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

    }
}
