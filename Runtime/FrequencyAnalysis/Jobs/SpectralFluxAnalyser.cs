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
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    /// <summary>
    /// Spectral Flux only works with an AudioClip source.
    /// </summary>
    /// <typeparam name="T_SAMPLES_PROVIDER"></typeparam>
    public class SpectralFluxAnalyser<T_SAMPLES_PROVIDER> : ProcessorChain
        where T_SAMPLES_PROVIDER : class, ISamplesProvider, new()
    {
        
        protected AudioClipSpectrum<T_SAMPLES_PROVIDER> m_audioClipSpectrum;

        public T_SAMPLES_PROVIDER sampleProvider { get { return m_audioClipSpectrum.samplesProvider; } }



        protected override void InternalLock()
        {
            throw new System.NotImplementedException();
        }

        protected override void Prepare(float delta)
        {
            throw new System.NotImplementedException();
        }

        protected override void Apply()
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalUnlock()
        {
            throw new System.NotImplementedException();
        }

        protected override void InternalDispose()
        {
            throw new System.NotImplementedException();
        }

    }
}
