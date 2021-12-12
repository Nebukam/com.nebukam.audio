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

    public static class FADraw
    {

        #region Bands

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="bands"></param>
        internal static void BandLines(Rect area, Bands bands)
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
        /// Draw band spectrum
        /// </summary>
        /// <param name="area"></param>
        /// <param name="data"></param>
        internal static void BandSpectrum(Rect area, float[] data, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

            int n = data.Length;
            float inc = area.width / n, hinc = inc * 0.5f;
            float sample, x, y = area.height, h;

            if (draw.HasFlag(SpectrumDrawMode.BANDS))
            {

                GL.Begin(GL.QUADS);
                GLCol(Color.gray, 0.25f);
                for (int i = 0; i < n; i++)
                {
                    sample = clamp(data[i] * scale, 0f, 1f);
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
                    sample = clamp(data[i] * scale, 0f, 1f); ;
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
                    sample = clamp(data[i] * scale, 0f, 1f); ;
                    x = i * inc + hinc;
                    h = sample * area.height;
                    GL.Vertex3(x, y - h, 0);
                }
                GL.End();
            }
        }

        #endregion

        #region Brackets

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="table"></param>
        internal static void BracketLines(Rect area, FrequencyTable table)
        {
            GL.Begin(GL.LINES);

            int lines = (int)table.ranges.Length;
            float inc = area.width / lines;

            for (int i = 0; i < lines; i++)
            {
                if(i == lines - 1) { continue; }
                GLCol(Color.black, 0.8f);
                float x = (i + 1) * inc;
                GL.Vertex3(x, 0, 0);
                GL.Vertex3(x, area.height, 0);
            }

            GL.End();

        }

        internal static void BracketSpectrum(Rect area, BracketData[] data, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

            float x, h;
            BracketData b;

            GL.Begin(GL.LINES);
            GLCol(Color.yellow, 1f);

            int pointCount = data.Length;
            float inc = area.width / pointCount;

            for (int i = 0; i < pointCount; i++)
            {
                x = i * inc + inc*0.5f;
                b = data[i];
                h = math.clamp(b.average * scale, 0f, 1f) * area.height;
                GL.Vertex3(x, area.height, 0f);
                GL.Vertex3(x, area.height - h, 0f);
            }

            GL.End();

        }

        #endregion

        #region Raw spectrum

        internal static void RawSpectrum(Rect area, float[] data, float scale = 1f, SpectrumDrawMode draw = SpectrumDrawMode.ALL)
        {

        }

        #endregion

        #region Frame

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="frame"></param>
        internal static void Frame(Rect rect, SpectrumFrame frame)
        {

            GL.Begin(GL.LINE_STRIP);
            GLCol(frame.color);
            Rect _r = FrequencyFrameEditor.GetFrameRect(frame, rect);
            GLRect(_r, true);
            GL.End();

            Vector2 s = new Vector2(100f, 100f);


            _r = new Rect(_r.center - s * 0.5f, s);

            Sample sample = FrequencyAnalysis.spectrumData[frame];
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
#endif