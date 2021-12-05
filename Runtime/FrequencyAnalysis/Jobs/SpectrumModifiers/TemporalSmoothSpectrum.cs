using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class TemporalSmoothSpectrum : AbstractSpectrumModifierParallel<TemporalSmoothSpectrumJob>
    {

        protected override int Prepare(ref TemporalSmoothSpectrumJob job, float delta)
        {
            int iterations = base.Prepare(ref job, delta);

            return iterations;
        }

        protected override void InternalDispose() { }

    }
}
