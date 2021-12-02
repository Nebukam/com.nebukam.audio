using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumDataProvider : IProcessor
    {
        Bins frequencyBins { get; set; }
        SpectrumInfos spectrumInfos { get; }
        NativeArray<float> outputSpectrum { get; }
    }

}
