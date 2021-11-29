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

    [System.Flags]
    public enum FrameEditorVisibility
    {
        None = 0,
        Preview = 1,
        Identifier = 2,
        Tolerance = 4,
        BandsAndOutputType = 8,
        Scaling = 16,
        FrequencyAndAmplitude = 32,
        Everything = ~0
    }

    [CustomEditor(typeof(FrequencyFrame))]
    [CanEditMultipleObjects]
    public class FrequencyFrameEditor : UnityEditor.Editor
    {

        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {

            __RequireRectUpdate(true);
            SetRect(new Rect(20f, 20f, Screen.width - 40f, 220f));

            if (Button("Open Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(); }
            Space(4f);

            float lH;

            PrintFrequencyFrameEditor(target as FrequencyFrame, out lH);

        }

        internal static int PrintFrequencyFrameEditor(FrequencyFrame frame, out float localHeight, FrameEditorVisibility showOptions = FrameEditorVisibility.Everything)
        {

            float _YBefore = Y;
            int changes = 0;

            if (showOptions.HasFlag(FrameEditorVisibility.Preview))
            {
                if (BeginGL(100f))
                {
                    Color c = Color.black;
                    c.a = 0.1f;
                    GLFill(c);

                    FrequencyAnalysis.DrawLines(GLArea, frame.bands);
                    FrequencyAnalysis.DrawSpectrum(GLArea, FrequencyAnalysis.freqAnalyser.GetBands(frame.bands), SpectrumDrawMode.BANDS);
                    FrequencyAnalysis.DrawFrame(GLArea, frame);

                    EndGL();
                }
            }

            if (showOptions.HasFlag(FrameEditorVisibility.Identifier))
            {
                MiniLabel("Identifier");
                __BeginInLine(W - 30f);
                changes += TextInput(ref frame.ID, "");
                __NextInline(30f);
                changes += ColorFieldInlined(ref frame.color);
                __EndInLine();
            }

            if (showOptions.HasFlag(FrameEditorVisibility.Tolerance))
            {
                Space(4f);
                changes += EnumPopup(ref frame.tolerance, "Tolerance");
            }

            if (showOptions.HasFlag(FrameEditorVisibility.BandsAndOutputType))
            {
                __BeginCol(2);
                Bands bandsBefore = frame.bands;
                if (EnumInlined(ref frame.bands, true, "Bands") == 1)
                {
                    changes++;
                    frame.frequency = RemapFrequencies(frame.frequency, bandsBefore, frame.bands);
                }

                __NextCol();
                changes += EnumPopupInlined(ref frame.output, "Default output");

                __EndCol();
                Space(4f);
            }

            if (showOptions.HasFlag(FrameEditorVisibility.Scaling))
            {
                __BeginCol(2);
                changes += FloatField(ref frame.inputScale, "Input Scale");
                __NextCol();
                changes += FloatField(ref frame.outputScale, "Output Scale");
                __EndCol();     
                
                Space(4f);
            }

            if (showOptions.HasFlag(FrameEditorVisibility.FrequencyAndAmplitude))
            {
                int nMax = (int)frame.bands;

                changes += StartSizeSlider(ref frame.frequency, new int2(0, nMax), 1, "Frequencies [0 - " + nMax + "]");
                if (frame.frequency.y < 1) { frame.frequency.y = 1; }

                Space(4f);

                changes += StartSizeSlider(ref frame.amplitude, new float2(0f, 1f), "Amplitudes");

            }

            if (changes != 0) { EditorUtility.SetDirty(frame); }

            localHeight = Y - _YBefore;

            return changes;

        }

        internal static int2 RemapFrequencies(int2 range, Bands from, Bands to)
        {
            int oMax = (int)from, nMax = (int)to;

            return new int2(
                map(range.x, 0, oMax, 0, nMax),
                map(range.y, 0, oMax, 0, nMax));

        }

        internal static Rect GetFrameRect(FrequencyFrame f, Rect rel)
        {
            float w = 1f / (int)f.bands;

            return new Rect(
                f.frequency.x * w * rel.width,
                rel.height - ((f.amplitude.x + f.amplitude.y) * rel.height),
                f.frequency.y * w * rel.width,
                f.amplitude.y * rel.height);
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
