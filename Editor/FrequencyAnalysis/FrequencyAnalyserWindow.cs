using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Nebukam.Editor;
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
        private static float m_currentScale = 1f;

        [MenuItem("N:Toolkit/:Audio/Frequency Analyzer")]
        public static void ShowWindow()
        {
            m_audioClip = Prefs.Get<AudioClip>("FAW_AudioClip", null);
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

            FrequencyAnalysis.SetCurrentFrameList(m_frequencyFrameList);

            __RequireRectUpdate(true);
            SetRect(new Rect(10f, 10f, Screen.width - 20f, 20f));

            __BeginCol(2);
            MiniLabel("Audio Clip");
            if (ObjectField(ref m_audioClip) == 1)
                Prefs.Update("FAW_AudioClip", ref m_audioClip);

            __NextCol();
            MiniLabel("Frame list");
            if (ObjectField(ref m_frequencyFrameList) == 1) { }
                Prefs.Update("FAW_FrameList", ref m_frequencyFrameList);

            FrequencyAnalysis.SetCurrentFrameList(m_frequencyFrameList);

            __EndCol();

            Space(8f);
            Line();
            Space(8f);

            FloatField(ref m_currentScale, "Frequency Band Scaling");
            FrequencyAnalysis.freqAnalyser.scale = m_currentScale;

            Space(8f);
            Line();
            Space(8f);

            if (m_audioClip == null)
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
                FrequencyAnalysis.Analyze(m_audioClip, m_currentTime);
            }

            if (BeginGL(200f))
            {

                Color c = Color.black;
                c.a = 0.25f;
                GLFill(c);

                if (m_audioClip != null)
                {
                    FrequencyAnalysis.DrawSpectrum(GLArea, FrequencyAnalysis.freqAnalyser.freqBands64);
                }

                if (m_frequencyFrameList != null)
                {
                    FrequencyFrame frame;
                    FrequencyAnalysis.DrawLines(GLArea, Bands.SixtyFour);

                    for (int i = 0; i < m_frequencyFrameList.Frames.Count; i++)
                    {
                        frame = m_frequencyFrameList.Frames[i];
                        FrequencyAnalysis.DrawFrame(GLArea, frame);
                    }

                }

                EndGL();
            }

            EditorGUI.LabelField(GetCurrentRect(10f, -50f), "test");

        }

        

    }
}
