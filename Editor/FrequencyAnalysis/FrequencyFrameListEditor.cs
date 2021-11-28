using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.Editor
{

    [CustomEditor(typeof(FrequencyFrameList))]
    public class FrequencyFrameListEditor : UnityEditor.Editor
    {

        internal static Dictionary<Object, bool> _expanded = new Dictionary<Object, bool>();

        internal ReorderableList listDrawer;
        internal SerializedProperty fflistSerialized;
        internal FrequencyFrameList fflist;

        internal Rect lastRectSize = new Rect(0,0,0,0);
        internal int activeIndex = 0;

        public void Update()
        {
            Repaint();
        }

        private void OnEnable()
        {

            fflistSerialized = serializedObject.FindProperty("Frames");
            listDrawer = new ReorderableList(serializedObject, fflistSerialized, true, true, true, true);
            listDrawer.drawElementCallback = DrawListItems;
            listDrawer.drawHeaderCallback = DrawHeader;
            listDrawer.onRemoveCallback = OnListItemRemoved;
            listDrawer.onAddCallback = OnListItemAdded;

            activeIndex = -1;

        }

        public override void OnInspectorGUI()
        {

            fflist = target as FrequencyFrameList;
            serializedObject.Update();

            ToggleLayoutMode(true);
            SetR(new Rect(20f, 20f, Screen.width - 40f, 20f));

            if (Button("Open in Frequency Analyzer")) { FrequencyAnalyserWindow.ShowWindow(fflist); }

            listDrawer.elementHeight = 260f;
            listDrawer.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            ToggleLayoutMode(false);

            if (isActive)
                activeIndex = index;

            FrequencyFrame frame = fflist.Frames[index];
                        

            Rect r = new Rect(rect.x, rect.y, rect.width - 10f, EditorGUIUtility.singleLineHeight);

            SetR(r);

            Space(8f);
            Line();
            Space(8f);

            int changes = ObjectField(ref frame, "");
            fflist.Frames[index] = frame;

            if (frame == null) { return; }

            FrequencyFrameEditor.PrintFrequencyFrameEditor(frame, false);

            //if (ObjectField<FrequencyFrame>(ref frame) > 0)
              //  frames.Frames[index] = frame;

            //Space(4f);

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
