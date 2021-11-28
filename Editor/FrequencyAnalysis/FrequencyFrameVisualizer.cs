using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Audio.FrequencyAnalysis;

namespace Nebukam.Audio.Editor
{

    public class FrequencyFrameVisualizer : MonoBehaviour
    {

        [Header("Data")]
        [Tooltip("FrequencyFrameList to visualize")]
        public FrequencyFrameList FrameList;

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
        private FrequencyAnalyser analyzer;

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

            analyzer = new FrequencyAnalyser();
        }

        // Update is called once per frame
        void Update()
        {

            analyzer.doSmooth = smooth;
            analyzer.smoothDownRate = SmoothDownRate;
            analyzer.scale = scale;

            analyzer.audioSource = source;
            analyzer.Analyze(SeekForward);
            analyzer.UpdateFrameData(frameDataDict);

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

            if (FrameList == null) { return; }

            int targetIndex = index;
            List<FrequencyFrame> defList = FrameList.Frames;

            if (targetIndex < 0 || targetIndex > defList.Count + 1) { targetIndex = -1; }

            float inc = rect.x / 64;
            float3 origin = transform.position;

            for(int i = 0; i < 64; i++) { isSampled[i] = false; }

            for (int i = 0, n = defList.Count; i < n; i++)
            {

                if (targetIndex != -1 && i != targetIndex) { continue; }

                FrequencyFrame def = defList[i];

                if(def == null) { continue; }

                float ampMax = def.bands == Bands.Eight ? FrequencyFrame.maxAmplitude8 : FrequencyFrame.maxAmplitude64;
                float ampStart = def.amplitude.x, ampSize = def.amplitude.y;
                int freqStart = def.frequency.x, freqSize = def.frequency.y;

                for(int j = def.frequency.x; j < clamp(def.frequency.x + max(1, def.frequency.y),0,64); j++) { isSampled[j] = true; }

                float3 bl = origin;
                bl.x += (freqStart * inc);
                bl.y += (ampStart / ampMax * rect.y);
                float3 tl = bl, tr = bl, br = bl;

                tl.y += ampSize / ampMax * rect.y;

                tr.x += freqSize * inc;
                tr.y = tl.y;

                br.x = tr.x;

                Color c = def.color;
                
                if(frameDataDict != null)
                {
                    float3 center = bl;
                    float sval = frameDataDict.Get(def.ID);
                    center.x += (freqSize * inc) * 0.5f;
                    center.y += (ampSize / ampMax * rect.y) * 0.5f;
                    
                    if(sval == 0f) { c.a = 0.5f; }
                    centeredText.normal.textColor = c;
                    Handles.Label(center, "" + sval, centeredText);

                }

                Debug.DrawLine(bl, tl, c);
                Debug.DrawLine(tl, tr, c);
                Debug.DrawLine(tr, br, c);
                Debug.DrawLine(br, bl, c);

            }

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