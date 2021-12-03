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

        protected FrequencyTable m_frequencyTable = null;
        public FrequencyTable frequencyTable
        {
            get { return m_frequencyTable; }
            set
            {
                m_frequencyTable = value;
                m_bandProcessor8.frequencyTable = m_frequencyTable;
                m_bandProcessor16.frequencyTable = m_frequencyTable;
                m_bandProcessor32.frequencyTable = m_frequencyTable;
                m_bandProcessor64.frequencyTable = m_frequencyTable;
                m_bandProcessor128.frequencyTable = m_frequencyTable;
            }
        }

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
