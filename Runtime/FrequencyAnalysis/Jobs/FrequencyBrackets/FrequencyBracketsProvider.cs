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

        protected NativeArray<BracketData> m_outputBrackets = new NativeArray<BracketData>(0, Allocator.Persistent);

        #region Inputs

        protected bool m_inputsDirty = true;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref Unemployed job, float delta) 
        {
        

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {
            m_outputBrackets.Dispose();
        }

    }
}
