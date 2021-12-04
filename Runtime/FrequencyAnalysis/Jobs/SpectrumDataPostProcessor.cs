using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class SpectrumDataPostProcessor : ProcessorGroup
    {

        protected FrequencyBandsProcessor m_frequencyBandsExtraction;
        protected FrequencyBracketsProcessor m_frequencyBracketsExtraction;

        public SpectrumDataPostProcessor()
        {
            Add(ref m_frequencyBandsExtraction);
            Add(ref m_frequencyBracketsExtraction);
        }

    }
}
