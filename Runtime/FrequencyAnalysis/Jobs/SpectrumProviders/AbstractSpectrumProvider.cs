using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumProvider : IProcessor
    {
        Bins frequencyBins { get; set; }
        NativeArray<float> outputSpectrum { get; }
    }

    [BurstCompile]
    public abstract class AbstractSpectrumProvider<T> : Processor<T>, ISpectrumProvider
        where T : struct, Unity.Jobs.IJob
    {

        protected float[] m_rawSpectrum;
        protected NativeArray<float> m_outputSpectrum = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputSpectrum { get { return m_outputSpectrum; } }

        public int channel { get; set; } = 0;

        public Bins frequencyBins { get; set; } = Bins.length512;

        protected UnityEngine.FFTWindow m_FFTWindowType = UnityEngine.FFTWindow.Hanning;
        public UnityEngine.FFTWindow FFTWindowType
        {
            get { return m_FFTWindowType; }
            set { m_FFTWindowType = value; }
        }

        protected abstract void FetchSpectrumData();

        protected override void Prepare(ref T job, float delta)
        {

            if (m_rawSpectrum == null 
                || m_rawSpectrum.Length != (int)frequencyBins)
                m_rawSpectrum = new float[(int)frequencyBins];

            FetchSpectrumData();

            Copy(ref m_rawSpectrum, ref m_outputSpectrum);

        }

        protected override void InternalDispose()
        {
            m_outputSpectrum.Dispose();
        }

    }
}
