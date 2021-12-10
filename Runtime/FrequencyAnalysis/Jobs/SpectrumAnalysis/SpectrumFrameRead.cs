// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class SpectrumFrameRead : ParallelProcessor<SpectrumFrameReadJob>
    {


        protected NativeArray<Sample> m_outputFrameSamples = new NativeArray<Sample>(50, Allocator.Persistent);

        protected FrameDataDictionary m_inputFrameDataDictionary;
        public FrameDataDictionary inputFrameDataDictionary
        {
            get { return m_inputFrameDataDictionary; }
            set { m_inputFrameDataDictionary = value; }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected List<SpectrumFrame> m_lockedFrames;

        protected ISpectrumFrameProvider m_inputFrameDataProvider;
        protected IFBandsProvider m_inputBandsProvider;

        #endregion

        protected override int Prepare(ref SpectrumFrameReadJob job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputFrameDataProvider, true)
                    || !TryGetFirstInCompound(out m_inputBandsProvider, true))
                {
                    throw new System.Exception("FrameData and/or Band provider missing");
                }

                m_inputsDirty = false;

            }

            m_lockedFrames = m_inputFrameDataProvider.lockedFrames;
            int frameCount = m_lockedFrames.Count;

            EnsureMinLength(ref m_outputFrameSamples, frameCount, frameCount);

            job.m_inputBand8 = m_inputBandsProvider.Get(Bands.band8).outputBands;
            job.m_inputBand16 = m_inputBandsProvider.Get(Bands.band16).outputBands;
            job.m_inputBand32 = m_inputBandsProvider.Get(Bands.band32).outputBands;
            job.m_inputBand64 = m_inputBandsProvider.Get(Bands.band64).outputBands;
            job.m_inputBand128 = m_inputBandsProvider.Get(Bands.band128).outputBands;

            job.m_outputFrameSamples = m_outputFrameSamples;

            return frameCount;

        }

        protected override void Apply(ref SpectrumFrameReadJob job)
        {
            int frameCount = m_lockedFrames.Count;
            for (int i = 0; i < frameCount; i++)
                m_inputFrameDataDictionary.Set(m_lockedFrames[i], m_outputFrameSamples[i]);
        }

        protected override void InternalDispose()
        {
            m_outputFrameSamples.Dispose();
        }

    }
}
