using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Audio;
using Nebukam.Audio.FrequencyAnalysis;
using static Nebukam.Editor.EditorGLDrawer;
using static Nebukam.Editor.EditorDrawer;


namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    [Flags]
    public enum SpectrumDrawMode
    {
        NONE = 0,
        LINES = 1,
        CURVE = 2,
        BANDS = 4,
        ALL = LINES | CURVE | BANDS
    }

    public static class FrequencyAnalysis
    {

        public static FrequencyAnalyserSync freqAnalyser;
        public static FrameDataDictionary data;

        public static FrequencyAnalyserSync activeAnalyser
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (NFAAnalyser.m_registeredAnalysers != null
                    && NFAAnalyser.m_registeredAnalysers.Count > 0)
                    {
                        NFAAnalyser analyser = NFAAnalyser.m_registeredAnalysers[0];
                        return analyser.analyser;
                    }
                }

                return freqAnalyser;
            }
        }

        public static List<FrequencyFrame> activeFrames
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (NFAAnalyser.m_registeredAnalysers != null
                    && NFAAnalyser.m_registeredAnalysers.Count > 0)
                    {
                        NFAAnalyser analyser = NFAAnalyser.m_registeredAnalysers[0];
                        return analyser.dataDictionary.frames;
                    }
                }

                return data.frames;
            }
        }

        public static float[] activeBands
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (NFAAnalyser.m_registeredAnalysers != null
                    && NFAAnalyser.m_registeredAnalysers.Count > 0)
                    {
                        NFAAnalyser analyser = NFAAnalyser.m_registeredAnalysers[0];
                        return analyser.analyser.freqBands128;
                    }
                }

                return freqAnalyser.freqBands128;
            }
        }

        static FrequencyAnalysis()
        {
            freqAnalyser = new FrequencyAnalyserSync();
            data = new FrameDataDictionary();

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        }

        public static void OnBeforeAssemblyReload()
        {

        }

        public static void OnAfterAssemblyReload()
        {

        }

        public static void SetCurrentFrameList(FrequencyFrameList list)
        {

            data.Clear();

            if (list != null)
                data.Add(list);

        }

        public static void SetFrequencyBins(Bins bins)
        {
            if (freqAnalyser.frequencyBins != bins)
            {
                freqAnalyser.Init(bins);
            }
        }

        public static void Analyze(AudioClip clip, float time)
        {

            if (time < 0f) { time = 0f; }
            if (time > clip.length) { time = clip.length; }

            freqAnalyser.AnalyseAt(clip, time);
            freqAnalyser.ReadDataDictionary(data);

        }

        public static float batchedDataStartTime = 0f;
        public static float batchedDataDuration = 0f;
        public static Sample[,] batchedData = new Sample[0, 0];

        public static void BatchAnalyze(AudioClip clip, float from, float duration)
        {

            batchedDataStartTime = from;
            batchedDataDuration = duration;
            freqAnalyser.ReadRange(clip, data, from, duration, ref batchedData);

        }

        #region Spectrum drawing utils

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="bands"></param>
        internal static void DrawLines(Rect area, Bands bands)
        {

            GL.Begin(GL.LINES);

            int lines = (int)bands;

            float inc = area.width / lines;
            for (int i = 0; i < lines; i++)
            {
                if (i % 8 == 0 || i == lines - 1) { GLCol(Color.black, 0.25f); }
                else { GLCol(Color.black, 0.1f); }
                float x = inc * 0.5f + i * inc;
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, area.height, 0);
            }

            GLCol(Color.black, 0.15f);
            GL.Vertex3(0, area.height * 0.5f, 0); GL.Vertex3(area.width, area.height * 0.5f, 0);
            GLCol(Color.black, 0.1f);
            GL.Vertex3(0, area.height * 0.25f, 0); GL.Vertex3(area.width, area.height * 0.25f, 0);
            GL.Vertex3(0, area.height * 0.75f, 0); GL.Vertex3(area.width, area.height * 0.75f, 0);
            GL.End();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="table"></param>
        internal static void DrawLines(Rect area, FrequencyTable table, Bins bins)
        {
            GL.Begin(GL.LINES);

            int lines = (int)table.Brackets.Count;
            float inc = area.width / lines;

            for (int i = 0; i < lines; i++)
            {
                GLCol(Color.black, 0.8f);
                float x = (i + 1) * inc;
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, area.height, 0);
            }

        }

        /// <summary>
        /// Draw band spectrum
        /// </summary>
        /// <param name="area"></param>
        /// <param name="frequencies"></param>
        internal static void DrawBandSpectrum(Rect area, float[] frequencies, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

            int n = frequencies.Length;
            float inc = area.width / n, hinc = inc * 0.5f;
            float sample, x, y = area.height, h;

            if (draw.HasFlag(SpectrumDrawMode.BANDS))
            {

                GL.Begin(GL.QUADS);
                GLCol(Color.gray, 0.25f);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(frequencies[i] * scale, 0f, 1f);
                    x = i * inc;
                    h = sample * area.height;
                    GLRect(new Rect(x, y - h, inc, h));
                }
                GL.End();
            }

            if (draw.HasFlag(SpectrumDrawMode.LINES))
            {
                GL.Begin(GL.LINES);
                GLCol(Color.red);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(frequencies[i] * scale, 0f, 1f); ;
                    x = i * inc + hinc;
                    h = sample * area.height;
                    GL.Vertex3(x, y, 0);
                    GL.Vertex3(x, y - h, 0);
                }
                GL.End();
            }

            if (draw.HasFlag(SpectrumDrawMode.CURVE))
            {
                GL.Begin(GL.LINE_STRIP);
                GLCol(Color.green, 0.25f);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(frequencies[i] * scale, 0f, 1f); ;
                    x = i * inc + hinc;
                    h = sample * area.height;
                    GL.Vertex3(x, y - h, 0);
                }
                GL.End();
            }
        }

        internal static void DrawBracketSpectrum(Rect area, FrequencyTable table, float[] frequencies, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

            float x, h;

            GL.Begin(GL.LINES);
            GLCol(Color.gray, 0.25f);

            float[] samples = activeAnalyser.samples;
            int sampleCount = samples.Length;
            float inc = area.width / sampleCount;

            for (int i = 0; i < sampleCount; i++)
            {
                x = i * inc;
                h = math.clamp(samples[i] * scale, 0f, 1f) * area.height;
                GL.Vertex3(x, area.height, 0f);
                GL.Vertex3(x, area.height-h, 0f);
            }

            GL.End();

        }

        internal static void DrawRawSpectrum(Rect area, float[] frequencies, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="frame"></param>
        internal static void DrawFrame(Rect rect, FrequencyFrame frame)
        {

            GL.Begin(GL.LINE_STRIP);
            GLCol(frame.color);
            Rect _r = FrequencyFrameEditor.GetFrameRect(frame, rect);
            GLRect(_r, true);
            GL.End();

            Vector2 s = new Vector2(100f, 100f);


            _r = new Rect(_r.center - s * 0.5f, s);

            Sample sample = data.Get(frame);
            FrameLabel labelInfos = new FrameLabel()
            {
                rect = _r,
                sample = sample,
                color = frame.color
            };

            if (m_drawingMultipleFrames)
                m_frameLabels.Add(labelInfos);
            else
                PrintFrameLabel(labelInfos);

        }

        private struct FrameLabel
        {
            public Rect rect;
            public Sample sample;
            public Color color;
        }

        private static List<FrameLabel> m_frameLabels = new List<FrameLabel>();
        private static bool m_drawingMultipleFrames = false;

        internal static void __BeginDrawMultipleFrames()
        {
            m_frameLabels.Clear();
            m_drawingMultipleFrames = true;
        }

        /// <summary>
        /// Required to draw labels AFTER GL has finished drawing.
        /// </summary>
        internal static void __EndDrawMultipleFrames()
        {
            m_drawingMultipleFrames = false;

            for (int i = 0; i < m_frameLabels.Count; i++)
                PrintFrameLabel(m_frameLabels[i]);

            m_frameLabels.Clear();
        }

        private static void PrintFrameLabel(FrameLabel labelInfos)
        {
            centeredLabel.normal.textColor = labelInfos.color;
            GUI.Label(labelInfos.rect, labelInfos.sample.Default.ToString("0.00"), centeredLabel);
        }

        #endregion


    }
}
