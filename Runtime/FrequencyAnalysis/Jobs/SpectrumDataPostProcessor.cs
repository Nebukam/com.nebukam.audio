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
        public FrequencyBandsProcessor bandsExtraction { get { return m_frequencyBandsExtraction; } }

        protected FrequencyBracketsProcessor m_frequencyBracketsExtraction;
        public FrequencyBracketsProcessor bracketsExtraction { get { return m_frequencyBracketsExtraction; } }

        public SpectrumDataPostProcessor()
        {
            Add(ref m_frequencyBandsExtraction);
            Add(ref m_frequencyBracketsExtraction);
            m_frequencyBracketsExtraction.chunkSize = 1;
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

    }
}
