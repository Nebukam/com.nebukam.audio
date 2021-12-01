using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public class FrameDataDictionary
    {

        protected List<FrequencyFrame> m_frames = new List<FrequencyFrame>();
        protected Dictionary<FrequencyFrame, Sample> m_dataDic = new Dictionary<FrequencyFrame, Sample>();

        public List<FrequencyFrame> frames { get { return m_frames; } }

        public FrameDataDictionary()
        {

        }

        /// <summary>
        /// Registers a list of FrequencyFrame in this dictionary
        /// to be updated by a FrequencyBandAnalyser
        /// </summary>
        /// <param name="list"></param>
        public void Add(FrequencyFrameList list)
        {
            for (int i = 0, n = list.Frames.Count; i < n; i++)
                Add(list.Frames[i]);
        }

        /// <summary>
        /// Registers a single FrequencyFrame in this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Add(FrequencyFrame frame)
        {
            if (m_frames.IndexOf(frame) != -1) { return; }
            m_frames.Add(frame);
            m_dataDic[frame] = new Sample();
        }

        /// <summary>
        /// Removes a single FrequencyFrame from this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Remove(FrequencyFrame frame)
        {
            int index = m_frames.IndexOf(frame);
            if (index == -1) { return; }
            m_frames.RemoveAt(index);
            m_dataDic.Remove(frame);
        }

        /// <summary>
        /// Returns the current Sample value associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Sample Get(FrequencyFrame frame)
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
        public bool TryGet(FrequencyFrame frame, out float result)
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
        public bool TryGet(FrequencyFrame frame, out Sample result)
        {
            return m_dataDic.TryGetValue(frame, out result);
        }

        /// <summary>
        /// Sets the value of a Sample
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="value"></param>
        public void Set(FrequencyFrame frame, Sample value)
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
                FrequencyFrame frame = m_frames[i];
                str += string.Format("[{0}] {1} = {2}\n", frame.name, frame.ID, m_dataDic[frame]);
            }

            return str;

        }

    }

}
