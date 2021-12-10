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
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    /// <summary>
    /// Spectral Flux only works with an AudioClip source.
    /// </summary>
    /// <typeparam name="T_SAMPLES_PROVIDER"></typeparam>
    public class FrequencyAnalyserBulk<T_SAMPLES_PROVIDER, T_FFT> : ProcessorGroup
        where T_SAMPLES_PROVIDER : class, ISamplesProvider, new()
        where T_FFT : class, IFFTransform, new()
    {

        protected int m_lockedBulkSize = 0;
        public int bulkSize { set; get; } = 20;

        protected AudioClip m_lockedAudioClip = null;
        public AudioClip audioClip { get; set; } = null;

        protected float m_lockedTime = 1.0f;
        public float time { get; set; } = 1.0f;

        protected Nebukam.Audio.FrequencyAnalysis.FFTWindow m_lockedWindow = FFTWindow.Hanning;
        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window { get; set; } = FFTWindow.Hanning;

        protected Bins m_lockedFrequencyBins = Bins.length256;
        public Bins frequencyBins { get; set; } = Bins.length256;

        public void Add(FrameDataDictionary frameDataDict)
        {
            FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>> proc = null;
            for (int i = 0, n = Count; i < n; i++)
            {
                proc = this[i] as FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>>;
                //proc.Add(frameDataDict);
            }
        }

        public void Remove(FrameDataDictionary frameDataDict)
        {
            FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>> proc = null;
            for (int i = 0, n = Count; i < n; i++)
            {
                proc = this[i] as FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>>;
                //proc.Remove(frameDataDict);
            }
        }

        protected override void InternalLock()
        {

            m_lockedAudioClip = audioClip;
            m_lockedTime = time;
            m_lockedWindow = window;
            m_lockedFrequencyBins = frequencyBins;

            int oldBulkSize = m_lockedBulkSize;
            m_lockedBulkSize = bulkSize;

            int diff = m_lockedBulkSize - oldBulkSize;
            FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>> proc = null;

            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    proc = null;
                    Add(ref proc);
                }
            }
            else if (diff < 0)
            {
                diff = math.abs(diff);
                for (int i = 0; i < diff; i++)
                {
                    proc = this[Count - 1] as FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>>;
                    Remove(proc);
                    proc.DisposeAll();
                }
            }

            for (int i = 0, n = Count; i < n; i++)
            {
                proc = this[i] as FrequencyAnalyser<AudioClipSpectrum<T_SAMPLES_PROVIDER, T_FFT>>;
                proc.spectrumProvider.time = m_lockedTime;
                proc.spectrumProvider.audioClip = m_lockedAudioClip;
                proc.spectrumProvider.frequencyBins = m_lockedFrequencyBins;
                proc.spectrumProvider.FFTProcessor.window = m_lockedWindow;
            }

        }

    }
}
