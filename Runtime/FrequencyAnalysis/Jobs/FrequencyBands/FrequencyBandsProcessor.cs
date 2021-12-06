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

        public NativeArray<float> outputBand8   { get { return m_frequencyBandsProvider.outputBand8; } }
        public NativeArray<float> outputBand16  { get { return m_frequencyBandsProvider.outputBand16; } }
        public NativeArray<float> outputBand32  { get { return m_frequencyBandsProvider.outputBand32; } }
        public NativeArray<float> outputBand64  { get { return m_frequencyBandsProvider.outputBand64; } }
        public NativeArray<float> outputBand128 { get { return m_frequencyBandsProvider.outputBand128; } }

        public NativeArray<BandInfos> outputBandInfos8      { get { return m_frequencyBandsProvider.outputBandInfos8; } }
        public NativeArray<BandInfos> outputBandInfos16     { get { return m_frequencyBandsProvider.outputBandInfos16; } }
        public NativeArray<BandInfos> outputBandInfos32     { get { return m_frequencyBandsProvider.outputBandInfos32; } }
        public NativeArray<BandInfos> outputBandInfos64     { get { return m_frequencyBandsProvider.outputBandInfos64; } }
        public NativeArray<BandInfos> outputBandInfos128    { get { return m_frequencyBandsProvider.outputBandInfos128; } }

        #endregion

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFrequencyTableProvider m_frequencyTableProvider;

        #endregion

        public FrequencyBandsProcessor()
        {
            Add(ref m_frequencyBandsProvider);
            Add(ref m_frequencyBandsExtraction);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta)
        {

        }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }



    }
}
