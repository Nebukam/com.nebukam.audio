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

public class MicrophoneAnalysisExample : MonoBehaviour
{

    protected FrequencyAnalyser<AudioClipSpectrum<SingleChannel, FFTC>> m_frequencyAnalyser;
    protected FrameDataDictionary m_frameDataDictionary;

    public SpectrumFrame Frame;
    public Bins FrequencyBins = Bins.length1024;
    public bool ForceComplete = false;

    protected AudioClip m_microphoneClip;
    protected string m_deviceName;
    protected int m_minFreq = 0;
    protected int m_maxFreq = 0;

    private void Awake()
    {

    }

    private void OnEnable()
    {

        m_deviceName = Microphone.devices[0];
        Microphone.GetDeviceCaps(m_deviceName, out m_minFreq, out m_maxFreq);
        m_microphoneClip = Microphone.Start(Microphone.devices[0], true, 1, m_maxFreq);

        // Setup frequency analyser

        m_frequencyAnalyser = new FrequencyAnalyser<AudioClipSpectrum<SingleChannel, FFTC>>();
        m_frequencyAnalyser.spectrumProvider.audioClip = m_microphoneClip;

        // Setup frame data dictionary

        m_frameDataDictionary = new FrameDataDictionary();

        if (Frame != null)
            m_frameDataDictionary.Add(Frame);

        m_frequencyAnalyser.Add(m_frameDataDictionary);

    }

    void Start()
    {

    }

    void Update()
    {

        // Update

        m_frequencyAnalyser.spectrumProvider.frequencyBins = FrequencyBins;
        m_frequencyAnalyser.spectrumProvider.time = (float)(Microphone.GetPosition(m_deviceName)-((int)FrequencyBins*2)) / (float)m_maxFreq;

        m_frequencyAnalyser.Schedule(0f);

        if (!ForceComplete)
        {

            m_frequencyAnalyser.Schedule(0f);

            if (m_frequencyAnalyser.TryComplete())
            {

                Sample sample = m_frameDataDictionary.Get(Frame);

                DrawBands();

                m_frequencyAnalyser.Schedule(0f);

            }

        }
        else
        {
            m_frequencyAnalyser.Run();

            DrawBands();

        }



    }

    private void LateUpdate()
    {

    }

    private float windowWidth = 10f;
    private float windowSpacing = -1f;
    private Color col = Color.red;

    private void DrawBands()
    {

        //
        // We access the native collection just for the sake of the example 
        // This is a very slow performance-wise so don't do it at home kids.
        //

        float windowIndex = -1;
        col = Color.red;
        col.a = 0.25f;

        //Draw outputSamples (what we got from AudioClip.GetData)
        windowIndex++;
        NativeArray<float> rawSamples = m_frequencyAnalyser.spectrumProvider.samplesProvider.outputMultiChannelSamples;
        for (int i = 0, n = rawSamples.Length; i < n; i++)
        {
            float x = i * (windowWidth / n);
            float y = rawSamples[i];
            float z = windowIndex * windowSpacing;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), col);
        }

        //Draw outputSamples (isolated channel data)
        col = Color.yellow;
        col.a = 0.25f;
        windowIndex++;
        NativeArray<float> outputSamples = m_frequencyAnalyser.spectrumProvider.samplesProvider.outputSamples;
        for (int i = 0, n = outputSamples.Length; i < n; i++)
        {
            float x = i * (windowWidth / n);
            float y = outputSamples[i];
            float z = windowIndex * windowSpacing;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), col);
        }

        //Draw outputSpectrum (what we will use to compute bands, brackets etc)
        windowIndex++;
        col = Color.white;
        col.a = 0.25f;
        NativeArray<float> spectrum = m_frequencyAnalyser.spectrumProvider.outputSpectrum;
        for (int i = 0, n = spectrum.Length; i < n; i++)
        {
            float x = i * (windowWidth / n);
            float y = spectrum[i] * 10f;
            float z = windowIndex * windowSpacing;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), col);
        }

        //Draw bands & bracket (per available FrequencyTable)
        windowIndex++;
        col = Color.green;
        col.a = 0.25f;
        ISpectrumAnalysis dist = m_frequencyAnalyser.spectrumAnalysis;

        for (int d = 0; d < dist.Count; d++)
        {

            FTableProcessor tableProcessor = dist[d] as FTableProcessor;

            if (tableProcessor == null) { continue; }

            FrequencyTable table = tableProcessor.table;
            FBandsProcessor bandsExt = tableProcessor.spectrumDataExtraction.bandsExtraction;
            FBracketsExtraction bracketExt = tableProcessor.spectrumDataExtraction.bracketsExtraction;

            float z = windowIndex * windowSpacing;
            float inc = windowSpacing / 6f;

            DrawBands(bandsExt.Get(Bands.band128).outputBands, z + inc * 0f);
            DrawBands(bandsExt.Get(Bands.band64).outputBands, z + inc * 1f);
            DrawBands(bandsExt.Get(Bands.band32).outputBands, z + inc * 2f);
            DrawBands(bandsExt.Get(Bands.band16).outputBands, z + inc * 3f);
            DrawBands(bandsExt.Get(Bands.band8).outputBands, z + inc * 4f);

            DrawBrackets(bracketExt.outputBrackets, z + inc * 5f);

        }


    }

    private void DrawBands(NativeArray<float> bands, float z)
    {
        for (int i = 0, n = bands.Length; i < n; i++)
        {
            float x = i * (windowWidth / n);
            float y = bands[i] * 10f;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), col);
        }
    }

    private void DrawBrackets(NativeArray<BracketData> brackets, float z)
    {
        for (int i = 0, n = brackets.Length; i < n; i++)
        {
            BracketData bData = brackets[i];
            float x = i * (windowWidth / n);
            float y = bData.min * 10f;
            Debug.DrawLine(new Vector3(x, 0f, z), new Vector3(x, y, z), Color.blue);
            float y2 = bData.average * 10f;
            Debug.DrawLine(new Vector3(x, y, z), new Vector3(x, y2, z), Color.white);
            float y3 = bData.max * 10f;
            Debug.DrawLine(new Vector3(x, y2, z), new Vector3(x, y3, z), Color.red);
        }
    }

    private void OnDisable()
    {
        //
        // Make sure to DisposeAll on the frequency analyser, as
        // it is using a bulk of unmanaged resources.
        //
        m_frequencyAnalyser.DisposeAll();
    }
}
