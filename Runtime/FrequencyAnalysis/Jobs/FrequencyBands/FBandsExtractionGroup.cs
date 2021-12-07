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
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFBandsExtractionGroup
    {
        float[] GetCachedOutput(Bands bands);
        FBandExtraction GetFBandExtraction(Bands bands);
    }

    [BurstCompile]
    public class FBandsExtractionGroup : ProcessorGroup, IFBandsExtractionGroup
    {

        protected FrequencyTable m_lockedTable = null;

        protected FBandExtraction m_band8;
        protected FBandExtraction m_band16;
        protected FBandExtraction m_band32;
        protected FBandExtraction m_band64;
        protected FBandExtraction m_band128;

        public FBandExtraction band8 { get { return m_band8; } }
        public FBandExtraction band16 { get { return m_band16; } }
        public FBandExtraction band32 { get { return m_band32; } }
        public FBandExtraction band64 { get { return m_band64; } }
        public FBandExtraction band128 { get { return m_band128; } }

        #region IFBandsExtraction

        public FBandExtraction GetFBandExtraction(Bands bands)
        {
            switch (bands)
            {
                case Bands.band8:
                    return m_band8;
                case Bands.band16:
                    return m_band16;
                case Bands.band32:
                    return m_band32;
                case Bands.band64:
                    return m_band64;
                case Bands.band128:
                    return m_band128;
            }

            return m_band8;
        }

        public float[] GetCachedOutput(Bands bands)
        {
            switch (bands)
            {
                case Bands.band8:
                    return m_band8.cachedBandsOutput;
                case Bands.band16:
                    return m_band16.cachedBandsOutput;
                case Bands.band32:
                    return m_band32.cachedBandsOutput;
                case Bands.band64:
                    return m_band64.cachedBandsOutput;
                case Bands.band128:
                    return m_band128.cachedBandsOutput;
            }

            return m_band8.cachedBandsOutput;
        }

        #endregion

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;
        protected IFBandsProvider m_inputBandsProvider;

        #endregion

        public FBandsExtractionGroup()
        {
            Add(ref m_band8);
            Add(ref m_band16);
            Add(ref m_band32);
            Add(ref m_band64);
            Add(ref m_band128);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) 
        {
            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider)
                    || !TryGetFirstInCompound(out m_inputBandsProvider))
                {
                    throw new System.Exception("IFrequencyBandProvider missing");
                }

                m_inputsDirty = false;

            }

            if(m_lockedTable != m_frequencyTableProvider.table)
            {
                m_lockedTable = m_frequencyTableProvider.table;

                Update(m_band8,   m_inputBandsProvider.outputBandInfos8,      Bands.band8);
                Update(m_band16,  m_inputBandsProvider.outputBandInfos16,     Bands.band16);
                Update(m_band32,  m_inputBandsProvider.outputBandInfos32,     Bands.band32);
                Update(m_band64,  m_inputBandsProvider.outputBandInfos64,     Bands.band64);
                Update(m_band128, m_inputBandsProvider.outputBandInfos128,    Bands.band128);

            }

            Update(m_band8,    Bands.band8);
            Update(m_band16,   Bands.band16);
            Update(m_band32,   Bands.band32);
            Update(m_band64,   Bands.band64);
            Update(m_band128,  Bands.band128);

        }

        protected void Update(FBandExtraction extractor, NativeArray<BandInfos> bandInfos, Bands bands)
        {

            NativeArray<BandInfos>.Copy(m_lockedTable.GetBandInfos(bands), bandInfos);

            extractor.referenceBand = bands;
            extractor.inputBandsInfos = bandInfos;
            extractor.inputBandsProvider = m_inputBandsProvider;
            
        }

        protected void Update(FBandExtraction extractor, Bands bands)
        {
            extractor.inputSpectrum = m_inputSpectrumProvider.outputSpectrum;
        }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

    }
}
