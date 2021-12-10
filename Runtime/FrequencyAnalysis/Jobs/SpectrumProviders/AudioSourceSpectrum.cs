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
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public class AudioSourceSpectrum : AbstractSpectrumProvider<Unemployed>
    {

        protected AudioSource m_audioSource = null;
        public AudioSource audioSource
        {
            get { return m_audioSource; }
            set { m_audioSource = value; }
        }

        protected override void FetchSpectrumData()
        {
            m_audioSource.GetSpectrumData(m_rawSpectrum, channel, m_FFTWindowType);
        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

#if UNITY_EDITOR

            if (m_audioSource == null)
                throw new System.Exception("AudioSource is null");

            if (m_audioSource.clip == null)
                throw new System.Exception("AudioSource has no clip set");

#endif

            base.Prepare(ref job, delta);

        }

        protected override void InternalLock() { }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

    }
}
