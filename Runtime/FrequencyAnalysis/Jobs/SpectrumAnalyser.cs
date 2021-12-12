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

namespace Nebukam.Audio.FrequencyAnalysis
{
    public interface ISpectrumAnalyser : IFrequencyAnalyser, ISpectrumAnalysis
    {
        ISpectrumAnalysis spectrumAnalysis { get; }
    }

    public class SpectrumAnalyser<T_SPECTRUM_PROVIDER> : FrequencyAnalyser<T_SPECTRUM_PROVIDER>, ISpectrumAnalyser
        where T_SPECTRUM_PROVIDER : class, ISpectrumProvider, new()
    {

        protected SpectrumAnalysis m_spectrumAnalysis;
        public ISpectrumAnalysis spectrumAnalysis { get { return m_spectrumAnalysis; } }

        #region ISpectrumAnalysis

        // Frequency tables

        public IFrequencyTableProcessor Add(FrequencyTable table)
        { 
            return m_spectrumAnalysis.Add(table); 
        }

        public IFrequencyTableProcessor Remove(FrequencyTable table)
        { 
            return m_spectrumAnalysis.Remove(table); 
        }

        public IFrequencyTableProcessor this[FrequencyTable table]
        { 
            get { return m_spectrumAnalysis[table]; } 
        }

        public bool TryGetTableProcessor(FrequencyTable table, out IFrequencyTableProcessor processor)
        { 
            return m_spectrumAnalysis.TryGetTableProcessor(table, out processor); 
        }

        // FrameData Dictionaries

        public bool Add(IFrameDataDictionary dictionary)
        { 
            return m_spectrumAnalysis.Add(dictionary); 
        }

        public bool Remove(IFrameDataDictionary dictionary)
        { 
            return m_spectrumAnalysis.Remove(dictionary); 
        }

        public bool cacheFrameData
        {
            get { return m_spectrumAnalysis.cacheFrameData; }
            set { m_spectrumAnalysis.cacheFrameData = value; }
        }

        #endregion

        public SpectrumAnalyser()
            :base()
        {
            Add(ref m_spectrumAnalysis);
        }

    }
}
