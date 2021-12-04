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

        public FrequencyBandsProcessor()
        {
            Add(ref m_frequencyBandsProvider);
            Add(ref m_frequencyBandsExtraction);
        }

    }
}
