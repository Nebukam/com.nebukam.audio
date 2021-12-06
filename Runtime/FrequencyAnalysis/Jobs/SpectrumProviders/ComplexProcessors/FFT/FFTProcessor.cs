using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFFT
    {
        Nebukam.Audio.FrequencyAnalysis.FFTWindow window { get; set; }
    }

    [BurstCompile]
    public class FFTProcessor : ProcessorChain, IFFT //, ISpectrumProvider
    {

        protected FFTParams m_FFTparams;
        protected FFTCoefficients m_FFTCoefficients;
        protected FFTScalePass m_FFTScalePass;
        protected FFTPreparation m_FFTPreparation;
        protected FFTExecution m_FFTExecution;
        protected FFTMagnitudePass m_FFTMagnitudePass;

        protected Nebukam.Audio.FrequencyAnalysis.FFTWindow m_window = Nebukam.Audio.FrequencyAnalysis.FFTWindow.Hanning;
        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_window; }
            set { m_window = value; }
        }

        public FFTProcessor()
        {
            Add(ref m_FFTparams);
            Add(ref m_FFTCoefficients);
            Add(ref m_FFTScalePass);
            Add(ref m_FFTPreparation);
            Add(ref m_FFTExecution);
            Add(ref m_FFTMagnitudePass);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
