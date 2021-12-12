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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using Nebukam.Audio;
using Nebukam.Audio.FrequencyAnalysis;
using static Nebukam.Editor.EditorGLDrawer;
using static Nebukam.Editor.EditorDrawer;


namespace Nebukam.Audio.FrequencyAnalysis.Editor
{

    public static class FrequencyAnalysis
    {

        public static SpectrumAnalyser<AudioClipSpectrum<SingleChannel, FFT4>> spectrumAnalyser;
        public static FrameDataDictionary spectrumData;

        static FrequencyAnalysis()
        {

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

            FrequencyTable.__onFrequencyTableAdded.Add(OnFrequencyTableAdded);
            FrequencyTable.__onFrequencyTableRemoved.Add(OnFrequencyTableRemoved);

            Init();

        }

        internal static void OnBeforeAssemblyReload()
        {
            // Dispose of analysers
            Release();
        }

        internal static void OnAfterAssemblyReload()
        {
            // Rebuild analysers
            Init();
        }

        internal static void Init()
        {

            spectrumAnalyser = new SpectrumAnalyser<AudioClipSpectrum<SingleChannel, FFT4>>();
            spectrumAnalyser.spectrumAnalysis.cacheFrameData = false;

            if (FrequencyTable.__loadedFrequencyTableList != null)
            {
                foreach (FrequencyTable table in FrequencyTable.__loadedFrequencyTableList)
                    spectrumAnalyser.Add(table);
            }

            spectrumData = new FrameDataDictionary();
            spectrumAnalyser.Add(spectrumData);

        }

        internal static void Release()
        {
            spectrumAnalyser?.DisposeAll();
            spectrumData.Clear();
            spectrumData = null;
        }

        internal static void OnFrequencyTableAdded(FrequencyTable table)
        {
            spectrumAnalyser.Add(table);
        }

        internal static void OnFrequencyTableRemoved(FrequencyTable table)
        {
            spectrumAnalyser.Remove(table);
        }

        public static void SetCurrentFrameList(SpectrumFrameList list)
        {

            spectrumData.Clear();

            if (list != null)
                spectrumData.Add(list);

        }

        public static void Run(AudioClip clip, float time)
        {

            if (time < 0f) { time = 0f; }
            if (time > clip.length) { time = clip.length; }

            spectrumAnalyser.spectrumProvider.audioClip = clip;
            spectrumAnalyser.spectrumProvider.time = time;
            spectrumAnalyser.Run();

        }

    }

}

#endif