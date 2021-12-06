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
    public struct SingleChannelJob : ISamplesExtractionJob
    {

        [ReadOnly]
        private int m_inputNumChannels;
        public int inputNumChannels { set { m_inputNumChannels = value; } }

        [ReadOnly]
        private NativeArray<float> m_inputMultiChannelSamples;
        public NativeArray<float> inputMultiChannelSamples { set { m_inputMultiChannelSamples = value; } }

        private NativeArray<float> m_outputSamples;
        public NativeArray<float> outputSamples { set { m_outputSamples = value; } }

        public int channel;

        public void Execute(int index)
        {
            
            int start = index * m_inputNumChannels;
            m_outputSamples[index] = m_inputMultiChannelSamples[start + channel];
        }

    }
}
