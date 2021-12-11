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

using Unity.Burst;
using Unity.Collections;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class CombineChannels : AbstractSamplesProvider<CombineChannelsJob>, ISamplesProvider
    {

        protected NativeArray<int> m_inputChannels = default;

        protected int[] m_channels = new int[0];
        public int[] channels
        {
            get { return m_channels; }
            set { m_channels = value; }
        }

        public void SetChannels(params int[] channelsList)
        {
            m_channels = channelsList;
        }

        protected override int Prepare(ref CombineChannelsJob job, float delta)
        {

            int result = base.Prepare(ref job, delta);

            Copy(m_channels, ref m_inputChannels);
            job.m_inputChannels = m_inputChannels;

            return result;

        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_inputChannels.Release();
        }

    }
}
