using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyAnalyser<T_SPECTRUM_PROVIDER> : ProcessorChain
        where T_SPECTRUM_PROVIDER : class, ISpectrumProvider, new()
    {

        protected T_SPECTRUM_PROVIDER m_spectrumProvider;
        public T_SPECTRUM_PROVIDER spectrumProvider { get { return m_spectrumProvider; } }

        protected SpectrumModifierChain m_modifiers;
        public SpectrumModifierChain modifiers { get { return m_modifiers; } }

        #region Analysis distribution

        protected FrequencyAnalysisDistribution m_analysisDistribution;

        public void Add(FrameDataDictionary frameDataDict)
        {
            m_analysisDistribution.Add(frameDataDict);
        }

        public void Remove(FrameDataDictionary frameDataDict)
        {
            m_analysisDistribution.Remove(frameDataDict);
        }

        #endregion

        public FrequencyAnalyser()
        {
            Add(ref m_spectrumProvider);
            Add(ref m_modifiers);
            Add(ref m_analysisDistribution);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta)
        {


        }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
