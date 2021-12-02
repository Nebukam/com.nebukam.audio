using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ICombineAllChannelSamplesProvider : IChannelSamplesProvider
    {
        
    }

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class CombineAllChannelSamplesProvider : AbstractSamplesProvider<CombineAllChannelsExtractionJob>, ICombineAllChannelSamplesProvider
    {

    }
}
