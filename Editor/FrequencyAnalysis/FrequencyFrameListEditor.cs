using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static Nebukam.Editor.EditorExtensions;

namespace Nebukam.Audio.Editor
{

    [CustomEditor(typeof(FrequencyFrameList))]
    public class FrequencyFrameListEditor : UnityEditor.Editor
    {

        internal static Dictionary<Object, bool> _expanded = new Dictionary<Object, bool>();

        internal ReorderableList listHandler;
        internal SerializedProperty frameList;
        internal FrequencyFrameList frames;

        internal Rect lastRectSize = new Rect(0,0,0,0);
        internal int activeIndex = 0;

        private void OnEnable()
        {
            frameList = serializedObject.FindProperty("Frames");
            listHandler = new ReorderableList(serializedObject, frameList, true, true, true, true);
            listHandler.drawElementCallback = DrawListItems;
            listHandler.drawHeaderCallback = DrawHeader;
            listHandler.onRemoveCallback = OnListItemRemoved;
            listHandler.onAddCallback = OnListItemAdded;

        }

        public override void OnInspectorGUI()
        {

            frames = target as FrequencyFrameList;

            ToggleLayoutMode(true);

            listHandler.elementHeight = 160f;

            serializedObject.Update();

            activeIndex = -1;

            listHandler.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            ToggleLayoutMode(false);

            if (isActive)
                activeIndex = index;

            FrequencyFrame frame = frames.Frames[index];
                        

            Rect r = new Rect(rect.x, rect.y, rect.width - 10f, EditorGUIUtility.singleLineHeight);

            SetR(r);

            Space(8f);
            Line();
            Space(8f);

            int changes = ObjectField(ref frame, "");
            frames.Frames[index] = frame;

            if (frame == null) { return; }

            FrequencyFrameEditor.PrintFrequencyFrameEditor(frame, false);

            //if (ObjectField<FrequencyFrame>(ref frame) > 0)
              //  frames.Frames[index] = frame;

            //Space(4f);

        }

        void OnListItemAdded(ReorderableList list)
        {
            //if (activeIndex == -1) { return; }
            frames.Frames.Add(null);
            EditorUtility.SetDirty(target);
        }

        void OnListItemRemoved(ReorderableList list)
        {
            if(activeIndex == -1) { return; }

            frames.Frames.RemoveAt(activeIndex);
            EditorUtility.SetDirty(target);
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Frames");
        }

    }
}
