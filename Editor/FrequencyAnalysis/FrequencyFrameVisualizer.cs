﻿// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Audio.FrequencyAnalysis;

namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    public class FrequencyFrameVisualizer : MonoBehaviour
    {

        [Header("Data")]
        [Tooltip("FrequencyFrameList to visualize")]
        public SpectrumFrameList FrameList;

        [Header("Debug")]
        [Tooltip("Whether to draw debug or not")]
        public bool drawDebug = true;
        [Tooltip("Vizualisation size")]
        public float2 rect = new float2(5.0f, 1.0f);
        [Tooltip("Unique frame to display")]
        public int index = -1;
        public Color frameColor = Color.gray;
        public Color highlightColor = Color.yellow;

        public AudioSource source = null;
        private FrequencyAnalyserSync analyzer;

        [Header("Analyzer settings")]
        public int bins = 512;
        public bool smooth = true;
        public float SmoothDownRate = 100f;
        public float scale = 1f;
        private FrameDataDictionary frameDataDict;
        public float SeekForward = 0f;

        private GUIStyle centeredText;
        private bool[] isSampled = new bool[64];


        // Start is called before the first frame update
        void Start()
        {
            frameDataDict = new FrameDataDictionary();
            frameDataDict.Add(FrameList);

            analyzer = new FrequencyAnalyserSync();
        }

        // Update is called once per frame
        void Update()
        {

            analyzer.doSmooth = smooth;
            analyzer.smoothDownRate = SmoothDownRate;
            analyzer.scale = scale;

            analyzer.audioSource = source;
            analyzer.Analyse(SeekForward);
            analyzer.ReadDataDictionary(frameDataDict);

        }

        private void InitGizmos()
        {
            centeredText = new GUIStyle();
            centeredText.alignment = TextAnchor.MiddleCenter;
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private void OnDrawGizmos()
        {
            if (!isActiveAndEnabled || !drawDebug) { return; }
         
            if(centeredText == null) { InitGizmos(); }
            
            DrawGrid();
            DrawFrames();

            if (!Application.isPlaying) { return; }
            DrawSpectrum();

        }


        private void DrawGrid()
        {

            float3 origin = transform.position, tl = origin, tr = origin, br = origin;

            tl.y += rect.y;
            tr.x += rect.x;
            tr.y = tl.y;
            br.x = tr.x;

            Color col = frameColor;
            Debug.DrawLine(origin, tl, col);
            Debug.DrawLine(tl, tr, col);
            Debug.DrawLine(tr, br, col);
            Debug.DrawLine(br, origin, col);

            float inc = rect.x / 64f;
            col *= 0.5f;

            for (int i = 0; i < 64; i++)
            {

                float px = i * inc + inc * 0.5f;

                float3 from = origin + new float3(px, 0, 0);
                float3 to = origin + new float3(px, rect.y, 0);
                Debug.DrawLine(from, to, col);
            }

        }

        private void DrawFrames()
        {

        }

        private void DrawSpectrum()
        {

            float linearXPos0 = 0f;
            float linearXPos1 = 0f;
            float currentFreq = 0f;

            float[] freqBands64 = analyzer.freqBands64;
            float[] freqBands8 = analyzer.freqBands8;

            float3 origin = transform.position;
            float3 prevTo = origin;

            float inc = rect.x / 64f;

            for (int i = 0; i < 64; i++)
            {

                linearXPos1 = i * inc + inc*0.5f;
                currentFreq = freqBands64[i];

                float3 from = origin + new float3(linearXPos1, 0, 0);
                float3 to = origin + new float3(linearXPos1, currentFreq * rect.y, 0);
                Debug.DrawLine(from, to, Color.white);
                Debug.DrawLine(prevTo, to, frameColor);

                if (isSampled[i]) { Debug.DrawLine(from, to, highlightColor); }

                prevTo = to;
            }

            for (int i = 1; i < 8; i++)
            {
                linearXPos0 = (float)(i - 1) / 7f;
                linearXPos1 = (float)(i) / 7f;

                float3 from = origin + new float3(linearXPos0 * rect.x, freqBands8[i - 1] * (rect.y/3.0f), 0);
                float3 to = origin + new float3(linearXPos1 * rect.x, freqBands8[i] * (rect.y / 3.0f), 0);

                Debug.DrawLine(from, to, Color.red * 0.1f);

            }

            if (!analyzer.audioSource) { return; }

            float seekPos = analyzer.audioSource.time / analyzer.audioSource.clip.length;
            float3 timePos = origin;
            timePos.x += seekPos * rect.x;
            timePos += (float3)(Vector3.up * (rect.y - 0.1f));

            Debug.DrawLine(timePos, timePos + (float3)(Vector3.up * 0.2f), frameColor);

            timePos = timePos + (float3)(Vector3.up * 0.2f);

            Handles.Label(timePos, ""+ analyzer.audioSource.time);

        }

    }

}
#endif