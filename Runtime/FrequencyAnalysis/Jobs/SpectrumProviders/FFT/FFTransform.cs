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

    public interface IFFTransform : IProcessor
    {
        Nebukam.Audio.FrequencyAnalysis.FFTWindow window { get; set; }
    }
    
    public abstract class FFTransform : ProcessorChain, IFFTransform
    {

        protected FFTParams m_FFTParams;
        protected FFTCoefficients m_FFTCoefficients;
        protected FFTScale m_FFTScalePass;

        public Nebukam.Audio.FrequencyAnalysis.FFTWindow window
        {
            get { return m_FFTParams.window; }
            set { m_FFTParams.window = value; }
        }

        public FFTransform() 
        {
            Add(ref m_FFTParams);
            Add(ref m_FFTCoefficients);
            Add(ref m_FFTScalePass);
        }

    }
}
