using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISingleChannelSamplesProvider : IChannelSamplesProvider
    {
        
    }

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class SingleChannelSamplesProvider : AbstractSamplesProvider<SingleChannelExtractionJob>, ISingleChannelSamplesProvider
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
