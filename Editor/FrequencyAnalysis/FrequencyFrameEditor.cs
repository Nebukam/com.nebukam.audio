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

        public override bool RequiresConstantRepaint() { return true; }

        internal static int PrintFrequencyFrameEditor(FrequencyFrame frame, bool drawSamplingOptions = true)
        {

            int changes = 0;
            
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

            MiniLabel("Identifier");
            BeginInLine(W-30f); 
            changes += TextInput(ref frame.ID, "");
            NextInline(30f);
            changes += ColorFieldInlined(ref frame.color);
            EndInLine();

            BeginColumns(2);
            Bands bandsBefore = frame.bands;
            if (EnumInlined(ref frame.bands, true, "Bands") == 1)
            {
                changes++;
                frame.frequency = RemapFrequencies(frame.frequency, bandsBefore, frame.bands);
            }

            NextColumn();
            changes += EnumPopupInlined(ref frame.output, "Default output");

            EndColumns();
            
            if (drawSamplingOptions)
            {
                Space(4f);
                Line();

                bool ex1 = false;
                _expanded.TryGetValue(frame, out ex1);
                _expanded[frame] = Foldout(ex1, "Sampling");

                if (ex1)
                {
                 //   changes += EnumPopup(ref frame.bands, "Bands");
                    changes += EnumPopup(ref frame.output, "Range");
                    changes += EnumPopup(ref frame.tolerance, "Tolerance");
                    changes += FloatField(ref frame.scale, "Scale");

                }

                Space(4f);

                Line();
            }

            int nMax = (int)frame.bands;

            changes += StartSizeSlider(ref frame.frequency, new int2(0, nMax), 1, "Frequencies [0 - " + nMax + "]");
            if(frame.frequency.y < 1) { frame.frequency.y = 1; }

            Space(4f);
            changes += StartSizeSlider(ref frame.amplitude, new float2(0f, 1f), "Amplitudes");

            if (changes != 0) { EditorUtility.SetDirty(frame); }

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
