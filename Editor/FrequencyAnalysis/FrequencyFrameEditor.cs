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

using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using static Nebukam.Editor.EditorGLDrawer;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    [System.Flags]
    public enum FrameEditorVisibility
    {
        None = 0,
        Preview = 1,
        Identifier = 2,
        Extraction = 4,
        Scaling = 8,
        FrequencyAndAmplitude = 16,
        Everything = ~0
    }

    [CustomEditor(typeof(SpectrumFrame))]
    [CanEditMultipleObjects]
    public class FrequencyFrameEditor : UnityEditor.Editor
    {

        internal static Dictionary<SpectrumFrame, Dictionary<FrequencyTable, SpectrumFrameData>> m_cachedModifications
            = new Dictionary<SpectrumFrame, Dictionary<FrequencyTable, SpectrumFrameData>>();

        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {

            __RequireRectUpdate(true);
            __SetRect(new Rect(20f, 20f, Screen.width - 40f, 220f));

            if (Button("Open Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(); }
            Space(4f);

            float lH;

            PrintFrequencyFrameEditor(target as SpectrumFrame, out lH);

        }

        internal static int PrintFrequencyFrameEditor(SpectrumFrame frame, out float localHeight, FrameEditorVisibility showOptions = FrameEditorVisibility.Everything)
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

                    IFrequencyTableProcessor tableProc;
                    if (FrequencyAnalysis.spectrumAnalyser.spectrumAnalysis.TryGetTableProcessor(frame.table, out tableProc))
                    {

                        if (frame.extraction == FrequencyExtraction.Bands)
                        {
                            FADraw.BandLines(GLArea, frame.bands);
                            FADraw.BandSpectrum(GLArea, tableProc.bandsExtraction.Get(frame.bands).cachedOutput, frame.inputScale, SpectrumDrawMode.BANDS);
                        }
                        else if (frame.extraction == FrequencyExtraction.Bracket)
                        {
                            FADraw.BracketLines(GLArea, frame.table);
                            FADraw.BracketSpectrum(GLArea, tableProc.bracketsExtraction.cachedOutput, frame.inputScale, SpectrumDrawMode.BANDS);
                        }
                        else if (frame.extraction == FrequencyExtraction.Raw)
                        {
                            FADraw.RawSpectrum(GLArea, FrequencyAnalysis.spectrumAnalyser.spectrumProvider.cachedOutput, frame.inputScale, SpectrumDrawMode.BANDS);
                        }

                    }

                    FADraw.Frame(GLArea, frame);

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

            if (showOptions.HasFlag(FrameEditorVisibility.Extraction))
            {
                Space(4f);
                //changes += EnumPopup(ref frame.tolerance, "Tolerance");

                __BeginCol(2);
                MiniLabel("Frequency Table");
                FrequencyTable prevTable = frame.table;
                if (ObjectField(ref frame.table) == 1 || frame.table == null)
                {
                    if (frame.table == null)
                        frame.table = FrequencyTable.tableCommon;

                    // Remap bracket values as the table chenged
                    if (prevTable != null && frame.extraction == FrequencyExtraction.Bracket)
                        frame.frequenciesBracket = RemapBrackets(frame.frequenciesBracket, prevTable.Count, frame.table.Count);

                    changes++;
                }
                __NextCol();
                changes += EnumInlined(ref frame.extraction, false, "Extraction mode");
                __EndCol();

                if (frame.extraction == FrequencyExtraction.Bands)
                {
                    Bands bandsBefore = frame.bands;
                    if (EnumInlined(ref frame.bands, true, "Bands") == 1)
                    {
                        // Remap band values as the band changed
                        frame.frequenciesBand = RemapBands(frame.frequenciesBand, bandsBefore, frame.bands);
                        changes++;
                    }
                }
                Space(4f);

            }

            if (showOptions.HasFlag(FrameEditorVisibility.FrequencyAndAmplitude))
            {

                int nMax = 0;

                if (frame.extraction == FrequencyExtraction.Bands)
                {

                    nMax = (int)frame.bands;

                    changes += StartSizeSlider(ref frame.frequenciesBand, new int2(0, nMax), 1, "Bands [0 - " + nMax + "]");
                    if (frame.frequenciesBand.y < 1) { frame.frequenciesBand.y = 1; }

                }
                else if (frame.extraction == FrequencyExtraction.Bracket)
                {

                    nMax = (int)frame.table.Count;

                    changes += StartSizeSlider(ref frame.frequenciesBracket, new int2(0, nMax), 1, "Brackets [0 - " + nMax + "]");
                    if (frame.frequenciesBracket.y < 1) { frame.frequenciesBracket.y = 1; }

                }
                else if (frame.extraction == FrequencyExtraction.Raw)
                {

                    nMax = (int)FrequencyTable.maxHz;

                    changes += StartSizeSlider(ref frame.frequenciesRaw, new int2(0, nMax), 1, "Frequencies [0 - " + nMax + "]");
                    if (frame.frequenciesRaw.y < 1) { frame.frequenciesRaw.y = 1; }

                }

                Space(4f);
                changes += StartSizeSlider(ref frame.amplitude, new float2(0f, 1f), "Amplitudes");

            }


            if (showOptions.HasFlag(FrameEditorVisibility.Scaling))
            {
                __BeginCol(3);

                changes += EnumPopupInlined(ref frame.output, "Value");

                __NextCol();

                changes += FloatFieldClamped(ref frame.inputScale, 0.001f, 1000f, "Input scale");

                __NextCol();

                changes += FloatFieldClamped(ref frame.outputScale, 0.001f, 1000f, "Output scale");

                __EndCol();
                Space(4f);
            }


            if (changes != 0)
            {
                EditorUtility.SetDirty(frame);
                WriteCache(frame, frame.table);
            }

            localHeight = Y - _YBefore;

            return changes;

        }

        internal static int2 RemapBands(int2 range, Bands from, Bands to)
        {
            int oMax = (int)from, nMax = (int)to;

            return new int2(
                map(range.x, 0, oMax, 0, nMax),
                map(range.y, 0, oMax, 0, nMax));

        }

        internal static int2 RemapBrackets(int2 range, int from, int to)
        {
            return new int2(
                map(range.x, 0, from, 0, to),
                map(range.y, 0, from, 0, to));
        }

        internal static Rect GetFrameRect(SpectrumFrame f, Rect rel)
        {

            float w;

            switch (f.extraction)
            {
                case FrequencyExtraction.Bands:
                    w = 1f / (int)f.bands;
                    return new Rect(
                        f.frequenciesBand.x * w * rel.width,
                        rel.height - ((f.amplitude.x + f.amplitude.y) * rel.height),
                        f.frequenciesBand.y * w * rel.width,
                        f.amplitude.y * rel.height);
                    break;
                case FrequencyExtraction.Bracket:
                    w = 1f / (int)f.table.Count;
                    return new Rect(
                        f.frequenciesBracket.x * w * rel.width,
                        rel.height - ((f.amplitude.x + f.amplitude.y) * rel.height),
                        f.frequenciesBracket.y * w * rel.width,
                        f.amplitude.y * rel.height);
                    break;
                case FrequencyExtraction.Raw:
                    w = 1f / (int)f.table.Count;
                    return new Rect(
                        f.frequenciesBracket.x * w * rel.width,
                        rel.height - ((f.amplitude.x + f.amplitude.y) * rel.height),
                        f.frequenciesBracket.y * w * rel.width,
                        f.amplitude.y * rel.height);
                    break;
            }

            return rel;

        }

        internal static void WriteCache(SpectrumFrame frame, FrequencyTable table)
        {
            Dictionary<FrequencyTable, SpectrumFrameData> data;

            if (!m_cachedModifications.TryGetValue(frame, out data))
            {
                data = new Dictionary<FrequencyTable, SpectrumFrameData>();
                m_cachedModifications[frame] = data;
            }

            data[table] = frame;

        }

        internal static void ReadCache(SpectrumFrame frame, FrequencyTable table)
        {
            Dictionary<FrequencyTable, SpectrumFrameData> data;
            SpectrumFrameData frameData;

            if (!m_cachedModifications.TryGetValue(frame, out data)
                || !data.TryGetValue(table, out frameData))
            {
                WriteCache(frame, table);
                return;
            }

            frame.Copy(frameData);

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
#endif