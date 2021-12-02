﻿using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FrequencyFrameReaderProcessor : ParallelProcessor<FrequencyFrameReaderJob>
    {


        protected NativeArray<Sample> m_outputSamples = new NativeArray<Sample>(50, Allocator.Persistent);

        protected FrameDataDictionary m_inputFrameDataDictionary;
        public FrameDataDictionary inputFrameDataDictionary
        {
            get { return m_inputFrameDataDictionary; }
            set { m_inputFrameDataDictionary = value; }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected List<FrequencyFrame> m_lockedFrames;

        IFrequencyFrameDataProvider m_inputFrameDataProvider;
        IFrequencyBandProvider m_inputBandsProvider;

        #endregion

        protected override void InternalLock()
        {

        }

        protected override int Prepare(ref FrequencyFrameReaderJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInGroup(out m_inputFrameDataProvider, true)
                    || !TryGetFirstInGroup(out m_inputBandsProvider, true))
                {
                    throw new System.Exception("FrameData and/or Band provider missing");
                }

                m_inputsDirty = false;

            }

            m_lockedFrames = m_inputFrameDataProvider.lockedFrames;
            int frameCount = m_lockedFrames.Count;

            //TODO : Resize m_sample array only if greater than frame count

            if (frameCount > m_outputSamples.Length)
            {
                m_outputSamples.Dispose();
                m_outputSamples = new NativeArray<Sample>(frameCount * 2, Allocator.Persistent);
            }

            job.m_inputBand8 = m_inputBandsProvider.outputBand8;
            job.m_inputBand16 = m_inputBandsProvider.outputBand16;
            job.m_inputBand32 = m_inputBandsProvider.outputBand32;
            job.m_inputBand64 = m_inputBandsProvider.outputBand64;
            job.m_inputBand128 = m_inputBandsProvider.outputBand128;

            job.m_outputSamples = m_outputSamples;

            return frameCount;

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FrequencyFrameReaderJob job)
        {
            int frameCount = m_lockedFrames.Count;
            for (int i = 0; i < frameCount; i++)
                m_inputFrameDataDictionary.Set(m_lockedFrames[i], m_outputSamples[i]);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_outputSamples.Dispose();
        }

    }
}