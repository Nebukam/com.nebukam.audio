using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.FrequencyAnalysis
{

    public class SamplingData
    {

        protected List<SamplingDefinitionList> m_lists = new List<SamplingDefinitionList>();
        protected Dictionary<String, float> m_data = new Dictionary<string, float>();

        public List<SamplingDefinitionList> lists { get { return m_lists; } }

        public SamplingData()
        {

        }

        public void Add(SamplingDefinitionList list)
        {
            if(m_lists.IndexOf(list) != -1) { return; }

            m_lists.Add(list);
            for (int i = 0, n = list.Definitions.Count; i < n; i++)
            {
                SamplingDefinition def = list.Definitions[i];
                m_data[def.ID] = 0f;
            }

        }

        public float Get(string ID)
        {
            float result = 0f;
            if(m_data.TryGetValue(ID, out result))
            {
                return result;
            }
            else
            {
                return 0f;
            }
        }

        public void Set(string ID, float value)
        {
            m_data[ID] = value;
        }

        public override string ToString()
        {
            string str = "---\n";
            for(int j = 0; j < m_lists.Count; j++)
            {
                SamplingDefinitionList list = m_lists[j];
                for (int i = 0, n = list.Definitions.Count; i < n; i++)
                {
                    SamplingDefinition def = list.Definitions[i];
                    str += "" + def.ID + " = " + m_data[def.ID] + "\n";
                }
            }

            return str;
            
        }

    }

}
