using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrequencyBandProvider : IProcessor
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

    }

    [BurstCompile]
    public class FrequencyBandsProvider : Processor<Unemployed>, IFrequencyBandProvider
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
