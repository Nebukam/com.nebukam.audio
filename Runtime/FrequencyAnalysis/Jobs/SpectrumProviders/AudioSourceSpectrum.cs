using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioSourceSpectrum : AbstractSpectrumProvider<Unemployed>
    {

        protected AudioSource m_audioSource = null;
        public AudioSource audioSource
        {
            get { return m_audioSource; }
            set { m_audioSource = value; }
        }

        protected override void FetchSpectrumData()
        {
            m_audioSource.GetSpectrumData(m_rawSpectrum, channel, m_FFTWindowType);
        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

#if UNITY_EDITOR

            if (m_audioSource == null)
                throw new System.Exception("AudioSource is null");

            if (m_audioSource.clip == null)
                throw new System.Exception("AudioSource has no clip set");

#endif

            base.Prepare(ref job, delta);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

    }
}
