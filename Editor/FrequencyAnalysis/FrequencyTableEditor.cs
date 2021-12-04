using Nebukam.Audio.FrequencyAnalysis;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Nebukam.Editor;
using static Nebukam.Editor.EditorGLDrawer;
using static Nebukam.Editor.EditorDrawer;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    [CustomEditor(typeof(FrequencyTable))]
    public class FrequencyTableEditor : UnityEditor.Editor
    {

        internal ReorderableList m_listDrawer;
        internal SerializedProperty m_freqTableIntsSerialized;
        internal FrequencyTable m_table;

        internal int activeIndex = 0;

        internal static Bands m_previewBands = Bands.band128;
        internal static Bins m_previewBins = Bins.length512;

        public override bool RequiresConstantRepaint() { return true; }

        private void OnEnable()
        {

            m_freqTableIntsSerialized = serializedObject.FindProperty("Brackets");

            UpdateList();

            activeIndex = -1;

            (target as FrequencyTable).BuildTable();

        }

        private void UpdateList()
        {

            m_listDrawer = new ReorderableList(serializedObject, m_freqTableIntsSerialized, true, true, true, true);
            m_listDrawer.drawElementCallback = DrawListItems;
            m_listDrawer.drawHeaderCallback = DrawHeader;
            m_listDrawer.onAddCallback = OnListItemAdded;
            m_listDrawer.elementHeightCallback = OnListElementHeight;
            m_listDrawer.elementHeight = EditorGUIUtility.singleLineHeight;

        }

        public override void OnInspectorGUI()
        {

            m_table = target as FrequencyTable;

            serializedObject.Update();

            __RequireRectUpdate(true);
            __SetRect(new Rect(20f, 20f, Screen.width - 40f, 20f));

            if (Button("Rebuild Table")) { m_table.BuildTable(); }

            //m_table.UseRawDistribution = EditorGUI.Toggle(__GetRect(), m_table.UseRawDistribution);

            DrawPreview();

            bool lockedTable = false;

            if(m_table == FrequencyTable.tableCommon 
                || m_table == FrequencyTable.tableFull)
            {
                lockedTable = true;

                HelpBox("This is a default FrequencyTable.\nIf you need to modify it consider creating your own instead.", MessageType.Warning, 32f, 10f);
            }

            EditorGUI.BeginDisabledGroup(lockedTable);

            m_listDrawer.DoLayoutList();

            EditorGUI.EndDisabledGroup();

            __RequireRectUpdate(true);

            serializedObject.ApplyModifiedProperties();

        }

        void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {

            __RequireRectUpdate(false);

            if (isActive)
                activeIndex = index;

            int prevHz = index == 0 ? 0 : m_table.Brackets[index - 1];
            int currentHz = m_table.Brackets[index];
            int nextHz = index == m_table.Brackets.Count - 1 ? currentHz + 1 : m_table.Brackets[index + 1];
            int limit = 24000;

            Rect r = new Rect(rect.x, rect.y + 4f, rect.width - 10f, EditorGUIUtility.singleLineHeight);
            __SetRect(r);

            if (index == 0)
            {
                Label("0");
                Space(6f);
            }

            bool error = currentHz < prevHz || currentHz > nextHz;

            if (error)
            {
                GUI.backgroundColor = Color.red;
                __BeginInLine(r.width - 30);
            }

            if (IntFieldInline(ref currentHz) == 1) //, "from " + prevHz + " to "
            {

                if (currentHz <= prevHz)
                    currentHz = prevHz + 1;

                if (currentHz <= 0)
                    currentHz = 1;

                if (currentHz > limit - 1)
                    currentHz = limit - 1;

                m_table.Brackets[index] = currentHz;
                EditorUtility.SetDirty(target);

            }


            if (error)
            {
                __NextInline(30f);
                if (Button("Fix"))
                {
                    m_table.Brackets.Sort();
                }
                __EndInLine();
            }

            if (index == m_table.Brackets.Count - 1)
            {
                Space(6f);
                Label("" + limit);
            }

        }

        protected void DrawPreview()
        {

            float binSize = (float)FrequencyTable.maxHz / (float)(int)m_previewBins;
            float bandSize = (float)FrequencyTable.maxHz / (float)(int)m_previewBands;

            if (BeginGL(200f))
            {
                Color col = Color.black;
                col.a = 0.25f;
                GLFill(col);

                BandInfos[] bandInfos;
                m_table.GetBandInfos(out bandInfos, m_previewBands);

                float
                    height = GLArea.height,
                    width = GLArea.width / bandInfos.Length,
                    max = m_table.GetBandInfosPair(m_previewBands).GetPeak(m_previewBins);

                Rect quad = new Rect();
                quad.width = width -2;

                Color bad = Color.red;
                bad.a = 0.25f;

                BandInfos infos;
                
                for (int i = 0; i < bandInfos.Length; i++)
                {
                    infos = bandInfos[i];
                    float a = infos.Length(m_previewBins);
                    float b = max;// (int)m_previewBins;
                    float nrm = (a / b);

                    GL.Begin(GL.QUADS);

                    if (a == 0f)
                    {
                        GL.Color(bad);
                        nrm = 1f;
                    }
                    else if(a == 1f)
                    {
                        GL.Color(Color.white);
                    }
                    else
                    {
                        GL.Color(Color.gray);
                    }

                    quad.x = i * width + 1;
                    quad.height = height * nrm;
                    quad.width = width-2;
                    quad.y = height - quad.height;

                    
                    GLRect(quad);
                    GL.End();

                }

                

                EndGL();
            }

            __BeginCol(2);
            EnumInlined(ref m_previewBands, true, "Preview bands");
            __NextCol();
            EnumInlined(ref m_previewBins, true, "Preview bins");
            __EndCol();

            string str = "Individual bin size : "+binSize+"Hz\nIndividual band size : "+bandSize+"Hz";

            Space(4f);
            HelpBox(str, MessageType.None, 30f);
        }

        void OnListItemAdded(ReorderableList list)
        {
            if(m_table.Brackets == null)
                m_table.Brackets = new List<int>();
            
            if (m_table.Brackets.Count == 0)
                m_table.Brackets.Add(16);
            else
                m_table.Brackets.Add(m_table.Brackets[m_table.Brackets.Count - 1] + 10);

            EditorUtility.SetDirty(target);

        }

        float OnListElementHeight(int index)
        {
            float baseSize = index == 0 || index == m_table.Brackets.Count - 1
                ? EditorGUIUtility.singleLineHeight * 2f + 8f : EditorGUIUtility.singleLineHeight;

            if(m_table.Brackets.Count == 1)
            {
                baseSize += 20f;
            }

            return baseSize + 6f;
        }

        //Draws the header
        void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Frequency brackets");
        }

    }
}
