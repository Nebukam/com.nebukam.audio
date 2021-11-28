using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;
using UnityEditor;
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum RangeType
    {
        Trigger, // 0f-1f
        Peak, // 0.0f...1.0f
        Average, // 0.5f
        Sum // Sums all values in frame.
    }

    public enum Bands
    {
        Eight = 0, // 8
        SixtyFour = 1, // 64
        HundredTwentyEight = 2 // 128
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

        public RangeType range;

        public bool ON { get { return trigger > 0f; } }

        public static implicit operator float(Sample s)
        {
            if (s.range == RangeType.Trigger) { return s.trigger; }
            if (s.range == RangeType.Peak) { return s.peak; }
            if (s.range == RangeType.Average) { return s.average; }
            if (s.range == RangeType.Sum) { return s.sum; }

            return s.average;
        }

    }

    public struct FrequencyFrameData
    {

        public string ID;
        public Bands bands;
        public RangeType range;
        public Tolerance tolerance;
        public int2 frequency;
        public float2 amplitude;
        public float scale;

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
        public Bands bands = Bands.SixtyFour;

        [Tooltip("How to process the data within the frame")]
        public RangeType range = RangeType.Average;

        [Tooltip("How tolerant is the sampling result. Strict = all frequency must be > 0")]
        public Tolerance tolerance = Tolerance.Loose;

        [Tooltip("Horizontal start & width of the frame (limited by bands)")]
        public int2 frequency = new int2(0,1);

        [Tooltip("Vertical start & height of the frame (limited by frequency)")]
        public float2 amplitude = new float2(0f,1f);

        [Tooltip("Raw output scale")]
        public float scale = 1f;

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
        public void Copy(FrequencyFrameData data, bool copyID = false)
        {
            if (copyID) { ID = data.ID; }
            bands = data.bands;
            range = data.range;
            tolerance = data.tolerance;
            frequency = data.frequency;
            amplitude = data.amplitude;
            scale = data.scale;
        }
        
        public static implicit operator FrequencyFrameData(FrequencyFrame value) {
            return new FrequencyFrameData() {
                ID = value.ID,
                bands = value.bands,
                range = value.range,
                tolerance = value.tolerance,
                frequency = value.frequency,
                amplitude = value.amplitude,
                scale = value.scale
            }; 
        }

    }
}
