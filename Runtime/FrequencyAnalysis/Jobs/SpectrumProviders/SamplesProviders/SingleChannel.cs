using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class SingleChannel : AbstractSamplesProvider<SingleChannelExtractionJob>
    {

        public int channel { get; set; } = 0;

        protected override int Prepare(ref SingleChannelExtractionJob job, float delta)
        {
            int result = base.Prepare(ref job, delta);
            job.channel = channel;
            return result;
        }

    }
}
