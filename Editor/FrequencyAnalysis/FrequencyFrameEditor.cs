using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using Nebukam.Editor;
using static Nebukam.Editor.EditorGLDrawer;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.Editor
{

    [CustomEditor(typeof(FrequencyFrame))]
    [CanEditMultipleObjects]
    public class FrequencyFrameEditor : UnityEditor.Editor
    {

        internal static Dictionary<Object, bool> _expanded = new Dictionary<Object, bool>();

        public override void OnInspectorGUI()
        {

            ToggleLayoutMode(true);
            SetR(new Rect(20f, 20f, Screen.width - 40f, 220f));

            if (Button("Open Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(); }
            Space(4f);

            PrintFrequencyFrameEditor(target as FrequencyFrame);


        }

        public void Update()
        {
            Repaint();
        }

        internal static int PrintFrequencyFrameEditor(FrequencyFrame frame, bool drawSamplingOptions = true)
        {

            int changes = 0;

            if (BeginGL(100f))
            {
                Color c = Color.black;
                c.a = 0.1f;
                GLFill(c);
                DrawFrame(frame, true);
                if (frame.bands == Bands.Eight)
                    DrawSpectrum(FAShared.freqAnalyser.freqBands8);
                else if (frame.bands == Bands.SixtyFour)
                    DrawSpectrum(FAShared.freqAnalyser.freqBands64);
                else if (frame.bands == Bands.HundredTwentyEight)
                    DrawSpectrum(FAShared.freqAnalyser.freqBands128);

                EndGL();
            }

            MiniLabel("Identifier");

            changes += TextInput(ref frame.ID, "", W - 30f);
            changes += InlineColorField(ref frame.color, W + (inLayout ? -10f : 10f));

            if (drawSamplingOptions)
            {
                Space(4f);
                Line();

                bool ex1 = false;
                _expanded.TryGetValue(frame, out ex1);
                _expanded[frame] = Foldout(ex1, "Sampling");

                if (ex1)
                {

                    Bands bandsBefore = frame.bands;
                    changes += EnumPopup(ref frame.bands, "Bands");
                    changes += EnumPopup(ref frame.range, "Range");
                    changes += EnumPopup(ref frame.tolerance, "Tolerance");
                    changes += FloatField(ref frame.scale, "Scale");

                    if (bandsBefore != frame.bands)
                        frame.frequency = RemapFrequencies(frame.frequency, bandsBefore, frame.bands);

                }

                Space(4f);

                Line();
            }

            int nMax = 0;

            if (frame.bands == Bands.Eight)
                nMax = 8;
            else if (frame.bands == Bands.SixtyFour)
                nMax = 64;
            else if (frame.bands == Bands.HundredTwentyEight)
                nMax = 128;

            changes += StartSizeSlider(ref frame.frequency, new int2(0, nMax), 1, "Frequencies [0 - " + nMax + "]");
            if(frame.frequency.y < 1) { frame.frequency.y = 1; }

            Space(4f);
            changes += StartSizeSlider(ref frame.amplitude, new float2(0f, 1f), "Amplitudes");

            if (changes != 0) { EditorUtility.SetDirty(frame); }

            return changes;

        }

        internal static int2 RemapFrequencies(int2 range, Bands from, Bands to)
        {
            int oMin = 0, oMax = 0, nMin = 0, nMax = 0;

            if (from == Bands.Eight)
                oMax = 7;
            else if (from == Bands.SixtyFour)
                oMax = 63;
            else if (from == Bands.HundredTwentyEight)
                oMax = 127;

            if (to == Bands.Eight)
                nMax = 7;
            else if (to == Bands.SixtyFour)
                nMax = 63;
            else if (to == Bands.HundredTwentyEight)
                nMax = 127;

            return new int2(
                map(range.x, oMin, oMax, nMin, nMax),
                map(range.y, oMin, oMax, nMin, nMax));

        }

        internal static Rect GetFrameRect(FrequencyFrame f, Rect rel)
        {
            float w = 1f;

            if (f.bands == Bands.Eight)
                w = 1f / 8f;
            else if (f.bands == Bands.SixtyFour)
                w = 1f / 64f;
            else if (f.bands == Bands.HundredTwentyEight)
                w = 1f / 128f;

            return new Rect(
                f.frequency.x * w * rel.width,
                (f.amplitude.x + f.amplitude.y) * rel.height,
                f.frequency.y * w * rel.width,
                f.amplitude.y * rel.height);
        }

        internal static void DrawFrame(FrequencyFrame frame, bool drawLines = false)
        {

            if (drawLines)
            {

                GL.Begin(GL.LINES);

                int lines = 0;
                if (frame.bands == Bands.Eight) lines = 8;
                else if (frame.bands == Bands.SixtyFour) lines = 64;
                else if (frame.bands == Bands.HundredTwentyEight) lines = 128;

                float inc = GLArea.width / lines;
                for (int i = 0; i < lines; i++)
                {
                    if (i % 8 == 0 || i == lines - 1) { GLCol(Color.black, 0.25f); }
                    else { GLCol(Color.black, 0.1f); }
                    float x = inc * 0.5f + i * inc;
                    GL.Vertex3(x, 0, 0);
                    GL.Vertex3(x, GLArea.height, 0);
                }

                GL.End();

            }

            GL.Begin(GL.LINE_STRIP);
            GLCol(frame.color);

            Rect fr = FrequencyFrameEditor.GetFrameRect(frame, GLArea);
            fr.y = GLArea.height - fr.y;

            GLRect(fr, true);
            GL.End();
        }

        internal static void DrawSpectrum(float[] list)
        {
            int n = list.Length;
            float inc = GLArea.width / n;
            float sample, x, y = GLArea.height;

            GL.Begin(GL.LINES);
            GLCol(Color.red);
            for(int i = 0; i < n; i++)
            {
                sample = list[i];
                x = i * inc;
                GL.Vertex3(x, y, 0);
                GL.Vertex3(x, y - sample * GLArea.height, 0);
            }

            GL.End();
        }

        internal static int map(int s, int oMin, int oMax, int nMin, int nMax)
        {
            return nMin + (s - oMin) * (nMax - nMin) / (oMax - oMin);
        }

        internal static float map(float s, float oMin, float oMax, float nMin, float nMax)
        {
            return nMin + (s - oMin) * (nMax - nMin) / (oMax - oMin);
        }

    }

}
