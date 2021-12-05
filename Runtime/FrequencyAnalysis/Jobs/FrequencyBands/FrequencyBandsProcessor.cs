using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyBandsProcessor : ProcessorChain, IFrequencyBandProvider
    {

        protected FrequencyBandsProvider m_frequencyBandsProvider;
        protected FrequencyBandsExtraction m_frequencyBandsExtraction;

        #region IFrequencyBandProvider

        public NativeArray<float> outputBand8 { get { return m_frequencyBandsProvider.outputBand8; } }
        public NativeArray<float> outputBand16 { get { return m_frequencyBandsProvider.outputBand16; } }
        public NativeArray<float> outputBand32 { get { return m_frequencyBandsProvider.outputBand32; } }
        public NativeArray<float> outputBand64 { get { return m_frequencyBandsProvider.outputBand64; } }
        public NativeArray<float> outputBand128 { get { return m_frequencyBandsProvider.outputBand128; } }

        #endregion

        protected bool m_inputsDirty = true;

        protected IFrequencyTableProvider m_frequencyTableProvider;

        #region Inputs



        #endregion

        public FrequencyBandsProcessor()
        {
            Add(ref m_frequencyBandsProvider);
            Add(ref m_frequencyBandsExtraction);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta)
        {
            // Fetch FrequencyTableDataProvider
            // feed it to frequency band extraction
            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider))
                {

                }

                m_inputsDirty = false;
            }

        }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }



    }
}
