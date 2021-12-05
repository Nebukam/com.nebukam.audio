using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class SingleFrequencyBandExtraction : Processor<SingleFrequencyBandExtractionJob>
    {

        protected Bands m_referenceBand = Bands.band8;
        public Bands referenceBand
        {
            get { return m_referenceBand; }
            set { m_referenceBand = value; }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFrequencyTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;
        protected IFrequencyBandProvider m_inputBandsProvider;

        #endregion

        protected override void Prepare(ref SingleFrequencyBandExtractionJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputBandsProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider))
                {
                    throw new System.Exception("Missing providers");
                }

                m_inputsDirty = false;

            }

            job.m_inputSpectrum = m_inputSpectrumProvider.outputSpectrum;

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

            m_frequencyTableProvider.frequencyTable.GetBandInfos(out job.m_inputBandInfos, m_referenceBand);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref SingleFrequencyBandExtractionJob job){ }

        protected override void InternalDispose() { }

    }
}
