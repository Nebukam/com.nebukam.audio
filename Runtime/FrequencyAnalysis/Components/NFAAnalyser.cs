using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [AddComponentMenu("N:Toolkit/Audio/Frequency Analysis/Analyser")]
    public class NFAAnalyser : MonoBehaviour
    {

        public AudioSource Source;

        protected FrequencyAnalyser m_analyser;
        public FrequencyAnalyser analyser { get { return m_analyser; } }

        public float TimeOffset = 0f;
        public float Scale = 1f;

        protected FrameDataDictionary m_dataDictionary;
        public FrameDataDictionary dataDictionary { get { return m_dataDictionary; } }

        private void Awake()
        {
            m_analyser = new FrequencyAnalyser();
            m_dataDictionary = new FrameDataDictionary();
        }

#if UNITY_EDITOR

        public static List<NFAAnalyser> m_registeredAnalysers;

        private void OnEnable()
        {
            if(m_registeredAnalysers == null)
                m_registeredAnalysers = new List<NFAAnalyser>();

            if (m_registeredAnalysers.IndexOf(this) == -1)
                m_registeredAnalysers.Add(this);
        }

        private void OnDisable()
        {
            if (m_registeredAnalysers == null) { return; }

            int index = m_registeredAnalysers.IndexOf(this);

            if (index != -1)
                m_registeredAnalysers.RemoveAt(index);
        }

#endif

        private void Update()
        {
            
            if(Source == null || Source.clip == null || !Source.isPlaying) { return; }

            m_analyser.scale = Scale;
            m_analyser.AnalyseAt(Source.clip, Source.time + TimeOffset);
            m_analyser.ReadDataDictionary(m_dataDictionary);

        }




    }
}
