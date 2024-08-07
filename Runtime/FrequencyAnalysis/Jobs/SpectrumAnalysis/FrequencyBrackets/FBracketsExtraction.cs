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
using Unity.Collections;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFBracketsProvider : IProcessor
    {
        NativeArray<BracketData> outputBrackets { get; }
        BracketData[] cachedOutput { get; }
    }

    public class FBracketsExtraction : ParallelProcessor<FBracketsExtractionJob>, IFBracketsProvider
    {

        protected NativeArray<BracketData> m_outputBrackets = default;
        public NativeArray<BracketData> outputBrackets { get { return m_outputBrackets; } }

        protected BracketData[] m_cachedOutput = new BracketData[0];
        public BracketData[] cachedOutput { get { return m_cachedOutput; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFTableProvider m_frequencyTableProvider;
        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion

        protected override void InternalLock() { }

        protected override int Prepare(ref FBracketsExtractionJob job, float delta)
        {
            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_frequencyTableProvider)
                    || !TryGetFirstInCompound(out m_inputSpectrumProvider))


                    m_inputsDirty = false;

            }

            NativeArray<FrequencyRange> ranges = m_frequencyTableProvider.outputRanges;
            int numRanges = ranges.Length;

            MakeLength(ref m_outputBrackets, numRanges);
            MakeLength(ref m_cachedOutput, numRanges);

            job.m_inputSpectrum = m_inputSpectrumProvider.outputSpectrum;
            job.m_inputRanges = ranges;
            job.m_outputBrackets = m_outputBrackets;

            return numRanges;

        }

        protected override void Apply(ref FBracketsExtractionJob job)
        {
            Copy(m_outputBrackets, ref m_cachedOutput);
        }

        protected override void InternalDispose()
        {
            m_outputBrackets.Release();
        }

    }
}
