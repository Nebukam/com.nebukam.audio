using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FFTParams : Processor<Unemployed>
    {

        public const int SCALE_FACTOR = 0;
        public const int LOG_N = 1;

        protected NativeArray<float> m_outputParams = new NativeArray<float>(2, Allocator.Persistent);
        public NativeArray<float> outputParams { get{ return m_outputParams; } }

        protected override void InternalLock() { }
        protected override void Prepare(ref Unemployed job, float delta) { }
        protected override void Apply(ref Unemployed job) { }
        protected override void InternalUnlock() { }

        protected override void InternalDispose()
        {
            m_outputParams.Dispose();
        }

    }
}
