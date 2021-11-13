using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace Nebukam.FrequencyAnalysis
{

    public enum Bands
    {
        EIGHT,
        SIXTY_FOUR
    }

    public enum RangeType
    {
        TRIGGER, // 0f-1f
        PEAK, // 0.0f...1.0f
        AVERAGE, // 0.5f
        SUM // Sums all values in frame.
    }

    public enum Tolerance
    {
        LOOSE, // Any
        STRICT // All frequencies need to be accounted for
    }

    [System.Serializable]
    public struct SamplingDefinition
    {

        public const float maxAmplitude8 = 3.0f;
        public const float maxAmplitude64 = 1.0f;

        [Tooltip("Band reference (8 or 64)")]
        public Bands bands;

        [Tooltip("How to process the data within the frame")]
        public RangeType range;

        [Tooltip("How tolerant is the sampling result. Strict = all frequency must have a value")]
        public Tolerance tolerance;

        [Tooltip("Horizontal start & width of the frame (limited by bands)")]
        public int2 frequency;

        [Tooltip("Vertical start & height of the frame (limited by frequency)")]
        public float2 amplitude;

        [Tooltip("Raw output scale")]
        public float scale;

        public string ID;

        [Tooltip("Debug Color")]
        public Color color;

        static public void Sanitize(ref SamplingDefinition def)
        {

            def.frequency = new int2(
                clamp(def.frequency.x, 0, 63),
                clamp(def.frequency.y, 0, 63));

            def.amplitude = new float2(
                clamp(def.amplitude.x, 0f, 3f),
                clamp(def.amplitude.y, 0f, 3f));

        }

    }

}
