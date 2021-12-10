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
using static Unity.Mathematics.math;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FFT4StageChain : ProcessorChain
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected FFTParams m_FFTParams;
        protected ITwiddleFactorsProvider m_twiddleFactorsProvider;
        protected IFFT4PairsProvider m_complexPairsProvider;

        #endregion

        protected override void InternalLock()
        {

            if (m_inputsDirty)
            {
                if(!TryGetFirstInCompound(out m_FFTParams))
                {
                    throw new System.Exception("FFTParams missing");
                }
            }

            int numStages = (m_FFTParams.FFTLogN - 1);
            int diff = numStages - Count;

            //Debug.Log("numStages : " + numStages + " Count : "+ Count + " / diff = " + diff+ " / m_FFTParams.FFTLogN = "+ m_FFTParams.FFTLogN);

            if(diff > 0)
            {
                // Add
                for(int i = 0; i < diff; i++)
                    Add(new FFT4Stage());

            }
            else if(diff < 0)
            {
                // Remove
                diff = abs(diff);

                for(int i = 0; i < diff; i++)
                    m_childs.Pop().Dispose();

            }

        }

        protected override void Prepare(float delta)
        {

            if (m_inputsDirty)
            {

                if(!TryGetFirstInCompound(out m_twiddleFactorsProvider)
                    || !TryGetFirstInCompound(out m_complexPairsProvider))
                {
                    throw new System.Exception("ITwiddleFactorsProvider or IFFT4PairsProvider missing");
                }

                m_inputsDirty = false;

            }

            for (int i = 0, n = Count; i < n; i++)
            {
                FFT4Stage stage = m_childs[i] as FFT4Stage;

                stage.fftParams = m_FFTParams;
                stage.inputComplexPair = m_complexPairsProvider.outputComplexPair;
                stage.inputTwiddleFactors = m_twiddleFactorsProvider.outputFactors;
            }

        }

    }
}
