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

using System.Collections.Generic;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public class FrameDataDictionary
    {

        protected List<SpectrumFrame> m_frames = new List<SpectrumFrame>();
        protected Dictionary<SpectrumFrame, Sample> m_dataDic = new Dictionary<SpectrumFrame, Sample>();

        public List<SpectrumFrame> frames { get { return m_frames; } }

        public FrameDataDictionary()
        {

        }

        /// <summary>
        /// Registers a list of FrequencyFrame in this dictionary
        /// to be updated by a FrequencyBandAnalyser
        /// </summary>
        /// <param name="list"></param>
        public void Add(SpectrumFrameList list)
        {
            for (int i = 0, n = list.Frames.Count; i < n; i++)
                Add(list.Frames[i]);
        }

        /// <summary>
        /// Registers a single FrequencyFrame in this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Add(SpectrumFrame frame)
        {
            if (m_frames.TryAddOnce(frame))
            {
                m_dataDic[frame] = new Sample();
            }
        }

        /// <summary>
        /// Removes a single FrequencyFrame from this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Remove(SpectrumFrame frame)
        {
            if (m_frames.TryRemove(frame))
            {
                m_dataDic.Remove(frame);
            }
        }

        /// <summary>
        /// Returns the current Sample value associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Sample Get(SpectrumFrame frame)
        {
            Sample result;

            if (m_dataDic.TryGetValue(frame, out result))
            {
                return result;
            }
            else
            {
                return new Sample();
            }
        }

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(SpectrumFrame frame, out float result)
        {
            Sample s;

            if (m_dataDic.TryGetValue(frame, out s))
            {
                result = s;
                return true;
            }

            result = 0f;
            return false;
        }

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(SpectrumFrame frame, out Sample result)
        {
            return m_dataDic.TryGetValue(frame, out result);
        }

        /// <summary>
        /// Sets the value of a Sample
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="value"></param>
        public void Set(SpectrumFrame frame, Sample value)
        {
            Sample previousSample;
            if (!m_dataDic.TryGetValue(frame, out previousSample) || !previousSample.ON)
                value.justTriggered = true;

            m_dataDic[frame] = value;
        }

        /// <summary>
        /// Clears the lists & data
        /// </summary>
        public void Clear()
        {
            m_frames.Clear();
            m_dataDic.Clear();
        }

        public override string ToString()
        {
            string str = "---\n";

            for (int i = 0; i < m_frames.Count; i++)
            {
                SpectrumFrame frame = m_frames[i];
                str += string.Format("[{0}] {1} = {2}\n", frame.name, frame.ID, m_dataDic[frame]);
            }

            return str;

        }

    }

}
