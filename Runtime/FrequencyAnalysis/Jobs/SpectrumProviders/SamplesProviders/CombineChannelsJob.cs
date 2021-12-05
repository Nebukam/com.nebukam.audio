﻿using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;

namespace Nebukam.Audio.FrequencyAnalysis
{
    [BurstCompile]
    public struct CombineChannelsJob : ISamplesExtractionJob
    {

        [ReadOnly]
        private int m_inputNumChannels;
        public int inputNumChannels { set { m_inputNumChannels = value; } }

        [ReadOnly]
        private NativeArray<float> m_inputMultiChannelSamples;
        public NativeArray<float> inputMultiChannelSamples { set { m_inputMultiChannelSamples = value; } }

        private NativeArray<float> m_outputSamples;
        public NativeArray<float> outputSamples { set { m_outputSamples = value; } }

        [ReadOnly]
        public NativeArray<int> m_inputChannels;

        public void Execute(int index)
        {

            int combinedChannels = m_inputChannels.Length;
            int start = index * m_inputNumChannels;

            float sampleValue = 0f;

            for (int i = 0; i < combinedChannels; i++)
            {
                sampleValue += m_inputMultiChannelSamples[start + m_inputChannels[i]];
            }

            m_outputSamples[index] = sampleValue / combinedChannels;

        }

    }
}
