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
using UnityEngine.Audio;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class MicrophoneSpectrum<T_FFT> : ProcessorChain, ISpectrumProvider
        where T_FFT : class, IFFTransform, new()
    {

        protected SingleChannel m_samplesProvider;
        public SingleChannel samplesProvider { get { return m_samplesProvider; } }

        protected T_FFT m_FFTransform;
        public IFFTransform FFTProcessor { get { return m_FFTransform; } }

        protected AudioClip m_audioClip = null;
        public AudioClip audioClip { get { return m_audioClip; } }

        public float time
        {
            get { return m_samplesProvider.time; }
            set { m_samplesProvider.time = value; }
        }

        protected int m_minFreq = 0;
        protected int m_maxFreq = 0;

        protected string m_deviceName = null;
        public string deviceName
        {
            get { return m_deviceName; }
            set {
                
                if (m_deviceName == value)
                    return;

                m_deviceName = value;

                if(m_deviceName == null)
                {
                    AudioClip.Destroy(m_audioClip);
                    m_audioClip = null;
                }
                else
                {
                    Microphone.GetDeviceCaps(m_deviceName, out m_minFreq, out m_maxFreq);
                    m_audioClip = Microphone.Start(m_deviceName, true, 1, m_maxFreq);
                }

                m_samplesProvider.audioClip = m_audioClip;

            }
        }

        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_FFTransform.window; }
            set { m_FFTransform.window = value; }
        }

        #region ISpectrumProvider

        public Bins frequencyBins
        {
            get { return m_samplesProvider.frequencyBins; }
            set { m_samplesProvider.frequencyBins = value; }
        }

        public int numBins { get { return m_samplesProvider.numBins; } }
        public int numSamples { get { return m_samplesProvider.numSamples; } }

        public NativeArray<float> outputPrevSpectrum { get { return m_samplesProvider.outputPrevSpectrum; } }

        public NativeArray<float> outputSpectrum { get { return m_samplesProvider.outputSpectrum; } }

        #endregion

        public MicrophoneSpectrum()
        {

            Add(ref m_samplesProvider);
            Add(ref m_FFTransform);

        }

        protected override void Prepare(float delta)
        {

            float _time = 0f;

            if(m_audioClip != null)
            {
                // Offset the sampling time by point counts to fetch live audio
                _time = (float)(Microphone.GetPosition(m_deviceName) - m_samplesProvider.numSamples) / (float)m_maxFreq;
            }

            m_samplesProvider.time = _time;
        }

    }
}
