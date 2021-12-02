using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FrequencyAnalysisChain<TSpectrumProvider> : ProcessorChain
        where TSpectrumProvider : ISpectrumDataProvider, new()
    {

        protected FrequencyAnalysisPreparation<FrequencyFrameDataProvider, TSpectrumProvider> m_frequencyAnalysisPreparation;
        protected FrequencyBandsExtraction m_frequencyBands;
        protected FrequencyFrameReaderProcessor m_frequencyFrameReader;

        public TSpectrumProvider spectrumData { get { return m_frequencyAnalysisPreparation.outputSpectrumProvider; } }

        protected FrameDataDictionary m_frameDataDictionary;
        public FrameDataDictionary frameDataDictionary
        {
            get { return m_frameDataDictionary; }
            set
            {
                m_frameDataDictionary = value;
                m_frequencyAnalysisPreparation.frequencyFrameDataProvider.frames = m_frameDataDictionary.frames;
                m_frequencyFrameReader.inputFrameDataDictionary = m_frameDataDictionary;
            }
        }

        public FrequencyAnalysisChain()
        {
            Add(ref m_frequencyAnalysisPreparation);
            Add(ref m_frequencyBands);
            Add(ref m_frequencyFrameReader);
        }

    }

}
