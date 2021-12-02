using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class FFTProcessor : ProcessorChain
    {

        protected FFTCoefficients m_FFTCoefficients;
        protected FFTPreparation m_FFTPreparation;
        protected FFTExecution m_FFTExecution;
        protected FFTMagnitudePass m_FFTMagnitudePass;

        public FFTProcessor()
        {
            Add(ref m_FFTCoefficients);
            Add(ref m_FFTPreparation);
            Add(ref m_FFTExecution);
            Add(ref m_FFTMagnitudePass);
        }


    }
}
