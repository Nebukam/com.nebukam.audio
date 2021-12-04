using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FFTElement
    {
        public float re;     // Real component
        public float im;     // Imaginary component
        public int next;     // Next element in linked list
        public uint revTgt;  // Target position post bit-reversal
        public int index;    // Index of self in NativeArray
    }
}
