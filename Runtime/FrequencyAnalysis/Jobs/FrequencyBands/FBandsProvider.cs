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

    public interface IFBandsProvider : IProcessor
    {

        NativeArray<float> outputBand8 { get; }
        NativeArray<float> outputBand16 { get; }
        NativeArray<float> outputBand32 { get; }
        NativeArray<float> outputBand64 { get; }
        NativeArray<float> outputBand128 { get; }

        NativeArray<BandInfos> outputBandInfos8 { get; }
        NativeArray<BandInfos> outputBandInfos16 { get; }
        NativeArray<BandInfos> outputBandInfos32 { get; }
        NativeArray<BandInfos> outputBandInfos64 { get; }
        NativeArray<BandInfos> outputBandInfos128 { get; }

        void Push(Bands bands, ref FBandExtractionJob job);
        NativeArray<float> GetBands(Bands bands);
        NativeArray<BandInfos> GetInfos(Bands bands);
    }

    [BurstCompile]
    public class FBandsProvider : Processor<Unemployed>, IFBandsProvider
    {

        public NativeArray<float> outputBand8 { get; set; }     = new NativeArray<float>(8, Allocator.Persistent);
        public NativeArray<float> outputBand16 { get; set; }    = new NativeArray<float>(16, Allocator.Persistent);
        public NativeArray<float> outputBand32 { get; set; }    = new NativeArray<float>(32, Allocator.Persistent);
        public NativeArray<float> outputBand64 { get; set; }    = new NativeArray<float>(64, Allocator.Persistent);
        public NativeArray<float> outputBand128 { get; set; }   = new NativeArray<float>(128, Allocator.Persistent);

        public NativeArray<BandInfos> outputBandInfos8 { get; set; }    = new NativeArray<BandInfos>(8, Allocator.Persistent);
        public NativeArray<BandInfos> outputBandInfos16 { get; set; }   = new NativeArray<BandInfos>(16, Allocator.Persistent);
        public NativeArray<BandInfos> outputBandInfos32 { get; set; }   = new NativeArray<BandInfos>(32, Allocator.Persistent);
        public NativeArray<BandInfos> outputBandInfos64 { get; set; }   = new NativeArray<BandInfos>(64, Allocator.Persistent);
        public NativeArray<BandInfos> outputBandInfos128 { get; set; }  = new NativeArray<BandInfos>(128, Allocator.Persistent);

        public void Push(Bands bands, ref FBandExtractionJob job)
        {

            switch (bands)
            {
                case Bands.band8:
                    job.m_outputBands = outputBand8;
                    job.m_inputBandInfos = outputBandInfos8;
                    break;
                case Bands.band16:
                    job.m_outputBands = outputBand16;
                    job.m_inputBandInfos = outputBandInfos16;
                    break;
                case Bands.band32:
                    job.m_outputBands = outputBand32;
                    job.m_inputBandInfos = outputBandInfos32;
                    break;
                case Bands.band64:
                    job.m_outputBands = outputBand64;
                    job.m_inputBandInfos = outputBandInfos64;
                    break;
                case Bands.band128:
                    job.m_outputBands = outputBand128;
                    job.m_inputBandInfos = outputBandInfos128;
                    break;
            }

        }

        public NativeArray<float> GetBands(Bands bands)
        {

            switch (bands)
            {
                case Bands.band8:
                    return outputBand8;
                case Bands.band16:
                    return outputBand16;
                case Bands.band32:
                    return outputBand32;
                case Bands.band64:
                    return outputBand64;
                case Bands.band128:
                    return outputBand128;
            }

            return outputBand8;

        }

        public NativeArray<BandInfos> GetInfos(Bands bands)
        {

            switch (bands)
            {
                case Bands.band8:
                    return outputBandInfos8;
                case Bands.band16:
                    return outputBandInfos16;
                case Bands.band32:
                    return outputBandInfos32;
                case Bands.band64:
                    return outputBandInfos64;
                case Bands.band128:
                    return outputBandInfos128;
            }

            return outputBandInfos8;

        }

        protected override void InternalLock() { }
        protected override void Prepare(ref Unemployed job, float delta) { }
        protected override void InternalUnlock() { }
        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {

            outputBand8.Dispose();
            outputBand16.Dispose();
            outputBand32.Dispose();
            outputBand64.Dispose();
            outputBand128.Dispose();

            outputBandInfos8.Dispose();
            outputBandInfos16.Dispose();
            outputBandInfos32.Dispose();
            outputBandInfos64.Dispose();
            outputBandInfos128.Dispose();

        }

    }
}
