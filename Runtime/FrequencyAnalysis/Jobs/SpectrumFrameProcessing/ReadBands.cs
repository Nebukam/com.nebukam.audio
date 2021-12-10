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
    public class ReadBands : AbstractSFrameReader<ReadBandsJob>
    {

        #region Inputs

        protected IFBandsProvider m_bandsProvider;

        #endregion

        protected override int Prepare(ref ReadBandsJob job, float delta)
        {
            
            if (m_inputsDirty)
            {
                if(!TryGetFirstInCompound(out m_bandsProvider))
                {
                    throw new System.Exception("IFBandsProvider missing.");
                }
            }

            job.m_inputBands8 = m_bandsProvider.Get(Bands.band8).outputBands;
            job.m_inputBands16 = m_bandsProvider.Get(Bands.band16).outputBands;
            job.m_inputBands32 = m_bandsProvider.Get(Bands.band32).outputBands;
            job.m_inputBands64 = m_bandsProvider.Get(Bands.band64).outputBands;
            job.m_inputBands128 = m_bandsProvider.Get(Bands.band128).outputBands;

            return base.Prepare(ref job, delta);
        }

    }
}
