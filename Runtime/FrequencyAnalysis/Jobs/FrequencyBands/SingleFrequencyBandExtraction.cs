using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Process a single band group.
    /// !!! NOTE !!!
    /// For the sake of performance, this processor cannot be used in a standalone fashion
    /// e.g it doesn't fetch required inputs on its own and instead requires a parent to set
    /// the right infos.
    /// </summary>
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

        protected NativeArray<BandInfos> m_inputBandInfos;
        public NativeArray<BandInfos> inputBandsInfos
        {
            get { return m_inputBandInfos; }
            set { m_inputBandInfos = value; }
        }

        protected NativeArray<float> m_inputSpectrum;
        public NativeArray<float> inputSpectrum
        {
            get { return m_inputSpectrum; }
            set { m_inputSpectrum = value; }
        }

        protected IFrequencyBandProvider m_inputBandsProvider;
        public IFrequencyBandProvider inputBandsProvider
        {
            get { return m_inputBandsProvider; }
            set { m_inputBandsProvider = value; }
        }

        #endregion

        protected override void Prepare(ref SingleFrequencyBandExtractionJob job, float delta)
        {

            job.m_inputSpectrum = m_inputSpectrum;

            switch (m_referenceBand)
            {
                case Bands.band8:
                    job.m_outputBands = m_inputBandsProvider.outputBand8;
                    job.m_inputBandInfos = m_inputBandsProvider.outputBandInfos8;
                    break;
                case Bands.band16:
                    job.m_outputBands = m_inputBandsProvider.outputBand16;
                    job.m_inputBandInfos = m_inputBandsProvider.outputBandInfos16;
                    break;
                case Bands.band32:
                    job.m_outputBands = m_inputBandsProvider.outputBand32;
                    job.m_inputBandInfos = m_inputBandsProvider.outputBandInfos32;
                    break;
                case Bands.band64:
                    job.m_outputBands = m_inputBandsProvider.outputBand64;
                    job.m_inputBandInfos = m_inputBandsProvider.outputBandInfos64;
                    break;
                case Bands.band128:
                    job.m_outputBands = m_inputBandsProvider.outputBand128;
                    job.m_inputBandInfos = m_inputBandsProvider.outputBandInfos128;
                    break;
            }

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref SingleFrequencyBandExtractionJob job){ }

        protected override void InternalDispose() { }

    }
}
