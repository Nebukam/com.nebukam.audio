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
        private SpectrumInfos m_spectrumInfos;
        public SpectrumInfos spectrumInfos { set { m_spectrumInfos = value; } }

        [ReadOnly]
        private NativeArray<float> m_inputRawSamples;
        public NativeArray<float> inputRawSamples { set { m_inputRawSamples = value; } }

        private NativeArray<float> m_outputSamples;
        public NativeArray<float> outputSamples { set { m_outputSamples = value; } }

        public void Execute(int index)
        {

            int numChannels = m_spectrumInfos.numChannels;
            int start = index * numChannels;
            int end = start + numChannels;

            float sampleValue = 0f;

            for (int i = start; i < end; i++)
                sampleValue += m_inputRawSamples[i];

            m_outputSamples[index] = sampleValue / numChannels;

        }

    }
}
