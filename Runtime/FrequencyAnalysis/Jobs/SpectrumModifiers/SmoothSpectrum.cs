using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class SmoothSpectrum : AbstractSpectrumModifierParallel<SmoothSpectrumJob>
    {

        protected override int Prepare(ref SmoothSpectrumJob job, float delta)
        {
            int iterations = base.Prepare(ref job, delta);

            return iterations;
        }

        protected override void InternalDispose() { }

    }
}
