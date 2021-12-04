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
        SpectrumInfos spectrumInfos { get; }
        NativeArray<float> outputSpectrum { get; }
    }

    [BurstCompile]
    public abstract class AbstractSpectrumProvider<T> : Processor<T>, ISpectrumProvider
        where T : struct, Unity.Jobs.IJob
    {

        protected float[] m_rawSpectrum;

        protected NativeArray<float> m_outputSpectrum = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputSpectrum { get { return m_outputSpectrum; } }

        protected SpectrumInfos m_spectrumInfos;
        public SpectrumInfos spectrumInfos { get { return m_spectrumInfos; } }

        public Bins frequencyBins { get; set; } = Bins.length512;

        protected abstract SpectrumInfos GetSpectrumInfos();

        protected virtual void PrepareRawSpectrum()
        {
            if (m_rawSpectrum == null || m_rawSpectrum.Length != m_spectrumInfos.pointCount)
                m_rawSpectrum = new float[m_spectrumInfos.pointCount];
        }

        protected abstract void FetchSpectrumData();

        protected override void Prepare(ref T job, float delta)
        {

            m_spectrumInfos = GetSpectrumInfos();

            int points = m_spectrumInfos.pointCount;

            if ((points != 0) && ((points & (points - 1)) == 0))
            {
                throw new System.Exception("number of points (" + points + ") is not power of two.");
            }

            PrepareRawSpectrum();
            FetchSpectrumData();

            Copy(ref m_rawSpectrum, ref m_outputSpectrum);

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) { return; }

            m_outputSpectrum.Dispose();
        }

    }
}
