using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static Nebukam.Editor.EditorDrawer;
using static Nebukam.Editor.EditorGLDrawer;
using Nebukam.Audio.FrequencyAnalysis;

namespace Nebukam.Audio.Editor { 

    public class FrequencyAnalyserWindow : EditorWindow
    {

        private const string m_title = "Frequency Analyser";

        private static FrequencyFrameList m_frequencyFrameList = null;
        private static AudioClip m_audioClip = null;
        private static float m_currentTime = 0f;

        [MenuItem("N:Toolkit/:Audio/Frequency Analyzer")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(FrequencyAnalyserWindow), false, m_title, true);
        }

        public static void ShowWindow(FrequencyFrameList frameList)
        {
            m_frequencyFrameList = frameList;
            ShowWindow();
        }

        public void Update()
        {
            Repaint();
        }

        void OnGUI()
        {

            FAShared.SetCurrentFrameList(m_frequencyFrameList);

            ToggleLayoutMode(false);
            SetR(new Rect(10f, 10f, Screen.width - 20f, 20f));

            MiniLabel("Frame list");
            ObjectField(ref m_frequencyFrameList);
            FAShared.SetCurrentFrameList(m_frequencyFrameList);

            MiniLabel("Audio Clip");
            ObjectField(ref m_audioClip);

            if(m_audioClip == null)
            {
                Label("--:-- / --:--");
                float fake = 0f;
                Slider(ref fake, 0f, 0f);
            }
            else
            {
                var ttl = TimeSpan.FromSeconds((double)m_audioClip.length).ToString(@"mm\:ss");
                var ttf = TimeSpan.FromSeconds((double)m_currentTime).ToString(@"mm\:ss");
                Label(ttf+" / "+ ttl);
                Slider(ref m_currentTime, 0f, m_audioClip.length);
                FAShared.Analyze(m_audioClip, m_currentTime);
            }

            if (BeginGL(100f))
            {
                //GL.Begin(GL.LINES);

                Color c = Color.black;
                c.a = 0.25f;
                GLFill(c);

                if (m_audioClip != null)
                {
                    FrequencyFrameEditor.DrawSpectrum(FAShared.freqAnalyser.freqBands64);
                }

                if (m_frequencyFrameList != null)
                {
                    FrequencyFrame frame;
                    for(int i = 0; i < m_frequencyFrameList.Frames.Count; i++)
                    {
                        frame = m_frequencyFrameList.Frames[i];
                        FrequencyFrameEditor.DrawFrame(frame, i == 0);
                    }
                }

                EndGL();
            }

        }

        

    }
}
