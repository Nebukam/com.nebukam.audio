using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class CombineAllChannels : AbstractSamplesProvider<CombineAllChannelsJob>, ISamplesProvider
    {

    }

}
