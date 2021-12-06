using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Nebukam.Audio.FrequencyAnalysis;

public class FrequencyAnalysisExample : MonoBehaviour
{

    public FrequencyFrameList FrameList;
    public AudioSource AudioSource;
    public FrequencyFrame ScaleID;
    public FrequencyFrame PositionID;
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
