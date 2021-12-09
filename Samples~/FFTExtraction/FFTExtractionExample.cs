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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nebukam.Audio.FrequencyAnalysis;
using Nebukam.Audio;
using Unity.Collections;

public class FFTExtractionExample : MonoBehaviour
{

    protected AudioClipSpectrum<SingleChannel, FFTC> m_audioClipSpectrum;

    public AudioClip Clip;
    public Bins FrequencyBins = Bins.length1024;
    public float Time = 1f;

    private void OnEnable()
    {
        m_audioClipSpectrum = new AudioClipSpectrum<SingleChannel, FFTC>();
    }

    void Update()
    {

        if (Clip == null) { return; }

        m_audioClipSpectrum.audioClip = Clip;
        m_audioClipSpectrum.time = Time;
        m_audioClipSpectrum.frequencyBins = FrequencyBins;

        if (m_audioClipSpectrum.TryComplete())
            DrawBands();

        m_audioClipSpectrum.Schedule(0f);
        
    }

    private void DrawBands()
    {

        //
        // We access the native collection just for the sake of the example 
        // This is a very slow performance-wise so don't do it at home kids.
        //

        float windowWidth = 10f;
        float windowSpacing = -1f;
        float windowIndex = -1;
        Color col = Color.red;
        col.a = 0.25f;

        NativeArray<float> spectrum = m_audioClipSpectrum.outputSpectrum;
        for (int i = 0, n = spectrum.Length; i < n; i++)
        {
            float x = i * (windowWidth / n);
            float y = spectrum[i] * 10f;
            float z = windowIndex * windowSpacing;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), col);
        }


    }

    private void OnDisable()
    {
        //
        // Make sure to DisposeAll on the frequency analyser, as
        // it is using a bulk of unmanaged resources.
        //
        m_audioClipSpectrum.DisposeAll();
    }
}
