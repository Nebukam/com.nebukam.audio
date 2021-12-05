using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Minimum required elements & jobs to kickstart frequency analysis
    /// </summary>
    /// <typeparam name="T_FRAME_PROVIDER"></typeparam>
    /// <typeparam name="T_SPECTRUM_PROVIDER"></typeparam>
    [BurstCompile]
    public class FrequencyAnalysisPreparation<T_FRAME_PROVIDER, T_SPECTRUM_PROVIDER> : ProcessorGroup, ISpectrumProvider, IFrequencyTableProvider, IFrequencyBandProvider, IFrequencyFrameDataProvider
        where T_FRAME_PROVIDER : class, IFrequencyFrameDataProvider, new()
        where T_SPECTRUM_PROVIDER : class, ISpectrumProvider, new()
    {

        protected T_FRAME_PROVIDER m_frequencyFrameDataProvider;
        public T_FRAME_PROVIDER frequencyFrameDataProvider { get { return m_frequencyFrameDataProvider; } }

        protected FrequencyTableDataProvider m_frequencyTableDataProvider;
        public IFrequencyTableProvider frequencyTableDataProvider { get { return m_frequencyTableDataProvider; } }

        protected T_SPECTRUM_PROVIDER m_spectrumProvider;
        public T_SPECTRUM_PROVIDER spectrumProvider { get { return m_spectrumProvider; } }

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
            get { return m_spectrumProvider.frequencyBins; }
            set { m_spectrumProvider.frequencyBins = value; }
        }

        public NativeArray<float> outputSpectrum { get { return m_spectrumProvider.outputSpectrum; } }

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
            Add(ref m_spectrumProvider); // Provide base spectrum arrays to work with
            Add(ref m_frequencyBandsProvider); // Provide base band arrays for post processor -- this is uneed
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }



    }
}
