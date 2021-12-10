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

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Nebukam.Editor;
using static Nebukam.Editor.EditorDrawer;
using static Nebukam.Editor.EditorGLDrawer;
using Nebukam.Audio.FrequencyAnalysis;
using Nebukam.Audio.Editor;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    public class FrequencyAnalyserWindow : EditorWindow
    {

        const string PID_FrameList = "FAW_FrameList";
        const string PID_AudioClip = "FAW_AudioClip";
        const string PID_AudioClipTime = "FAW_AudioClipTime";
        const string PID_AudioClipPreviewDuration = "FAW_AudioClipPreviewDuration";

        private const string m_title = "FAnalyser Viewer";

        private static SpectrumFrameList m_frequencyFrameList = null;
        private static AudioClip m_audioClip = null;
        private static float m_currentTime = 0f;
        private static float m_currentScale = 1f;
        private static float m_previewDuration = 1f;

        [MenuItem("N:Toolkit/Audio/FAnalyser Viewer")]
        public static void ShowWindow()
        {
            FrequencyAnalyserWindow window = EditorWindow.GetWindow(typeof(FrequencyAnalyserWindow), false, m_title, true) as FrequencyAnalyserWindow;
            window.Refresh();
        }

        public static void ShowWindow(SpectrumFrameList frameList)
        {
            m_frequencyFrameList = Prefs.Set(PID_FrameList, frameList);
            ShowWindow();
        }

        private void Awake()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            AudioPlayer.Stop();
        }

        public void Refresh()
        {
            m_audioClip = Prefs.Get<AudioClip>(PID_AudioClip, null);
            AudioPlayer.clip = m_audioClip;

            m_frequencyFrameList = Prefs.Get<SpectrumFrameList>(PID_FrameList, null);
            m_currentTime = Prefs.Get(PID_AudioClipTime, 0f);
            m_previewDuration = Prefs.Get(PID_AudioClipPreviewDuration, 0f);
        }

        public void Update()
        {
            Repaint();
        }

        private Rect m_windowRect;

        void OnGUI()
        {

            FrequencyAnalysis.SetCurrentFrameList(m_frequencyFrameList);

            __RequireRectUpdate(true);
            m_windowRect = new Rect(10f, 10f, Screen.width - 20f, 20f);
            __SetRect(m_windowRect);

            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            if (Button("Refresh")) { Refresh(); }

            #region User data

            __BeginCol(2);
            MiniLabel("Audio Clip");
            if (ObjectField(ref m_audioClip) == 1)
                Prefs.Update(PID_AudioClip, ref m_audioClip);

            __NextCol();
            MiniLabel("Frame list");
            if (ObjectField(ref m_frequencyFrameList) == 1) { }
            Prefs.Update(PID_FrameList, ref m_frequencyFrameList);

            FrequencyAnalysis.SetCurrentFrameList(m_frequencyFrameList);

            __EndCol();

            #endregion

            Separator(20f);

            #region Bins & Scale

            __BeginCol(2);

            if (FloatField(ref m_currentScale, "Frequency Band Scaling") == 1)
                FrequencyAnalysis.freqAnalyser.scale = m_currentScale;

            __NextCol();

            Bins currentBins = FrequencyAnalysis.freqAnalyser.frequencyBins;
            if (EnumInlined<Bins>(ref currentBins, true, "Frequency bins") == 1)
                FrequencyAnalysis.SetFrequencyBins(currentBins);

            __EndCol();

            #endregion

            Separator(20f);

            DrawPlayerControls();


            EditorGUI.EndDisabledGroup();


            DrawPreview();

            Separator(20f);

            if (Slider(ref m_previewDuration, 0.1f, 10f) == 1)
            {
                Prefs.Set(PID_AudioClipPreviewDuration, m_previewDuration);
            }

        }

        protected void UpdateAnalyser()
        {
            if (m_audioClip != null && Event.current.type == EventType.Repaint)
                FrequencyAnalysis.Analyze(m_audioClip, m_currentTime);
        }

        protected void DrawPlayerControls()
        {

            string format_timeTotal = "--:--";
            string format_timeElapsed = "--:--";
            float timeTotal = 0f;

            if (m_audioClip != null)
            {
                format_timeTotal = TimeSpan.FromSeconds((double)m_audioClip.length).ToString(@"mm\:ss");
                format_timeElapsed = TimeSpan.FromSeconds((double)m_currentTime).ToString(@"mm\:ss");
                timeTotal = m_audioClip.length;
            }
            else
            {
                m_currentTime = 0f;
            }

            UpdateAnalyser();

            __BeginInLine(50f);
            Label(format_timeElapsed);
            __NextInline(m_windowRect.width - 100f);
            if (AudioPlayer.isPlaying) { m_currentTime = AudioPlayer.currentTime; }
            Slider(ref m_currentTime, 0f, timeTotal);
            Prefs.Set(PID_AudioClipTime, m_currentTime);
            __NextInline(50f);
            Label(format_timeTotal);
            __EndInLine();

            if (m_audioClip != null)
            {
                float2 f2 = new float2(10f, 50f);
                MinMaxSlider(ref f2, new float2(0f, m_audioClip.length), "Loop range");
            }

            __SetRect(new Rect(m_windowRect.x + m_windowRect.width * 0.3f, Y, m_windowRect.width * 0.3f, 0f));

            __BeginCol(5);
            if (Button(">"))
            {
                AudioPlayer.Play(m_audioClip, m_currentTime);
            }
            __NextCol();
            if (Button("||"))
            {
                AudioPlayer.currentTime = m_currentTime;
                AudioPlayer.Pause(true);
            }
            __NextCol();
            if (Button("x"))
            {
                AudioPlayer.Stop();
            }
            __NextCol();
            if (Button("|<"))
            {
                //AudioPlayer.Pause(); 
                AudioPlayer.currentTime = m_currentTime -= 1f / 60f;
            }
            __NextCol();
            if (Button(">|"))
            {
                //AudioPlayer.Pause(); 
                AudioPlayer.currentTime = m_currentTime += 1f / 60f;
            }
            __EndCol();


            m_windowRect.y = Y;
            __SetRect(m_windowRect);

        }

        #region Previews

        protected void DrawPreview()
        {

            List<SpectrumFrame> frames = FrequencyAnalysis.activeFrames;
            float[] bands = FrequencyAnalysis.activeBands;

            if (BeginGL(200f))
            {

                Color c = Color.black;
                c.a = 0.25f;
                GLFill(c);

                if (bands != null)
                    FrequencyAnalysis.DrawBandSpectrum(GLArea, bands);

                if (frames != null)
                {

                    FrequencyAnalysis.DrawLines(GLArea, Bands.band64);

                    FrequencyAnalysis.__BeginDrawMultipleFrames();

                    for (int i = 0; i < frames.Count; i++)
                        FrequencyAnalysis.DrawFrame(GLArea, frames[i]);

                    FrequencyAnalysis.__EndDrawMultipleFrames();
                }

                EndGL();
            }

        }

        protected void DrawRangePreview()
        {

            if (BeginGL(200f))
            {

                if (m_audioClip != null)
                    FrequencyAnalysis.BatchAnalyze(m_audioClip, m_currentTime, m_previewDuration);

                Color c = Color.black;
                c.a = 0.25f;
                GLFill(c);

                Sample[,] samples = FrequencyAnalysis.batchedData;
                int sampleCount = samples.GetLength(0);
                int frameCount = samples.GetLength(1);
                float sampleWidth = GLArea.width / sampleCount;
                float height = GLArea.height;
                float maxSampleSize = 1f; //TODO : Expose
                float sampleHeight = GLArea.height / maxSampleSize;

                for (int s = 0; s < sampleCount; s++)
                {
                    for (int f = 0; f < frameCount; f++)
                    {
                        SpectrumFrame frame = FrequencyAnalysis.data.frames[f];

                        Sample sample = samples[s, f];
                        float value = math.clamp(sample, 0f, maxSampleSize) * sampleHeight;

                        if (sample.average == 0f) { continue; }

                        Rect sRect = new Rect(
                            sampleWidth * s,
                            height - value,
                            sampleWidth,
                            value);

                        if (frame.output == OutputType.Trigger)
                        {
                            if (sample.ON)
                            {
                                GL.Begin(GL.LINES);
                                GL.Color(frame.color);
                                sRect.y = 0f;
                                sRect.height = height;
                                sRect.x += sRect.width * 0.5f;
                                sRect.width = 0f;
                                GL.Vertex3(sRect.x, sRect.y, 0);
                                GL.Vertex3(sRect.x, sRect.height, 0);
                                GL.End();
                            }
                        }
                        else
                        {
                            if (sample.output == OutputType.Peak)
                                sRect.height = 2;

                            GL.Begin(GL.QUADS);
                            GL.Color(frame.color);
                            GLRect(sRect);
                            GL.End();
                        }


                    }
                }

                EndGL();
            }

            // Update analyser again since we batch-updated the visualisation is not up-to-date in other inspectors.
            UpdateAnalyser();

        }

        #endregion


    }
}
#endif