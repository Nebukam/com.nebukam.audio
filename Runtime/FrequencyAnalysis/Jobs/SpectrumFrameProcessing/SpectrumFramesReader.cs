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
using static Nebukam.JobAssist.CollectionsUtils;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class SpectrumFramesReader : ProcessorGroup
    {

        protected bool m_recompute = true;

        public List<SpectrumFrame> m_lockedFrames;
        public List<SpectrumFrame> lockedFrames
        {
            get { return m_lockedFrames; }
            set
            {
                m_lockedFrames = value;
                m_recompute = true;
            }
        }

        protected NativeArray<SpectrumFrameData> m_inputFrameData = default;
        protected NativeArray<Sample> m_outputFrameSamples = default;

        protected ReadBands m_readBands;
        protected ReadBrackets m_readBrackets;
        protected ReadSpectrum m_readSpectrum;

        public SpectrumFramesReader()
        {
            Add(ref m_readBands);
            Add(ref m_readBrackets);
            Add(ref m_readSpectrum);
        }

        protected override void Prepare(float delta)
        {

            if (m_recompute)
            {

                int count = m_lockedFrames.Count;
                MakeLength(ref m_inputFrameData, count);
                MakeLength(ref m_outputFrameSamples, count);

                for (int i = 0; i < count; i++)
                    m_inputFrameData[i] = m_lockedFrames[i];

                m_recompute = false;

            }

            m_readBands.inputFrameData = m_inputFrameData;
            m_readBands.outputFrameSamples = m_outputFrameSamples;

            m_readBrackets.inputFrameData = m_inputFrameData;
            m_readBrackets.outputFrameSamples = m_outputFrameSamples;

            m_readSpectrum.inputFrameData = m_inputFrameData;
            m_readSpectrum.outputFrameSamples = m_outputFrameSamples;

        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_inputFrameData.Dispose();
            m_outputFrameSamples.Dispose();
        }

    }
}
