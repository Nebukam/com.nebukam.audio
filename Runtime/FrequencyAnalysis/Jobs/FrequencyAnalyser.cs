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
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyAnalyser<T_SPECTRUM_PROVIDER> : ProcessorChain
        where T_SPECTRUM_PROVIDER : class, ISpectrumProvider, new()
    {

        protected T_SPECTRUM_PROVIDER m_spectrumProvider;
        public T_SPECTRUM_PROVIDER spectrumProvider { get { return m_spectrumProvider; } }

        protected SpectrumModifierChain m_modifiers;
        public SpectrumModifierChain modifiers { get { return m_modifiers; } }

        #region Analysis distribution

        protected SpectrumAnalysis m_spectrumAnalysis;
        public SpectrumAnalysis distribution { get { return m_spectrumAnalysis; } }

        public void Add(FrameDataDictionary frameDataDict)
        {
            m_spectrumAnalysis.Add(frameDataDict);
        }

        public void Remove(FrameDataDictionary frameDataDict)
        {
            m_spectrumAnalysis.Remove(frameDataDict);
        }

        #endregion

        public FrequencyAnalyser()
        {
            Add(ref m_spectrumProvider);
            Add(ref m_modifiers);
            Add(ref m_spectrumAnalysis);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta)
        {


        }

        protected override void Apply() 
        {
        
        }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
