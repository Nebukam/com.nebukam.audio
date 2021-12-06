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

        protected FrequencyTable m_lockedTable = null;

        protected SingleFrequencyBandExtraction m_bandProcessor8;
        protected SingleFrequencyBandExtraction m_bandProcessor16;
        protected SingleFrequencyBandExtraction m_bandProcessor32;
        protected SingleFrequencyBandExtraction m_bandProcessor64;
        protected SingleFrequencyBandExtraction m_bandProcessor128;

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFrequencyTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;
        protected IFrequencyBandProvider m_inputBandsProvider;

        #endregion

        public FrequencyBandsExtraction()
        {
            Add(ref m_bandProcessor8);
            Add(ref m_bandProcessor16);
            Add(ref m_bandProcessor32);
            Add(ref m_bandProcessor64);
            Add(ref m_bandProcessor128);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) 
        {
            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider)
                    || !TryGetFirstInCompound(out m_inputBandsProvider))
                {
                    throw new System.Exception("IFrequencyBandProvider missing");
                }

                m_inputsDirty = false;

            }

            if(m_lockedTable != m_frequencyTableProvider.table)
            {
                m_lockedTable = m_frequencyTableProvider.table;

                Update(m_bandProcessor8,   m_inputBandsProvider.outputBandInfos8,      Bands.band8);
                Update(m_bandProcessor16,  m_inputBandsProvider.outputBandInfos16,     Bands.band16);
                Update(m_bandProcessor32,  m_inputBandsProvider.outputBandInfos32,     Bands.band32);
                Update(m_bandProcessor64,  m_inputBandsProvider.outputBandInfos64,     Bands.band64);
                Update(m_bandProcessor128, m_inputBandsProvider.outputBandInfos128,    Bands.band128);

            }

            Update(m_bandProcessor8,    Bands.band8);
            Update(m_bandProcessor16,   Bands.band16);
            Update(m_bandProcessor32,   Bands.band32);
            Update(m_bandProcessor64,   Bands.band64);
            Update(m_bandProcessor128,  Bands.band128);

        }

        protected void Update(SingleFrequencyBandExtraction extractor, NativeArray<BandInfos> bandInfos, Bands bands)
        {

            BandInfos[] srcInfos;
            m_lockedTable.GetBandInfos(out srcInfos, bands);
            NativeArray<BandInfos>.Copy(srcInfos, bandInfos);

            extractor.referenceBand = bands;
            extractor.inputBandsInfos = bandInfos;
            extractor.inputBandsProvider = m_inputBandsProvider;
            
        }

        protected void Update(SingleFrequencyBandExtraction extractor, Bands bands)
        {
            extractor.inputSpectrum = m_inputSpectrumProvider.outputSpectrum;
        }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

    }
}
