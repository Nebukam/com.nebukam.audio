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
using Unity.Burst;
using Unity.Collections;
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumProvider : IProcessor
    {
        Bins frequencyBins { get; set; }
        int numBins { get; }
        int numSamples { get; }

        NativeArray<float> outputPrevSpectrum { get; }
        NativeArray<float> outputSpectrum { get; }
    }

    [BurstCompile]
    public abstract class AbstractSpectrumProvider<T> : Processor<T>, ISpectrumProvider
        where T : struct, Unity.Jobs.IJob
    {

        protected float[] m_rawSpectrum;

        protected NativeArray<float> m_outputPrevSpectrum = default;
        public NativeArray<float> outputPrevSpectrum { get { return m_outputPrevSpectrum; } }

        protected NativeArray<float> m_outputSpectrum = default;
        public NativeArray<float> outputSpectrum { get { return m_outputSpectrum; } }

        public int channel { get; set; } = 0;

        protected Bins m_frequencyBins = Bins.length512;
        protected int m_numBins = 512;
        protected int m_numPoints = 1024;

        public Bins frequencyBins
        {
            get { return m_frequencyBins; }
            set
            {
                m_frequencyBins = value;
                m_numBins = (int)m_frequencyBins;
                m_numPoints = m_numBins * 2;
            }
        }

        public int numBins { get { return m_numBins; } }
        public int numSamples { get { return m_numPoints; } }

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
                || m_rawSpectrum.Length != m_numBins)
                m_rawSpectrum = new float[m_numBins];

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
