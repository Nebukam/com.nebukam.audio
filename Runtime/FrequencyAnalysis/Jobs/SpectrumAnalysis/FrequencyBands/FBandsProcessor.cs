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
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFBandsProvider : IProcessor
    {
        FBandExtraction Get(Bands bands);
    }

    public class FBandsProcessor : ProcessorGroup, IFBandsProvider
    {

        public Bands frequencyBands { get; set; } = Bands.band8;

        protected FBandExtraction 
            m_band8, 
            m_band16, 
            m_band32, 
            m_band64, 
            m_band128;

        public FBandsProcessor()
        {
            Add(ref m_band8); 
            m_band8.chunkSize = 8;
            
            Add(ref m_band16);
            m_band16.chunkSize = 8;

            Add(ref m_band32);
            m_band32.chunkSize = 8;

            Add(ref m_band64);
            m_band64.chunkSize = 8;

            Add(ref m_band128);
            m_band128.chunkSize = 8;
        }

        public FBandExtraction Get(Bands bands)
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


        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion


        protected override void InternalLock() 
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider))
                {
                    throw new System.Exception("IFTableProvider or ISpectrumProvider missing");
                }

                m_inputsDirty = false;

                for (int i = 0, n = Count; i < n; i++)
                    (m_childs[i] as FBandExtraction).spectrumProvider = m_inputSpectrumProvider;

            }

            for(int i = 0, n = Count; i < n; i++)
            {
                FBandExtraction band = m_childs[i] as FBandExtraction;
                band.frequencyBands = FrequencyTable.__bandTypes[i];
                band.table = m_frequencyTableProvider.table;
            }
        }

    }
}
