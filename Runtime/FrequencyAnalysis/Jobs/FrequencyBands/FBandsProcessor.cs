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

    public interface IFBandsProcessor : IFBandsProvider, IFBandsExtractionGroup
    {

    }

    public class FBandsProcessor : ProcessorChain, IFBandsProcessor
    {

        protected FBandsProvider m_FBandsProvider;
        protected FBandsExtractionGroup m_FBandsExtractionGroup;

        #region IFBandsProvider

        public NativeArray<float> outputBand8   { get { return m_FBandsProvider.outputBand8; } }
        public NativeArray<float> outputBand16  { get { return m_FBandsProvider.outputBand16; } }
        public NativeArray<float> outputBand32  { get { return m_FBandsProvider.outputBand32; } }
        public NativeArray<float> outputBand64  { get { return m_FBandsProvider.outputBand64; } }
        public NativeArray<float> outputBand128 { get { return m_FBandsProvider.outputBand128; } }

        public NativeArray<BandInfos> outputBandInfos8      { get { return m_FBandsProvider.outputBandInfos8; } }
        public NativeArray<BandInfos> outputBandInfos16     { get { return m_FBandsProvider.outputBandInfos16; } }
        public NativeArray<BandInfos> outputBandInfos32     { get { return m_FBandsProvider.outputBandInfos32; } }
        public NativeArray<BandInfos> outputBandInfos64     { get { return m_FBandsProvider.outputBandInfos64; } }
        public NativeArray<BandInfos> outputBandInfos128    { get { return m_FBandsProvider.outputBandInfos128; } }

        public void Push(Bands bands, ref FBandExtractionJob job){
            m_FBandsProvider.Push(bands, ref job);}

        public NativeArray<float> GetBands(Bands bands){
            return m_FBandsProvider.GetBands(bands);}

        public NativeArray<BandInfos> GetInfos(Bands bands){
            return m_FBandsProvider.GetInfos(bands);}

        #endregion

        #region IFBandsExtractionGroup

        public float[] GetCachedOutput(Bands bands){
            return m_FBandsExtractionGroup.GetCachedOutput(bands);}

        public FBandExtraction GetFBandExtraction(Bands bands){
            return m_FBandsExtractionGroup.GetFBandExtraction(bands);}

        #endregion

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFTableProvider m_frequencyTableProvider;

        #endregion

        public FBandsProcessor()
        {
            Add(ref m_FBandsProvider);
            Add(ref m_FBandsExtractionGroup);
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
