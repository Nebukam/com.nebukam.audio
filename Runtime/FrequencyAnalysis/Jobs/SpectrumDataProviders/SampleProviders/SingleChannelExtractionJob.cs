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
    public struct SingleChannelExtractionJob : IChannelSamplesJob
    {

        [ReadOnly]
        private SpectrumInfos m_spectrumInfos;
        public SpectrumInfos spectrumInfos { set { m_spectrumInfos = value; } }

        [ReadOnly]
        private NativeArray<float> m_rawPoints;
        public NativeArray<float> inputRawSamples { set { m_rawPoints = value; } }

        private NativeArray<float> m_points;
        public NativeArray<float> outputSamples { set { m_points = value; } }

        public int channel;

        public void Execute(int index)
        {
            int start = index * m_spectrumInfos.numChannels;            
            m_points[index] = m_rawPoints[start + channel];
        }

    }
}
