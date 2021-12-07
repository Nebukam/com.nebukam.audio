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
using static Nebukam.JobAssist.CollectionsUtils;
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
    public class FBandExtraction : Processor<FBandExtractionJob>
    {

        protected float[] m_cachedBandsOutput = new float[0];
        public float[] cachedBandsOutput { get { return m_cachedBandsOutput; } }

        protected Bands m_referenceBand = Bands.band8;
        public Bands referenceBand
        {
            get { return m_referenceBand; }
            set { 
                m_referenceBand = value;
                MakeLength(ref m_cachedBandsOutput, (int)value);
            }
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

        protected IFBandsProvider m_inputBandsProvider;
        public IFBandsProvider inputBandsProvider
        {
            get { return m_inputBandsProvider; }
            set { m_inputBandsProvider = value; }
        }

        #endregion

        protected override void Prepare(ref FBandExtractionJob job, float delta)
        {
            job.m_referenceBands = m_referenceBand;
            job.m_inputSpectrum = m_inputSpectrum;
            m_inputBandsProvider.Push(m_referenceBand, ref job);
        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref FBandExtractionJob job)
        {
            Copy(job.m_outputBands, ref m_cachedBandsOutput);
        }

        protected override void InternalDispose() { }

    }
}
