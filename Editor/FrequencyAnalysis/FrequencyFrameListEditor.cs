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
using UnityEditorInternal;
using Nebukam.Editor;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    [CustomEditor(typeof(SpectrumFrameList))]
    public class FrequencyFrameListEditor : UnityEditor.Editor
    {

        internal static FrameEditorVisibility m_showComponents = FrameEditorVisibility.Everything;

        internal float[] m_sizes = new float[0];

        internal ReorderableList m_listDrawer;
        internal SerializedProperty m_fflistSerialized;
        internal SpectrumFrameList m_fflist;

        internal Rect m_lastRectSize = new Rect(0,0,0,0);
        internal int m_activeIndex = 0;

        public override bool RequiresConstantRepaint() { return true; }

        private void OnEnable()
        {

            m_showComponents = Prefs.GetFlags("FFLE_ShowComponents", m_showComponents);

            m_fflistSerialized = serializedObject.FindProperty("Frames");

            UpdateList();
            //listDrawer.elementHeight = 320f;

            m_activeIndex = -1;

        }

        private void UpdateList()
        {
            m_listDrawer = new ReorderableList(serializedObject, m_fflistSerialized, true, true, true, true);
            m_listDrawer.drawElementCallback = DrawListItems;
            m_listDrawer.drawHeaderCallback = DrawHeader;
            m_listDrawer.onRemoveCallback = OnListItemRemoved;
            m_listDrawer.onAddCallback = OnListItemAdded;
            m_listDrawer.elementHeightCallback = OnListElementHeight;

            m_fflist = target as SpectrumFrameList;
            if (m_sizes.Length != m_fflist.Frames.Count)
                m_sizes = new float[m_fflist.Frames.Count];

        }

        public override void OnInspectorGUI()
        {

            m_fflist = target as SpectrumFrameList;
            serializedObject.Update();

            __RequireRectUpdate(true);

            __SetRect(new Rect(20f, 20f, Screen.width - 40f, 20f));

            if (Button("Open in Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(m_fflist); }
            if(EnumFlagsField(ref m_showComponents, "Frame editor masks") == 1) { 
                Prefs.SetFlags("FFLE_ShowComponents", m_showComponents);
                UpdateList();
            }

            m_listDrawer.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

        }

        float OnListElementHeight(int index)
        {
            float baseHeight = EditorGUIUtility.singleLineHeight * 2f + 40f;
            if (index > m_sizes.Length - 1) { return baseHeight; }
            return m_sizes[index] + baseHeight;
        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            if(index > m_sizes.Length - 1) { return; }

            __RequireRectUpdate(false);

            if (isActive)
                m_activeIndex = index;

            float itemHeight = 0f;
            SpectrumFrame frame = m_fflist.Frames[index];
                        

            Rect r = new Rect(rect.x, rect.y, rect.width - 10f, EditorGUIUtility.singleLineHeight);
            __SetRect(r);

            Color col = frame == null ? Color.gray : frame.color * 0.5f;
            col.a = 1f;
            Separator(20f, col);
            if (ObjectField(ref frame, "") == 1) { 
                m_fflist.Frames[index] = frame;
                EditorUtility.SetDirty(target);
            }
            Separator(20f, col);

            if (frame == null) 
            {
                EditorGUI.LabelField(__GetCurrentRect(0f,100f), "Undefined FrequencyFrame", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                FrequencyFrameEditor.PrintFrequencyFrameEditor(frame, out itemHeight, m_showComponents);
            }

            m_sizes[index] = itemHeight;

        }

        void OnListItemAdded(ReorderableList list)
        {
            //if (activeIndex == -1) { return; }
            m_fflist.Frames.Add(null);
            EditorUtility.SetDirty(target);
        }

        void OnListItemRemoved(ReorderableList list)
        {
            if(m_activeIndex == -1) { return; }

            m_fflist.Frames.RemoveAt(m_activeIndex);
            EditorUtility.SetDirty(target);
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Frames");
        }

    }
}
#endif