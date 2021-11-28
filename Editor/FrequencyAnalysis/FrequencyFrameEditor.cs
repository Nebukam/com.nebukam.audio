using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using Nebukam.Editor;
using static Nebukam.Editor.EditorExtensions;

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
            SetR(new Rect(20f,20f,Screen.width - 60f, 500f));
            PrintFrequencyFrameEditor(target as FrequencyFrame);
        }

        internal static int PrintFrequencyFrameEditor(FrequencyFrame frame, bool drawSamplingOptions = true)
        {

            int changes = 0;

            MiniLabel("Identifier");

            changes += TextInput(ref frame.ID, "", W - 30f);
            changes += InlineColorField(ref frame.color, 1f);

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
            Space(4f);
            changes += StartSizeSlider(ref frame.amplitude, new float2(0f, 3f), "Amplitudes");

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
