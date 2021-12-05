using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioListenerSpectrum : AbstractSpectrumProvider<Unemployed>
    {

        protected override void FetchSpectrumData()
        {
            AudioListener.GetSpectrumData(m_rawSpectrum, channel, m_FFTWindowType);
        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

    }
}
