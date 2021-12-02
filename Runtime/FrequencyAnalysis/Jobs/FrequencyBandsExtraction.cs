using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FrequencyBandsExtraction : ProcessorGroup
    {

        protected SingleFrequencyBandExtraction m_bandProcessor8;
        protected SingleFrequencyBandExtraction m_bandProcessor16;
        protected SingleFrequencyBandExtraction m_bandProcessor32;
        protected SingleFrequencyBandExtraction m_bandProcessor64;
        protected SingleFrequencyBandExtraction m_bandProcessor128;

        public FrequencyBandsExtraction()
        {
            Add(ref m_bandProcessor8);
            Add(ref m_bandProcessor16);
            Add(ref m_bandProcessor32);
            Add(ref m_bandProcessor64);
            Add(ref m_bandProcessor128);
        }

    }
}
