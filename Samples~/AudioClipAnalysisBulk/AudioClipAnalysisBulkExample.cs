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

public class AudioClipAnalysisBulkExample : MonoBehaviour
{

    protected FrequencyAnalyserBulk<SingleChannel, FFTC> m_frequencyAnalyser;
    protected FrameDataDictionary m_frameDataDictionary;

    public SpectrumFrame Frame;
    public AudioClip Clip;
    public int BulkSize = 20;
    public Bins FrequencyBins = Bins.length1024;
    public float Time = 1f;

    private void Awake()
    {

    }

    private void OnEnable()
    {
        //
        // First, create a FrequencyAnalyser object.
        // The basic analyser requires to be instanciated with a "SpectrumProvider"
        // The following types of SpectrumProvider are readily available :
        //      - AudioSourceSpectrum
        //      - AudioListenerSpectrum
        //      - AudioClipSpectrum
        //
        // The first two are only capable of analysing the "live" audio for obvious reasons
        // However the last one can fetch data from anywhere inside an AudioClip, effectively
        // allowing "in advance" analysis.
        //

        m_frequencyAnalyser = new FrequencyAnalyserBulk<SingleChannel, FFTC>();

        //
        // Next, create a FrameDataDictionary.
        // This is a basic object into which you can register
        // "FrequencyFrame". Their values will be extracted and stored
        // inside the FrameDataDictionary during the analysis.
        //

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

        if (Clip == null) { return; }

        //
        // Set the audio clip to analyse.
        // This doesn't need to happen during the Update, once set it's good to go
        // But it can be changed anytime.
        //

        m_frequencyAnalyser.bulkSize = BulkSize;
        m_frequencyAnalyser.audioClip = Clip;
        m_frequencyAnalyser.time = Time;
        m_frequencyAnalyser.frequencyBins = FrequencyBins;

        //
        // The frequency analyser is running on Unity's job system
        // Hence you need to Schedule the analysis.
        //

        m_frequencyAnalyser.Schedule(0f);

        //
        // Check for completion...
        //
        if (m_frequencyAnalyser.TryComplete())
        {

            // Once the analysis is complete you can access the frames
            // output directly inside the FrameDataDictionary like so :

            Sample sample = m_frameDataDictionary[Frame];

            //Debug.Log(sample.average);
            //DrawBands();

            // Make sure to schedule it again : if complete, it couldn't
            // be scheduled through the last call.
            // Not doing that will introduce a 1-frame gap in anlysis.
            // However we need to do so _after_ drawing debug, as accessing
            // the ressources is not permitted once engaged in a job.
            m_frequencyAnalyser.Schedule(0f);

        }

        



    }
    /*
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
        NativeArray<float> rawSamples = m_frequencyAnalyser.spectrumProvider.channelSamplesProvider.outputMultiChannelSamples;
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
        NativeArray<float> outputSamples = m_frequencyAnalyser.spectrumProvider.channelSamplesProvider.outputSamples;
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
        SpectrumAnalysis dist = m_frequencyAnalyser.distribution;
        
        for (int d = 0; d < dist.Count; d++)
        {

            FrequencyTableProcessor tableProcessor = dist[d] as FrequencyTableProcessor;

            if (tableProcessor == null) { continue; }

            FrequencyTable table = tableProcessor.table;
            FBandsProcessor bandsExt = tableProcessor.spectrumDataExtraction.bandsExtraction;
            FBracketsExtraction bracketExt = tableProcessor.spectrumDataExtraction.bracketsExtraction;

            float z = windowIndex * windowSpacing;
            float inc = windowSpacing / 6f;

            DrawBands(bandsExt.outputBand128, z + inc * 0f);
            DrawBands(bandsExt.outputBand64, z + inc * 1f);
            DrawBands(bandsExt.outputBand32, z + inc * 2f);
            DrawBands(bandsExt.outputBand16, z + inc * 3f);
            DrawBands(bandsExt.outputBand8, z + inc * 4f);

            DrawBrackets(bracketExt.outputBrackets, z + inc * 5f);

        }


    }

    private void DrawBands(NativeArray<float> bands, float z)
    {
        for(int i = 0, n = bands.Length; i < n; i++)
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

    */
    private void OnDisable()
    {
        //
        // Make sure to DisposeAll on the frequency analyser, as
        // it is using a bulk of unmanaged resources.
        //
        m_frequencyAnalyser.DisposeAll();
    }
}
