using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEditor;
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum OutputType
    {
        Trigger, // 0f-1f
        Peak, // 0.0f...1.0f
        Average, // 0.5f
        Sum // Sums all values in frame.
    }

    public enum Bands
    {
        _8 = 8,
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128
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
        public OutputType output;
        public Tolerance tolerance;
        public int2 frequency;
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

        [Tooltip("ID under which the data will be stored. Note that duplicate IDs will overwrite each other.")]
        public string ID = "Unidentified Sample";

        [Tooltip("Band reference (8 or 64)")]
        public Bands bands = Bands._64;

        [Tooltip("How to process the data within the frame")]
        public OutputType output = OutputType.Average;

        [Tooltip("How tolerant is the sampling result. Strict = all frequency must be > 0")]
        public Tolerance tolerance = Tolerance.Loose;

        [Tooltip("Horizontal start & width of the frame (limited by bands)")]
        public int2 frequency = new int2(0,1);

        [Tooltip("Vertical start & height of the frame (limited by frequency)")]
        public float2 amplitude = new float2(0f,1f);

        [Tooltip("Data input scale")]
        public float inputScale = 1f;

        [Tooltip("Data output scale")]
        public float outputScale = 1f;

        [Tooltip("Debug Color")]
        public Color color = Color.red;

#if UNITY_EDITOR

        /// <summary>
        /// Ensure frequency is within supported limits
        /// </summary>
        public void Validate()
        {
            frequency = new int2(
                clamp(frequency.x, 0, 63),
                clamp(frequency.y, 0, 63));

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
            tolerance = data.tolerance;
            frequency = data.frequency;
            amplitude = data.amplitude;
            inputScale = data.inputScale;
            outputScale = data.outputScale;
        }
        
        public static implicit operator FrequencyFrameData(FrequencyFrame value) {
            return new FrequencyFrameData() {
                bands = value.bands,
                output = value.output,
                tolerance = value.tolerance,
                frequency = value.frequency,
                amplitude = value.amplitude,
                inputScale = value.inputScale,
                outputScale = value.outputScale
            }; 
        }

    }
}
