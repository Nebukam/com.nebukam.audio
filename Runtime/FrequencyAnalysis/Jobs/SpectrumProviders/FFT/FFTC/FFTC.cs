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

namespace Nebukam.Audio.FrequencyAnalysis
{

    public class FFTC : FFTransform
    {

        //Preparation
        protected FFTCPreparation m_FFTPreparation;
        protected FFTCPush m_FFTPush;
        //Execution
        protected FFTCExecution m_FFTExecution;
        protected FFTCUnscramble m_FFTUnscramble;
        protected FFTCPrune m_FFTPrune;
        protected FFTCMagnitude m_FFTMagnitudePass;

        public FFTC() 
            : base()
        {
            //Preparation
            Add(ref m_FFTPreparation);
            Add(ref m_FFTPush);
            //Execution
            Add(ref m_FFTExecution);
            Add(ref m_FFTUnscramble);
            Add(ref m_FFTPrune);
            Add(ref m_FFTMagnitudePass);

            Add(new FFTScalePost());
        }

    }
}
