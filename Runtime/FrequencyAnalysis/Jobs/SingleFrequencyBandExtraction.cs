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

        protected Bands m_referenceBand = Bands._8;
        public Bands referenceBand
        {
            get { return m_referenceBand; }
            set { m_referenceBand = value; }
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
                case Bands._8:
                    job.m_outputBands = m_inputBandsProvider.outputBand8;
                    break;
                case Bands._16:
                    job.m_outputBands = m_inputBandsProvider.outputBand16;
                    break;
                case Bands._32:
                    job.m_outputBands = m_inputBandsProvider.outputBand32;
                    break;
                case Bands._64:
                    job.m_outputBands = m_inputBandsProvider.outputBand64;
                    break;
                case Bands._128:
                    job.m_outputBands = m_inputBandsProvider.outputBand128;
                    break;
            }

            job.m_inputBandInfos = Octaves.GetNativeBandInfos(m_referenceBand);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref SingleFrequencyBandJob job){ }

    }
}
