﻿// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using Nebukam.JobAssist;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISamplesExtractionJob : IJobParallelFor
    {
        int inputNumChannels { set; }
        NativeArray<float> inputMultiChannelSamples { set; }
        NativeArray<float> outputSamples { set; }
    }

    public interface ISamplesProvider : IProcessor, ISpectrumProvider
    {

        AudioClip audioClip { get; set; }
        float time { get; set; }

        NativeArray<float> outputMultiChannelSamples { get; }
        NativeArray<float> outputSamples { get; }
        float[] cachedOutput { get; }

    }

    [BurstCompile]
    public abstract class AbstractSamplesProvider<T> : ParallelProcessor<T>, ISamplesProvider
        where T : struct, ISamplesExtractionJob
    {

        #region ISamplesProvider

        protected NativeArray<float> m_outputPrevSpectrum = default;
        public NativeArray<float> outputPrevSpectrum { get { return m_outputPrevSpectrum; } }

        protected NativeArray<float> m_outputSpectrum = default;
        public NativeArray<float> outputSpectrum { get { return m_outputSpectrum; } }

        protected float[] m_cachedOutput = new float[0];
        public float[] cachedOutput{ get { return m_cachedOutput; } }
        
        protected Bins m_frequencyBins = Bins.length512;
        protected int m_numBins = 512;
        protected int m_numSamples = 1024;

        public Bins frequencyBins {
            get { return m_frequencyBins; }
            set { 
                m_frequencyBins = value;
                m_numBins = (int)m_frequencyBins;
                m_numSamples = m_numBins * 2;
            } 
        }

        public int numBins { get { return m_numBins; } }
        public int numSamples { get { return m_numSamples; } }

        #endregion

        protected float[] m_multiChannelSamples = new float[0];

        protected internal NativeArray<float> m_outputMultiChannelSamples = default;
        public NativeArray<float> outputMultiChannelSamples { get { return m_outputMultiChannelSamples; } }

        protected internal NativeArray<float> m_outputSamples = default;
        public NativeArray<float> outputSamples { get { return m_outputSamples; } }

        protected internal AudioClip m_lockedAudioClip;
        protected internal AudioClip m_audioClip;
        public AudioClip audioClip
        {
            get { return m_audioClip; }
            set { m_audioClip = value; }
        }

        protected internal int m_offsetSamples = 0;
        protected internal float m_time = 0f;
        public float time
        {
            get { return m_time; }
            set { m_time = value; }
        }

        protected override void InternalLock()
        {
            m_lockedAudioClip = m_audioClip;
        }

        protected override int Prepare(ref T job, float delta)
        {
#if UNITY_EDITOR
            if (m_lockedAudioClip == null)
            {
                throw new System.Exception("Clip is not set.");
            }
#endif

            m_offsetSamples = (int)((float)m_audioClip.frequency * m_time);

            int numChannels = m_audioClip.channels;
            int multiChannelPointCount = m_numSamples * numChannels;

            if (m_multiChannelSamples == null
                || m_multiChannelSamples.Length != multiChannelPointCount)
                m_multiChannelSamples = new float[multiChannelPointCount];

            m_lockedAudioClip.GetData(m_multiChannelSamples, math.clamp(m_offsetSamples, 0, m_audioClip.samples - multiChannelPointCount));

            MakeLength(ref m_outputSpectrum, m_numBins);
            MakeLength(ref m_outputSamples, m_numSamples);
            Copy(m_multiChannelSamples, ref m_outputMultiChannelSamples);
            Copy(m_outputSpectrum, ref m_outputPrevSpectrum);

            //Debug.Log("m_multiChannelSamples l = "+ m_multiChannelSamples.Length + " / m_offsetSamples = "+ m_offsetSamples + " / m_outputMultiChannelSamples L = "+ m_outputMultiChannelSamples.Length+ " // numChannels = "+ numChannels);

            job.inputNumChannels = numChannels;
            job.inputMultiChannelSamples = m_outputMultiChannelSamples;
            job.outputSamples = m_outputSamples;

            return m_outputSamples.Length;

        }

        protected override void Apply(ref T job)
        {
            Copy(m_outputSpectrum, ref m_cachedOutput);
        }

        protected override void InternalUnlock()
        {
            m_lockedAudioClip = null;
        }

        protected override void InternalDispose()
        {
            m_multiChannelSamples = null;

            m_outputPrevSpectrum.Release();
            m_outputSpectrum.Release();
            m_outputMultiChannelSamples.Release();
            m_outputSamples.Release();
        }

    }
}
