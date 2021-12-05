using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum FrequencyExtraction
    {
        Bands, // Read aggregated band data
        Bracket, // Read frequency "bands" as defined in the FrequencyTable (as if they were custom bands)
        Raw // Read raw frequency
    }

    public enum OutputType
    {
        Trigger, // 0f-1f
        Peak, // 0.0f...1.0f
        Average, // 0.5f
        Sum // Sums all values in frame.
    }

    public enum Bands
    {
        band8 = 8,
        band16 = 16,
        band32 = 32,
        band64 = 64,
        band128 = 128
    }

    public enum Tolerance
    {
        Loose, // Any
        Strict // All frequencies need to be accounted for
    }

    public struct Sample
    {

        public float average;
        public float trigger;
        public float peak;
        public float sum;

        public bool justTriggered;

        public OutputType output;

        public bool ON { get { return trigger > 0f; } }
        public float Default { get { return (float)this; } }
        public float Value(OutputType type)
        {
            if (type == OutputType.Trigger) { return trigger; }
            if (type == OutputType.Peak) { return peak; }
            if (type == OutputType.Average) { return average; }
            if (type == OutputType.Sum) { return sum; }

            return average;
        }

        public static implicit operator float(Sample s)
        {
            return s.Value(s.output);
        }

        public override string ToString()
        {
            return String.Format("A:{0}, T:{1}({5}), P:{2}, S:{3}, {4}", average, trigger, peak, sum, output, justTriggered);
        }

    }

    public struct FrequencyFrameData
    {
        public Bands bands;
        public FrequencyExtraction extraction;
        public OutputType output;
        public Tolerance tolerance;
        public int2 frequenciesBand;
        public int2 frequenciesBracket;
        public int2 frequenciesRaw;
        public float2 amplitude;
        public float inputScale;
        public float outputScale;
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "Frequency Frame", menuName = "N:Toolkit/Audio/Frequency Frame", order = 1)]
    public class FrequencyFrame : ScriptableObject
    {

        public const float maxAmplitude8 = 3.0f;
        public const float maxAmplitude64 = 1.0f;

        public string ID = "Unidentified Sample";

        public FrequencyTable table;

        public Bands bands = Bands.band64;

        public OutputType output = OutputType.Average;

        public FrequencyExtraction extraction = FrequencyExtraction.Bands;

        public Tolerance tolerance = Tolerance.Loose;

        public int2 frequenciesBand = new int2(0, 1);

        public int2 frequenciesBracket = new int2(0, 1);

        public int2 frequenciesRaw = new int2(0, 1);

        public float2 amplitude = new float2(0f, 1f);

        public float inputScale = 1f;

        public float outputScale = 1f;

        public Color color = Color.red;

#if UNITY_EDITOR

        /// <summary>
        /// Ensure frequency is within supported limits
        /// </summary>
        public void Validate()
        {
            frequenciesBand = new int2(
                clamp(frequenciesBand.x, 0, 63),
                clamp(frequenciesBand.y, 0, 63));

            amplitude = new float2(
                clamp(amplitude.x, 0f, 10f),
                clamp(amplitude.y, 0f, 10f));
        }

#endif

        /// <summary>
        /// Copy data to this class from a struct
        /// </summary>
        /// <param name="data"></param>
        /// <param name="copyID"></param>
        public void Copy(FrequencyFrameData data)
        {
            bands = data.bands;
            output = data.output;
            extraction = data.extraction;
            tolerance = data.tolerance;
            frequenciesBand = data.frequenciesBand;
            frequenciesBracket = data.frequenciesBracket;
            frequenciesRaw = data.frequenciesRaw;
            amplitude = data.amplitude;
            inputScale = data.inputScale;
            outputScale = data.outputScale;
        }

        public static implicit operator FrequencyFrameData(FrequencyFrame value)
        {
            return new FrequencyFrameData()
            {
                bands = value.bands,
                output = value.output,
                extraction = value.extraction,
                tolerance = value.tolerance,
                frequenciesBand = value.frequenciesBand,
                frequenciesBracket = value.frequenciesBracket,
                frequenciesRaw = value.frequenciesRaw,
                amplitude = value.amplitude,
                inputScale = value.inputScale,
                outputScale = value.outputScale
            };
        }

    }
}
