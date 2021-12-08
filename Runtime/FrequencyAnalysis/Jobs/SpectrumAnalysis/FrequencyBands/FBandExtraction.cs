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
    public class FBandExtraction : ParallelProcessor<FBandExtractionJob>
    {

        protected NativeArray<float> m_outputBands = new NativeArray<float>(0, Allocator.Persistent);
        public NativeArray<float> outputBands { get { return m_outputBands; } }

        protected float[] m_cachedOutput = new float[0];
        public float[] cachedOutput { get { return m_cachedOutput; } }

        protected NativeArray<BandInfos> m_bandInfos = new NativeArray<BandInfos>(0, Allocator.Persistent);
        public NativeArray<BandInfos> bandInfos { get { return m_bandInfos; } }

        protected Bands m_lockedBands = Bands.band8;
        public Bands frequencyBands { get; set; } = Bands.band8;

        protected FrequencyTable m_lockedTable = null;
        public FrequencyTable table { get; set; } = null;

        public ISpectrumProvider spectrumProvider { get; set; } = null;

        protected override void InternalLock() 
        {

            m_lockedBands = frequencyBands;
            int numBands = (int)m_lockedBands;

            bool forceTableUpdate = m_lockedTable != table;
            m_lockedTable = table;

            MakeLength(ref m_outputBands, numBands);
            MakeLength(ref m_cachedOutput, numBands);
            bool u = !MakeLength(ref m_bandInfos, numBands);

            if (u || forceTableUpdate)
                Copy(m_lockedTable.GetBandInfos(m_lockedBands), ref m_bandInfos);

        }

        protected override int Prepare(ref FBandExtractionJob job, float delta)
        {
            job.m_inputBandInfos = m_bandInfos;
            job.m_outputBands = m_outputBands;
            job.m_inputSpectrum = spectrumProvider.outputSpectrum;
            return m_bandInfos.Length;
        }

        protected override void Apply(ref FBandExtractionJob job)
        {
            Copy(m_outputBands, ref m_cachedOutput);
        }

        protected override void InternalDispose() 
        {
            m_outputBands.Dispose();
            m_bandInfos.Dispose();
        }

    }
}
