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

    [CreateAssetMenu(fileName = "Frequency Frame List", menuName = "N:Toolkit/Audio/Frequency Frame List", order = 0)]
    public class FrequencyFrameList : ScriptableObject
    {

        [Header("Data")]
        [Tooltip("List of frequency frames")]
        public List<FrequencyFrame> Frames = new List<FrequencyFrame>();

        private void OnValidate()
        {
            for (int i = 0, n = Frames.Count; i < n; i++)
            {
                if(Frames[i] != null)
                    Frames[i].Validate();
            }
        }

    }

}
