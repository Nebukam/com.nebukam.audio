using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Nebukam.Audio;
using Nebukam.Audio.FrequencyAnalysis;

namespace Nebukam.Audio.Editor
{
    public static class FAShared
    {

        public static FrequencyAnalyser freqAnalyser;
        public static FrameDataDictionary data;

        static FAShared()
        {
            freqAnalyser = new FrequencyAnalyser();
            data = new FrameDataDictionary();
        }

        public static void SetCurrentFrameList(FrequencyFrameList list)
        {

            data.Clear();

            if(list != null)
                data.Add(list);

        }

        public static void Analyze(AudioClip clip, float time)
        {
            
            if(time < 0f) { time = 0f; }
            if(time > clip.length) { time = clip.length; }

            freqAnalyser.AnalyzeAt(clip, time);
            freqAnalyser.UpdateFrameData(data);

        }

    }
}
