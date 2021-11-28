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

        protected List<FrequencyFrameList> m_lists = new List<FrequencyFrameList>();
        protected Dictionary<String, Sample> m_dataDic = new Dictionary<string, Sample>();

        public List<FrequencyFrameList> lists { get { return m_lists; } }

        public FrameDataDictionary()
        {

        }

        /// <summary>
        /// Registers a list of sampling definition in this SamplingData object
        /// to be updated by a FrequencyBandAnalyser
        /// </summary>
        /// <param name="list"></param>
        public void Add(FrequencyFrameList list)
        {
            if(m_lists.IndexOf(list) != -1) { return; }

            m_lists.Add(list);
            for (int i = 0, n = list.Frames.Count; i < n; i++)
            {
                FrequencyFrame def = list.Frames[i];
                m_dataDic[def.ID] = new Sample();
            }

        }

        /// <summary>
        /// Returns the current Sample value associated with a given ID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Sample Get(string ID)
        {
            Sample result;

            if(m_dataDic.TryGetValue(ID, out result))
            {
                return result;
            }
            else
            {
                return new Sample();
            }
        }

        /// <summary>
        /// Tries to find a Sample associated with a given ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(string ID, out float result)
        {
            Sample s;

            if(m_dataDic.TryGetValue(ID, out s)) 
            {
                result = s;
                return true;
            }

            result = 0f;
            return false;
        }

        /// <summary>
        /// Tries to find a Sample associated with a given ID
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(string ID, out Sample result)
        {
            return m_dataDic.TryGetValue(ID, out result);
        }

        /// <summary>
        /// Sets the value of a Sample
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="value"></param>
        public void Set(string ID, Sample value)
        {
            Sample previousSample;
            if (!m_dataDic.TryGetValue(ID, out previousSample) || !previousSample.ON)
                value.justTriggered = true;

            m_dataDic[ID] = value;
        }

        /// <summary>
        /// Clears the lists & data
        /// </summary>
        public void Clear()
        {
            m_lists.Clear();
            m_dataDic.Clear();
        }

        public override string ToString()
        {
            string str = "---\n";
            for(int j = 0; j < m_lists.Count; j++)
            {
                FrequencyFrameList list = m_lists[j];
                for (int i = 0, n = list.Frames.Count; i < n; i++)
                {
                    FrequencyFrame def = list.Frames[i];
                    str += "" + def.ID + " = " + m_dataDic[def.ID] + "\n";
                }
            }

            return str;
            
        }

    }

}
