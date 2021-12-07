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

        protected FrequencyAnalyserSync m_analyser;
        public FrequencyAnalyserSync analyser { get { return m_analyser; } }

        public float TimeOffset = 0f;
        public float Scale = 1f;

        protected FrameDataDictionary m_dataDictionary;
        public FrameDataDictionary dataDictionary { get { return m_dataDictionary; } }

        private void Awake()
        {
            m_analyser = new FrequencyAnalyserSync();
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
