using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyBracketsExtraction : ParallelProcessor<FrequencyBracketsExtractionJob>
    {

        protected override void InternalLock()
        {
            throw new System.NotImplementedException();
        }

        protected override int Prepare(ref FrequencyBracketsExtractionJob job, float delta)
        {
            throw new System.NotImplementedException();
        }

        protected override void Apply(ref FrequencyBracketsExtractionJob job)
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalUnlock()
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalDispose()
        {
            throw new System.NotImplementedException();
        }

    }
}
