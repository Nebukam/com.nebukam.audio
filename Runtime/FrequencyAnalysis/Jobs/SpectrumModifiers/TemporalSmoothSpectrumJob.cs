﻿using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public struct TemporalSmoothSpectrumJob : IJobParallelFor, ISpectrumModifierJob
    {

        public void Execute(int index)
        {

        }

    }
}
