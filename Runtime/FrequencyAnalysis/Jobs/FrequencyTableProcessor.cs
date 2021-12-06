using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyTableProcessor : ProcessorChain
    {

        // - Table index set
        // - Bands & Brackets
        // - Frame reading

        public FrequencyTable table
        {
            get { return m_frequencyTableDataProvider.table; }
            set { m_frequencyTableDataProvider.table = value; }
        }

        protected FrequencyTableDataProvider m_frequencyTableDataProvider;
        
        protected SpectrumDataPostProcessor m_spectrumDataPostProcessor;
        public SpectrumDataPostProcessor spectrumDataPostProcessor { get { return m_spectrumDataPostProcessor; } }


        public FrequencyTableProcessor()
        {
            Add(ref m_frequencyTableDataProvider);
            Add(ref m_spectrumDataPostProcessor);
        }

        
        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
