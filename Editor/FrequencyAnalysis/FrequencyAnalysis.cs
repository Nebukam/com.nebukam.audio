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


namespace Nebukam.Audio.Editor
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

        public static FrequencyAnalyser freqAnalyser;
        public static FrameDataDictionary data;

        static FrequencyAnalysis()
        {
            freqAnalyser = new FrequencyAnalyser();
            data = new FrameDataDictionary();
        }

        public static void SetCurrentFrameList(FrequencyFrameList list)
        {

            data.Clear();

            if (list != null)
                data.Add(list);

        }

        public static void Analyze(AudioClip clip, float time)
        {

            if (time < 0f) { time = 0f; }
            if (time > clip.length) { time = clip.length; }

            freqAnalyser.AnalyzeAt(clip, time);
            freqAnalyser.UpdateFrameData(data);

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
        /// <param name="frequencies"></param>
        internal static void DrawSpectrum(Rect area, float[] frequencies, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

            int n = frequencies.Length;
            float inc = area.width / n, hinc = inc * 0.5f;
            float sample, x, y = area.height, h;

            if (draw.HasFlag(SpectrumDrawMode.BANDS))
            {

                GL.Begin(GL.QUADS);
                GLCol(Color.gray, 0.5f);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(frequencies[i], 0f, 1f);
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
                    sample = clamp(frequencies[i], 0f, 1f); ;
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
                GLCol(Color.green, 0.5f);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(frequencies[i], 0f, 1f); ;
                    x = i * inc + hinc;
                    h = sample * area.height;
                    GL.Vertex3(x, y - h, 0);
                }
                GL.End();
            }
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
            centeredLabel.normal.textColor = frame.color;
            _r = new Rect(_r.center - s * 0.5f, s);

            GUI.Label(_r, "0.0", centeredLabel);

        }

        #endregion


    }
}
