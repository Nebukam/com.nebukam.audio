using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Nebukam.Editor;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.Editor
{

    [CustomEditor(typeof(FrequencyFrameList))]
    public class FrequencyFrameListEditor : UnityEditor.Editor
    {

        internal static FrameEditorVisibility m_showComponents = FrameEditorVisibility.Everything;

        internal float[] m_sizes = new float[0];

        internal ReorderableList listDrawer;
        internal SerializedProperty fflistSerialized;
        internal FrequencyFrameList fflist;

        internal Rect lastRectSize = new Rect(0,0,0,0);
        internal int activeIndex = 0;

        public override bool RequiresConstantRepaint() { return true; }

        private void OnEnable()
        {

            m_showComponents = Prefs.GetFlags("FFLE_ShowComponents", m_showComponents);

            fflistSerialized = serializedObject.FindProperty("Frames");

            UpdateList();
            //listDrawer.elementHeight = 320f;

            activeIndex = -1;

        }

        private void UpdateList()
        {
            listDrawer = new ReorderableList(serializedObject, fflistSerialized, true, true, true, true);
            listDrawer.drawElementCallback = DrawListItems;
            listDrawer.drawHeaderCallback = DrawHeader;
            listDrawer.onRemoveCallback = OnListItemRemoved;
            listDrawer.onAddCallback = OnListItemAdded;
            listDrawer.elementHeightCallback = OnListElementHeight;

            fflist = target as FrequencyFrameList;
            if (m_sizes.Length != fflist.Frames.Count)
                m_sizes = new float[fflist.Frames.Count];

        }

        public override void OnInspectorGUI()
        {

            fflist = target as FrequencyFrameList;
            serializedObject.Update();

            __RequireRectUpdate(true);

            __SetRect(new Rect(20f, 20f, Screen.width - 40f, 20f));

            if (Button("Open in Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(fflist); }
            if(EnumFlagsField(ref m_showComponents, "Frame editor masks") == 1) { 
                Prefs.SetFlags("FFLE_ShowComponents", m_showComponents);
                UpdateList();
            }

            listDrawer.DoLayoutList();

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
                activeIndex = index;

            float itemHeight = 0f;
            FrequencyFrame frame = fflist.Frames[index];
                        

            Rect r = new Rect(rect.x, rect.y, rect.width - 10f, EditorGUIUtility.singleLineHeight);
            __SetRect(r);

            Color col = frame == null ? Color.gray : frame.color * 0.5f;
            col.a = 1f;
            Separator(20f, col);
            if (ObjectField(ref frame, "") == 1) { 
                fflist.Frames[index] = frame;
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
            fflist.Frames.Add(null);
            EditorUtility.SetDirty(target);
        }

        void OnListItemRemoved(ReorderableList list)
        {
            if(activeIndex == -1) { return; }

            fflist.Frames.RemoveAt(activeIndex);
            EditorUtility.SetDirty(target);
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Frames");
        }

    }
}
