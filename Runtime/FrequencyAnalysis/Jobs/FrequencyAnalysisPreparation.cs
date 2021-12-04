using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Minimum required elements & jobs to kickstart frequency analysis
    /// </summary>
    /// <typeparam name="TFrameDataProvider"></typeparam>
    /// <typeparam name="TSpectrumData"></typeparam>
    [BurstCompile]
    public class FrequencyAnalysisPreparation<TFrameDataProvider, TSpectrumData> : ProcessorGroup, ISpectrumProvider, IFrequencyTableProvider, IFrequencyBandProvider, IFrequencyFrameDataProvider
        where TFrameDataProvider : IFrequencyFrameDataProvider, new()
        where TSpectrumData : ISpectrumProvider, new()
    {

        protected TFrameDataProvider m_frequencyFrameDataProvider;
        public TFrameDataProvider frequencyFrameDataProvider { get { return m_frequencyFrameDataProvider; } }

        protected FrequencyTableDataProvider m_frequencyTableDataProvider;
        public IFrequencyTableProvider frequencyTableDataProvider { get { return m_frequencyTableDataProvider; } }

        protected TSpectrumData m_baseSpectrum;
        public TSpectrumData baseSpectrum { get { return m_baseSpectrum; } }

        protected FrequencyBandsProvider m_frequencyBandsProvider;
        public IFrequencyBandProvider frequencyBandsProvider { get { return m_frequencyBandsProvider; } }

        #region IFrequencyFrameDataProvider

        public NativeList<FrequencyFrameData> outputFrameDataList { get { return m_frequencyFrameDataProvider.outputFrameDataList; } }

        public List<FrequencyFrame> lockedFrames { get { return m_frequencyFrameDataProvider.lockedFrames; } }

        #endregion

        #region IFrequencyTableDataProvider

        public NativeArray<FrequencyRange> outputFrequencyRanges { get { return m_frequencyTableDataProvider.outputFrequencyRanges; } }

        public FrequencyTable frequencyTable
        {
            get { return m_frequencyTableDataProvider.frequencyTable; }
            set { m_frequencyTableDataProvider.frequencyTable = value; }
        }

        #endregion

        #region ISpectrumDataProvider

        public Bins frequencyBins
        {
            get { return m_baseSpectrum.frequencyBins; }
            set { m_baseSpectrum.frequencyBins = value; }
        }

        public SpectrumInfos spectrumInfos { get { return m_baseSpectrum.spectrumInfos; } }

        public NativeArray<float> outputSpectrum { get { return m_baseSpectrum.outputSpectrum; } }

        #endregion

        #region IFrequencyBandProvider

        public NativeArray<float> outputBand8 { get { return m_frequencyBandsProvider.outputBand8; } }
        public NativeArray<float> outputBand16 { get { return m_frequencyBandsProvider.outputBand16; } }
        public NativeArray<float> outputBand32 { get { return m_frequencyBandsProvider.outputBand32; } }
        public NativeArray<float> outputBand64 { get { return m_frequencyBandsProvider.outputBand64; } }
        public NativeArray<float> outputBand128 { get { return m_frequencyBandsProvider.outputBand128; } }

        #endregion

        public FrequencyAnalysisPreparation()
        {
            Add(ref m_frequencyFrameDataProvider); // Provide data for analysis
            Add(ref m_frequencyTableDataProvider); // Provide table definition for framing analysis
            Add(ref m_baseSpectrum); // Provide base spectrum arrays to work with
            Add(ref m_frequencyBandsProvider); // Provide base band arrays for post processor -- this is uneed
        }


    }
}
