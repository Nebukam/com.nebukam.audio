using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Nebukam.FrequencyAnalysis;

public class ExampleScript : MonoBehaviour
{

    public SamplingDefinitionList Definitions;
    public AudioSource AudioSource;
    public string ScaleID = "A";
    public string PositionID = "B";
    public float SmoothDownRate = 10f;

    private FrequencyBandAnalyser _Analyzer;
    private SamplingData _SamplingData;



    // Start is called before the first frame update
    void Start()
    {

        // Set up analyzer
        _Analyzer = new FrequencyBandAnalyser();
        _Analyzer.audioSource = AudioSource;
        _Analyzer.smoothDownRate = SmoothDownRate;

        //Create a new SamplingData object -- this is where the Analyzer will write data
        _SamplingData = new SamplingData();

        //Add one or more "Definitions" to the SamplingData for the Analyzer to use
        _SamplingData.Add(Definitions);

    }

    // Update is called once per frame
    void Update()
    {

        // Update parameters if they changed in the editor in play mode
        _Analyzer.smoothDownRate = SmoothDownRate;

        // Update Analyzer so it compute the current AudioSource
        _Analyzer.Update();

        // Ask Analyzer to write data into a SamplingData Object
        _Analyzer.UpdateSamplingData(_SamplingData);

        // Use the data!
        float value = _SamplingData.Get(ScaleID);
        transform.localScale = new float3(value, value, value);

        transform.localPosition = transform.up * _SamplingData.Get(PositionID);

    }
}
