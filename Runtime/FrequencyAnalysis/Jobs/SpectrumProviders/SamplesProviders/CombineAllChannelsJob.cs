using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

namespace Nebukam.Audio.FrequencyAnalysis
{
    [BurstCompile]
    public struct CombineAllChannelsJob : ISamplesExtractionJob
    {

        [ReadOnly]
        private int m_inputNumChannels;
        public int inputNumChannels { set { m_inputNumChannels = value; } }

        [ReadOnly]
        private NativeArray<float> m_inputRawSamples;
        public NativeArray<float> inputMultiChannelSamples { set { m_inputRawSamples = value; } }

        private NativeArray<float> m_outputSamples;
        public NativeArray<float> outputSamples { set { m_outputSamples = value; } }

        public void Execute(int index)
        {

            int start = index * m_inputNumChannels;
            int end = start + m_inputNumChannels;

            float sampleValue = 0f;

            for (int i = start; i < end; i++)
                sampleValue += m_inputRawSamples[i];

            m_outputSamples[index] = sampleValue / m_inputNumChannels;

        }

    }
}
