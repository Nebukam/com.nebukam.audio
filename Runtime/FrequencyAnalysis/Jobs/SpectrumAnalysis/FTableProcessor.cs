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

using Nebukam.Collections;
using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrequencyTableProcessor : IProcessorChain
    {

        FrequencyTable table { get; set; }

        FBandsProcessor bandsExtraction { get; }
        FBracketsExtraction bracketsExtraction { get; }

        IFramesReader framesReader { get; }

    }

    public class FTableProcessor : ProcessorChain, IFrequencyTableProcessor
    {

        protected FTableProvider m_frequencyTableProvider;

        protected FBandsProcessor m_frequencyBandsExtraction;
        public FBandsProcessor bandsExtraction { get { return m_frequencyBandsExtraction; } }

        protected FBracketsExtraction m_frequencyBracketsExtraction;
        public FBracketsExtraction bracketsExtraction { get { return m_frequencyBracketsExtraction; } }

        protected SpectrumFramesReader m_spectrumFramesReader;
        public IFramesReader framesReader { get { return m_spectrumFramesReader; } }

        public FrequencyTable table
        {
            get { return m_frequencyTableProvider.table; }
            set {
                m_frequencyTableProvider.table = value;
                m_spectrumFramesReader.table = value;
            }
        }

        public FTableProcessor()
        {

            // Prepare frequency table
            Add(ref m_frequencyTableProvider);

            // Extract data
            Add(ref m_frequencyBandsExtraction);
            Add(ref m_frequencyBracketsExtraction);
            m_frequencyBracketsExtraction.chunkSize = 1;

            // Read data
            Add(ref m_spectrumFramesReader);

        }
    }
}
