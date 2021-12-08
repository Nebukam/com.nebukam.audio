// Copyright (c) 2021 Timothé Lapetite - nebukam@gmail.com.
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
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumFrameProvider : IProcessor
    { 
        NativeList<SpectrumFrameData> outputFrameDataList { get; }
        List<SpectrumFrame> lockedFrames { get; }
    }

    [BurstCompile]
    public class SpectrumFrameProvider : Processor<Unemployed>, ISpectrumFrameProvider
    {

        protected List<SpectrumFrame> m_lockedFrames = new List<SpectrumFrame>(10);
        public List<SpectrumFrame> lockedFrames { get { return m_lockedFrames; } }

        protected NativeList<SpectrumFrameData> m_outputFrameDataList = new NativeList<SpectrumFrameData>(10, Allocator.Persistent);
        public NativeList<SpectrumFrameData> outputFrameDataList { get { return m_outputFrameDataList; } }

        protected List<SpectrumFrame> m_frames;
        public List<SpectrumFrame> frames
        {
            get { return m_frames; }
            set { m_frames = value; }
        }

        protected override void InternalLock()
        {
            m_lockedFrames.Clear();

            if (m_lockedFrames.Count != m_frames.Count)
                m_lockedFrames.Capacity = m_frames.Count;

        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

            //TODO : Avoid repopulating frameDataList each single time
            //but rather only when the list has to be updated.

            int frameCount = m_frames.Count;
            m_outputFrameDataList.Clear();

            SpectrumFrame frame;
            for (int i = 0; i < frameCount; i++)
            {
                frame = m_frames[i];
                m_lockedFrames[i] = frame;
                m_outputFrameDataList.Add(frame);
            }            

        }

        protected override void InternalDispose()
        {
            m_outputFrameDataList.Dispose();
        }

    }
}
