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
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT> : ProcessorChain, ISpectrumProvider
        where T_SAMPLES_PROVIDER : class, ISamplesProvider, new()
        where T_FFT : class, IFFTransform, new()
    {

        protected T_SAMPLES_PROVIDER m_samplesProvider;
        public T_SAMPLES_PROVIDER samplesProvider { get { return m_samplesProvider; } }

        protected T_FFT m_FFTransform;
        public IFFTransform FFTProcessor { get { return m_FFTransform; } }
        
        public AudioClip audioClip
        {
            get { return m_samplesProvider.audioClip; }
            set { m_samplesProvider.audioClip = value; }
        }

        public float time
        {
            get { return m_samplesProvider.time; }
            set { m_samplesProvider.time = value; }
        }

        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_FFTransform.window; }
            set { m_FFTransform.window = value; }
        }

        #region ISpectrumProvider

        public Bins frequencyBins { 
            get { return m_samplesProvider.frequencyBins; } 
            set { m_samplesProvider.frequencyBins = value; } 
        }

        public int numBins { get { return m_samplesProvider.numBins; } }
        public int numSamples { get { return m_samplesProvider.numSamples; } }

        public NativeArray<float> outputPrevSpectrum { get { return m_samplesProvider.outputPrevSpectrum; } }
        public NativeArray<float> outputSpectrum { get { return m_samplesProvider.outputSpectrum; } }
        public float[] cachedOutput { get { return m_samplesProvider.cachedOutput; } }

        #endregion

        public AudioClipSpectrum()
        {
            Add(ref m_samplesProvider);
            Add(ref m_FFTransform);
        }

    }
}
