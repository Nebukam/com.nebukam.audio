using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrequencyBracketProvider : IProcessor
    {
        
    }

    [BurstCompile]
    public class FrequencyBracketsProvider : Processor<Unemployed>, IFrequencyBracketProvider
    {

        protected override void InternalLock() { }
        protected override void Prepare(ref Unemployed job, float delta) { }
        protected override void InternalUnlock() { }
        protected override void Apply(ref Unemployed job) { }

    }
}
