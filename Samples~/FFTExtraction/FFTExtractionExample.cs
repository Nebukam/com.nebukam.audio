using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nebukam.Audio.FrequencyAnalysis;
using Nebukam.Audio;
using Unity.Collections;

public class FFTExtractionExample : MonoBehaviour
{

    protected AudioClipSpectrum<SingleChannel> m_audioClipSpectrum;

    public AudioClip Clip;
    public Bins FrequencyBins = Bins.length1024;
    public float Time = 1f;

    private void OnEnable()
    {
        m_audioClipSpectrum = new AudioClipSpectrum<SingleChannel>();
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
