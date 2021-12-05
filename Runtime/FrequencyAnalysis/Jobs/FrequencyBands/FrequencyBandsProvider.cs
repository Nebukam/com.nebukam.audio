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
    }

    [BurstCompile]
    public class FrequencyBandsProvider : Processor<Unemployed>, IFrequencyBandProvider
    {

        protected internal NativeArray<float> m_outputBand8 = new NativeArray<float>(8, Allocator.Persistent);
        protected internal NativeArray<float> m_outputBand16 = new NativeArray<float>(16, Allocator.Persistent);
        protected internal NativeArray<float> m_outputBand32 = new NativeArray<float>(32, Allocator.Persistent);
        protected internal NativeArray<float> m_outputBand64 = new NativeArray<float>(64, Allocator.Persistent);
        protected internal NativeArray<float> m_outputBand128 = new NativeArray<float>(128, Allocator.Persistent);

        public NativeArray<float> outputBand8 { get { return m_outputBand8; } }
        public NativeArray<float> outputBand16 { get { return m_outputBand16; } }
        public NativeArray<float> outputBand32 { get { return m_outputBand32; } }
        public NativeArray<float> outputBand64 { get { return m_outputBand64; } }
        public NativeArray<float> outputBand128 { get { return m_outputBand128; } }

        protected override void InternalLock() { }
        protected override void Prepare(ref Unemployed job, float delta) { }
        protected override void InternalUnlock() { }
        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {
            m_outputBand8.Dispose();
            m_outputBand16.Dispose();
            m_outputBand32.Dispose();
            m_outputBand64.Dispose();
            m_outputBand128.Dispose();
        }

    }
}
