using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class SingleFrequencyBandExtraction : Processor<SingleFrequencyBandJob>
    {

        protected Bands m_referenceBand = Bands.band8;
        public Bands referenceBand
        {
            get { return m_referenceBand; }
            set { m_referenceBand = value; }
        }

        protected FrequencyTable m_frequencyTable = null;
        public FrequencyTable frequencyTable
        {
            get { return m_frequencyTable; }
            set{ m_frequencyTable = value; }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected ISpectrumDataProvider m_inputSpectrumDataProvider;
        protected IFrequencyBandProvider m_inputBandsProvider;

        #endregion

        protected override void Prepare(ref SingleFrequencyBandJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputBandsProvider)
                    || !TryGetFirstInGroup(out m_inputSpectrumDataProvider))
                {
                    throw new System.Exception("Missing providers");
                }

                m_inputsDirty = false;

            }

            job.m_inputSpectrum = m_inputSpectrumDataProvider.outputSpectrum;

            switch (m_referenceBand)
            {
                case Bands.band8:
                    job.m_outputBands = m_inputBandsProvider.outputBand8;
                    break;
                case Bands.band16:
                    job.m_outputBands = m_inputBandsProvider.outputBand16;
                    break;
                case Bands.band32:
                    job.m_outputBands = m_inputBandsProvider.outputBand32;
                    break;
                case Bands.band64:
                    job.m_outputBands = m_inputBandsProvider.outputBand64;
                    break;
                case Bands.band128:
                    job.m_outputBands = m_inputBandsProvider.outputBand128;
                    break;
            }

            job.m_inputBandInfos = FrequencyRanges.GetNativeBandInfos(m_referenceBand);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref SingleFrequencyBandJob job){ }

    }
}
