using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyBracketsProcessor : ParallelProcessor<FrequencyBracketsExtractionJob>
    {

        protected NativeArray<BracketData> m_outputBrackets = new NativeArray<BracketData>(0, Allocator.Persistent);
        public NativeArray<BracketData> outputBrackets { get { return m_outputBrackets; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFrequencyTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion
        
        protected override void InternalLock() { }

        protected override int Prepare(ref FrequencyBracketsExtractionJob job, float delta)
        {
            if (m_inputsDirty)
            {

                if(!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider))


                m_inputsDirty = false;

            }
            
            NativeArray<FrequencyRange> ranges = m_frequencyTableProvider.outputRanges;
            int numRanges = ranges.Length;

            MakeLength(ref m_outputBrackets, numRanges);

            job.m_inputSpectrum = m_inputSpectrumProvider.outputSpectrum;
            job.m_inputRanges = ranges;
            job.m_outputBrackets = m_outputBrackets;

            return numRanges;

        }

        protected override void Apply(ref FrequencyBracketsExtractionJob job)
        {
            
        }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() 
        {
            m_outputBrackets.Dispose();
        }

    }
}
