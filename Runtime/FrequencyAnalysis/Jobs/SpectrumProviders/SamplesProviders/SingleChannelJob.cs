﻿// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
