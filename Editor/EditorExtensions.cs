using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

namespace Nebukam.Audio.Editor
{
    public static class EditorExtensions
    {

        #region enum

        public static int EnumPopup<T>(ref T e, string label = "")
            where T : Enum
        {
            T r;
            if (label != "")
                r = (T)EditorGUILayout.EnumPopup(label, e);
            else
                r = (T)EditorGUILayout.EnumPopup(e);

            if (r.Equals(e)) { return 0; }

            e = r;
            return 1;

        }

        public static int EnumGrid<T>(ref T e, string label = "")
        {
            T[] eList = (T[])Enum.GetValues(typeof(T));
            string[] list = new string[eList.Length];
            int sel = 0, i = 0;
            foreach (T ee in eList)
            {
                list[i] = ee.ToString();
                if (ee.Equals(e)) { sel = i; }
                i++;
            }

            GUILayout.BeginHorizontal();

            if (label != "")
            {
                GUILayout.Label(label);
            }

            //GUILayout.FlexibleSpace();

            //int selection = GUILayout.SelectionGrid(sel, list, eList.Length);
            i = 0;
            int selection = -1;
            foreach (string S in list)
            {
                if (GUILayout.Toggle((sel == i), S, EditorStyles.toolbarButton)) { selection = i; }
                i++;
            }


            //if (label != "")
            GUILayout.EndHorizontal();

            if (selection == -1 || selection == sel) { return 0; }

            e = eList[selection];
            return 1;
        }

        #endregion

        #region bool

        public static int Checkbox(ref bool value, string label)
        {
            bool input = EditorGUILayout.Toggle(label, value);
            if (input == value) { return 0; }
            value = input;
            return 1;
        }

        #endregion

        #region string

        public static int TextInput(ref string value, string label = "")
        {
            string input;
            if (label != "")
                input = EditorGUILayout.TextField(label, value);
            else
                input = EditorGUILayout.TextField(value);

            if (input == value) { return 0; }
            value = input;
            return 1;
        }

        public static int PathInput(ref string value, string label = "", bool folder = false)
        {

            GUILayout.BeginHorizontal();

            int result = TextInput(ref value, label);

            if (folder)
            {
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string input = EditorUtility.OpenFolderPanel("Pick a folder...", "...", "");
                    value = input;
                }
            }
            else if (result == 0)
            {
                GUILayout.EndHorizontal();
                return 0;
            }

            value = value.Replace("\\", "/");

            if (folder && value.Length > 0 && value.Substring(value.Length - 1) != "/")
                value += "/";

            GUILayout.EndHorizontal();

            return 1;

        }

        #endregion

        #region int

        public static int Slider(ref int value, int min, int max, string label = "")
        {
            int input;
            if (label != "")
                input = EditorGUILayout.IntSlider(label, value, min, max);
            else
                input = EditorGUILayout.IntSlider(value, min, max);

            if (input == value) { return 0; }
            value = input;
            return 1;
        }

        internal static int MinMaxSlider(ref int2 values, int2 range, string label = "")
        {
            float
                rx = values.x,
                ry = values.y;

            if (label != "")
                GUILayout.Label(label, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            rx = EditorGUILayout.IntField((int)rx, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref rx, ref ry, range.x, range.y);
            ry = EditorGUILayout.IntField((int)ry, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            if (values.x != (int)rx || values.y != (int)ry)
            {
                values.x = (int)rx;
                values.y = (int)ry;
                return 1;
            }

            return 0;

        }

        internal static int StartSizeSlider(ref int2 values, int2 range, string label = "")
        {
            float
                rx = values.x,
                ry = rx + values.y;

            if (label != "")
                GUILayout.Label(label, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            rx = EditorGUILayout.IntField((int)rx, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref rx, ref ry, range.x, range.y);
            //if(ry != (values.x + values.y))
                ry = (int)ry - (int)rx;
            
            ry = EditorGUILayout.IntField((int)ry, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();


            if (values.x != (int)rx || values.y != (int)ry)
            {
                values.x = (int)rx;
                values.y = (int)ry;
                return 1;
            }

            return 0;

        }

        #endregion

        #region float

        internal static int MinMaxSlider(ref float2 values, float2 range, string label = "")
        {
            float
                rx = values.x,
                ry = values.y;


            if (label != "")
                GUILayout.Label(label, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            rx = EditorGUILayout.FloatField(rx, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref rx, ref ry, range.x, range.y);
            ry = EditorGUILayout.FloatField(ry, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            if (values.x != rx || values.y != ry)
            {
                values.x = rx;
                values.y = ry;
                return 1;
            }

            return 0;

        }

        internal static int StartSizeSlider(ref float2 values, float2 range, string label = "")
        {
            float
                rx = values.x,
                ry = rx + values.y;

            if (label != "")
                GUILayout.Label(label, EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();
            rx = EditorGUILayout.FloatField(rx, GUILayout.Width(30));
            EditorGUILayout.MinMaxSlider(ref rx, ref ry, range.x, range.y);
            //if(ry != (values.x + values.y))
            ry = ry - rx;

            ry = EditorGUILayout.FloatField(ry, GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();


            if (values.x != rx || values.y != ry)
            {
                values.x = rx;
                values.y = ry;
                return 1;
            }

            return 0;

        }


        internal static int FloatField(ref float value, string label = "")
        {

            float input;

            if (label != "")
                input = EditorGUILayout.FloatField("Scale", value);
            else
                input = EditorGUILayout.FloatField(value);

            if (value == input) { return 0; }

            value = input;
            return 1;

        }

        #endregion

        #region Colors

        internal static int ColorField(ref Color col, string label = "")
        {
            Color newCol;
            if (label != "")
                newCol = EditorGUILayout.ColorField(label, col);
            else
                newCol = EditorGUILayout.ColorField(col);

            if (newCol == col) { return 0; }

            col = newCol;
            return 1;

        }

        internal static int ColorSquare(ref Color col, string label = "")
        {
            Color newCol;
            newCol = EditorGUILayout.ColorField(GUIContent.none, col, false, true, false, GUILayout.Width(30));

            if (newCol == col) { return 0; }

            col = newCol;
            return 1;

        }

        #endregion

        #region Misc

        internal static void Line(float w = 1f, int i_height = 1)
        {

            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;
            rect.x = rect.x + (rect.width * (1f - w)) * 0.5f;
            rect.width = rect.width * w;

            EditorGUILayout.Space(2f);
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.5f));
            EditorGUILayout.Space(2f);

        }

        #endregion

    }
}
