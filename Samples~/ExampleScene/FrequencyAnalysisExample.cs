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
using Unity.Mathematics;
using Nebukam.Audio.FrequencyAnalysis;

public class FrequencyAnalysisExample : MonoBehaviour
{

    public FrequencyFrameList FrameList;
    public AudioSource AudioSource;
    public SpectrumFrame ScaleID;
    public SpectrumFrame PositionID;
    public float SmoothDownRate = 10f;
    public float SeekForward = 0f;

    private FrequencyAnalyserSync m_analyzer;
    private FrameDataDictionary m_frameDataDict;

    // Start is called before the first frame update
    void Start()
    {

        // Set up analyzer
        m_analyzer = new FrequencyAnalyserSync();
        m_analyzer.audioSource = AudioSource;
        m_analyzer.smoothDownRate = SmoothDownRate;

        //Create a new SamplingData object -- this is where the Analyzer will write data
        m_frameDataDict = new FrameDataDictionary();

        //Add one or more "Definitions" to the SamplingData for the Analyzer to use
        m_frameDataDict.Add(FrameList);

    }

    // Update is called once per frame
    void Update()
    {

        // Update parameters if they changed in the editor in play mode
        m_analyzer.smoothDownRate = SmoothDownRate;

        // Update Analyzer so it compute the current AudioSource
        m_analyzer.Analyse(SeekForward);

        // Ask Analyzer to write data into a SamplingData Object
        m_analyzer.ReadDataDictionary(m_frameDataDict);

        // Use the data!
        float value = m_frameDataDict.Get(ScaleID);
        transform.localScale = new float3(value, value, value);

        transform.localPosition = transform.up * m_frameDataDict.Get(PositionID);

    }
}
