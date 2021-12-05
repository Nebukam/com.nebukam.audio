using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyBracketsProcessor : ProcessorChain
    {

        protected FrequencyBracketsProvider m_frequencyBracketsProvider;
        protected FrequencyBracketsExtraction m_frequencyBracketsExtraction;

        public FrequencyBracketsProcessor()
        {
            Add(ref m_frequencyBracketsProvider);
            Add(ref m_frequencyBracketsExtraction);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
