using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    [BurstCompile]
    public struct FrequencyBracketsExtractionJob : Unity.Jobs.IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> m_inputSpectrum;
        [ReadOnly]
        internal NativeArray<FrequencyRange> m_inputRanges;

        public NativeArray<BracketData> m_outputBrackets;

        public void Execute(int index)
        {

            int bins = m_inputSpectrum.Length;

            FrequencyRange
                range = m_inputRanges[index];

            float
                value,
                sum = 0f,
                max = 0f,
                min = float.MaxValue;

            int
                start = range.Start(bins),
                width = range.Length(bins),
                end = start + width;

            for (int i = start; i < end; i++)
            {
                value = m_inputSpectrum[i];
                sum += value;
                max = math.max(max, value);
                min = math.min(min, value);
            }

            m_outputBrackets[index] = new BracketData()
            {
                min = min,
                max = max,
                average = sum /= width,
                width = width
            };

        }

    }
}
