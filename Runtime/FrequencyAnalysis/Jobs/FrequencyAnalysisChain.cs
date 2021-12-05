﻿using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FrequencyAnalysisChain<TSpectrumProvider> : ProcessorChain
        where TSpectrumProvider : class, ISpectrumProvider, new()
    {

        protected FrequencyAnalysisPreparation<FrequencyFrameDataProvider, TSpectrumProvider> m_frequencyAnalysisPreparation;
        // TODO : Support multiple BandsExtraction processors
        // Note that this is solely to support band extraction.
        // Precise frequence extraction should be done directly in the FrameReader
        // Doesn't even need to be threaded...?
        protected FrequencyBandsExtraction m_frequencyBands; 
        protected FrequencyFrameReaderProcessor m_frequencyFrameReader;

        public TSpectrumProvider spectrumProvider { get { return m_frequencyAnalysisPreparation.spectrumProvider; } }

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

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }

}
