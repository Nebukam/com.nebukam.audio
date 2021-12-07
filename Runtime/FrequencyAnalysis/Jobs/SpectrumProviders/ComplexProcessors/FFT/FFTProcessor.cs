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

    public interface IFFT
    {
        Nebukam.Audio.FrequencyAnalysis.FFTWindow window { get; set; }
    }

    [BurstCompile]
    public class FFTProcessor : ProcessorChain, IFFT //, ISpectrumProvider
    {

        protected FFTParams m_FFTparams;
        protected FFTCoefficients m_FFTCoefficients;
        protected FFTScalePass m_FFTScalePass;
        protected FFTPreparation m_FFTPreparation;
        protected FFTExecution m_FFTExecution;
        protected FFTMagnitudePass m_FFTMagnitudePass;

        protected Nebukam.Audio.FrequencyAnalysis.FFTWindow m_window = Nebukam.Audio.FrequencyAnalysis.FFTWindow.Hanning;
        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_window; }
            set { m_window = value; }
        }

        public FFTProcessor()
        {
            Add(ref m_FFTparams);
            Add(ref m_FFTCoefficients);
            Add(ref m_FFTScalePass);
            Add(ref m_FFTPreparation);
            Add(ref m_FFTExecution);
            Add(ref m_FFTMagnitudePass);
        }

        protected override void InternalLock() { }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

        protected override void InternalDispose() { }

    }
}
