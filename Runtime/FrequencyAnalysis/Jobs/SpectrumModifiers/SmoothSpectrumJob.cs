using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    [BurstCompile]
    public struct SmoothSpectrumJob : IJobParallelFor, ISpectrumModifierJob
    {

        public void Execute(int index)
        {

        }

    }
}
