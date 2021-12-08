// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
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
        NativeArray<float> outputPrevSpectrum { get; }
        NativeArray<float> outputSpectrum { get; }
    }

    [BurstCompile]
    public abstract class AbstractSpectrumProvider<T> : Processor<T>, ISpectrumProvider
        where T : struct, Unity.Jobs.IJob
    {

        protected float[] m_rawSpectrum;

        protected NativeArray<float> m_outputPrevSpectrum = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputPrevSpectrum { get { return m_outputPrevSpectrum; } }

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

            int numBins = (int)frequencyBins;

            if (m_rawSpectrum == null 
                || m_rawSpectrum.Length != numBins)
                m_rawSpectrum = new float[numBins];

            FetchSpectrumData();

            Copy(m_outputSpectrum, ref m_outputPrevSpectrum);
            Copy(m_rawSpectrum, ref m_outputSpectrum);

        }

        protected override void InternalDispose()
        {
            m_outputPrevSpectrum.Dispose();
            m_outputSpectrum.Dispose();
        }

    }
}
