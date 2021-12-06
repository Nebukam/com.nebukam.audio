using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Nebukam.Editor;
using static Nebukam.Editor.EditorDrawer;
using static Nebukam.Editor.EditorGLDrawer;
using Nebukam.Audio.FrequencyAnalysis;
using Nebukam.Audio.Editor;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    public class FrequencyAnalyserMonitor : EditorWindow
    {

        private const string m_title = "FAnalysis Monitor";

        [MenuItem("N:Toolkit/Audio/FAnalysis monitor")]
        public static void ShowWindow()
        {
            FrequencyAnalyserWindow window = EditorWindow.GetWindow(typeof(FrequencyAnalyserWindow), false, m_title, true) as FrequencyAnalyserWindow;
            window.Refresh();
        }

        public static void ShowWindow(FrequencyFrameList frameList)
        {
            ShowWindow();
        }

        private void Awake()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            AudioPlayer.Stop();
        }

        public void Refresh()
        {
            
        }

        public void Update()
        {
            Repaint();
        }

        void OnGUI()
        {

        }

    }
}
