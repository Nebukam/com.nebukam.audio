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
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FFT4SpectrumExtraction : ParallelProcessor<FFT4SpectrumExtractionJob>
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected IFFT4PairsProvider m_complexPairsProvider;
        protected ISamplesProvider m_samplesProvider;
        protected ISpectrumProvider m_spectrumProvider;

        #endregion

        protected override int Prepare(ref FFT4SpectrumExtractionJob job, float delta)
        {

            if (m_inputsDirty)
            {
                
                if (!TryGetFirstInCompound(out m_FFTParams)
                    || !TryGetFirstInCompound(out m_complexPairsProvider)
                    || !TryGetFirstInCompound(out m_samplesProvider)
                    || !TryGetFirstInCompound(out m_spectrumProvider))
                {

                }

                m_inputsDirty = false;

            }

            job.m_params = m_FFTParams.outputParams;
            job.m_inputComplexPair = m_complexPairsProvider.outputComplexPair;
            job.m_scaleFactor = 2.0f / m_FFTParams.numPoints;
            job.m_outputSpectrum = m_spectrumProvider.outputSpectrum;

            return m_FFTParams.numBins/2;

        }

    }

}
