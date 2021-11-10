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

        protected SamplingDefinitionList m_list;
        protected Dictionary<String, float> m_data = new Dictionary<string, float>();

        public SamplingDefinitionList list { get { return m_list; } }
        public List<SamplingDefinition> definitions { get { return m_list.Definitions; } }

        public SamplingData(SamplingDefinitionList list)
        {
            m_list = list;
            for(int i = 0, n = list.Definitions.Count; i < n; i++)
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

    }

}
