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
    public class FFTParams : Processor<Unemployed>
    {

        public const int SCALE_FACTOR = 0;
        public const int LOG_N = 1;
        public const int NUM_BINS = 2;
        public const int NUM_SAMPLES = 3;
        public const int NATURAL_SCALE = 4;

        protected NativeArray<float> m_outputParams = new NativeArray<float>(5, Allocator.Persistent);
        public NativeArray<float> outputParams { get{ return m_outputParams; } }

        public float scaleFactor { get; set; } = 1f;
        public float naturalScale { get; set; } = 1f;
        public int FFTLogN { get; set; } = 1;
        public int numBins { get; set; } = 1;
        public int numSamples { get; set; } = 1;

        protected Nebukam.Audio.FrequencyAnalysis.FFTWindow m_window 
            = Nebukam.Audio.FrequencyAnalysis.FFTWindow.Hanning;
        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_window; }
            set { m_window = value; }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected ISamplesProvider m_samplesProvider;

        #endregion

        protected override void Prepare(ref Unemployed job, float delta) 
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_samplesProvider))
                {
                    throw new System.Exception("ISamplesProvider missing.");
                }

                m_inputsDirty = false;

            }

            numBins = m_samplesProvider.numBins;
            numSamples = m_samplesProvider.numSamples;
            FFTLogN = (int)math.log2(numSamples);
            naturalScale = math.sqrt(2) / (float)numSamples;

            m_outputParams[SCALE_FACTOR] = scaleFactor;
            m_outputParams[NATURAL_SCALE] = naturalScale;
            m_outputParams[LOG_N] = FFTLogN;
            m_outputParams[NUM_BINS] = numBins;
            m_outputParams[NUM_SAMPLES] = numSamples;

        }

        protected override void InternalDispose()
        {
            m_outputParams.Dispose();
        }

    }
}
