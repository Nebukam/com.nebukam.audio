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
    public struct MultiChannelExtractionJob : IChannelSamplesJob
    {

        [ReadOnly]
        private SpectrumInfos m_spectrumInfos;
        public SpectrumInfos spectrumInfos { set { m_spectrumInfos = value; } }

        [ReadOnly]
        private NativeArray<float> m_rawPoints;
        public NativeArray<float> inputRawSamples { set { m_rawPoints = value; } }

        private NativeArray<float> m_points;
        public NativeArray<float> outputSamples { set { m_points = value; } }

        public NativeArray<int> channels;

        public void Execute(int index)
        {

            int numChannels = m_spectrumInfos.numChannels;
            int combinedChannels = channels.Length;
            int start = index * numChannels;

            float sampleValue = 0f;

            for (int i = 0; i < combinedChannels; i++)
            {
                sampleValue += m_rawPoints[start + channels[i]];
            }

            m_points[index] = sampleValue / combinedChannels;

        }

    }
}
