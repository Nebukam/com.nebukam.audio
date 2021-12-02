using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public interface ISpectrumProcessor : ISpectrumDataProvider
    {
        NativeArray<float> outputSpectrum { get; }
    }
}
