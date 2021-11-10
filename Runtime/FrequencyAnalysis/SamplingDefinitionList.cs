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

    [CreateAssetMenu(fileName = "Sampling Definitions", menuName = "FrequencyAnalysis/Definitions", order = 1)]
    public class SamplingDefinitionList : ScriptableObject
    {

        [Header("Data")]
        [Tooltip("List of definitions")]
        public List<SamplingDefinition> Definitions = new List<SamplingDefinition>();

        private void OnValidate()
        {
            SamplingDefinition def;
            for (int i = 0, n = Definitions.Count; i < n; i++)
            {
                def = Definitions[i];
                SamplingDefinition.Sanitize(ref def);
                Definitions[i] = def;
            }
        }

    }

}
