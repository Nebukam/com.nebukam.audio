using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FrequencyAnalysisPreparation<TFrameDataProvider, TSpectrumDataProvider> : ProcessorGroup, ISpectrumDataProvider
        where TFrameDataProvider : IFrequencyFrameDataProvider, new()
        where TSpectrumDataProvider : ISpectrumDataProvider, new()
    {

        protected TFrameDataProvider m_frequencyFrameDataProvider;
        public TFrameDataProvider frequencyFrameDataProvider { get { return m_frequencyFrameDataProvider; } }

        protected TSpectrumDataProvider m_spectrumDataProvider;
        public TSpectrumDataProvider outputSpectrumProvider { get { return m_spectrumDataProvider; } }

        protected FrequencyBandsProvider m_frequencyBandsProvider;
        public IFrequencyBandProvider frequencyBandsProvider { get { return m_frequencyBandsProvider; } }

        #region IFrequencyFrameDataProvider

        public NativeList<FrequencyFrameData> outputFrameDataList { get { return m_frequencyFrameDataProvider.outputFrameDataList; } }

        public List<FrequencyFrame> lockedFrames { get { return m_frequencyFrameDataProvider.lockedFrames; } }

        #endregion

        #region ISpectrumDataProvider

        public Bins frequencyBins {
            get { return m_spectrumDataProvider.frequencyBins; }
            set { m_spectrumDataProvider.frequencyBins = value; }
        }

        public SpectrumInfos spectrumInfos { get { return m_spectrumDataProvider.spectrumInfos; } }

        public NativeArray<float> outputSpectrum { get { return m_spectrumDataProvider.outputSpectrum; } }

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
            Add(ref m_frequencyFrameDataProvider);
            Add(ref m_spectrumDataProvider);
            Add(ref m_frequencyBandsProvider);
        }


    }
}
