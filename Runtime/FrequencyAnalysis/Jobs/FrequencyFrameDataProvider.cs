using Nebukam.JobAssist;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrequencyFrameDataProvider : IProcessor
    { 
        NativeList<FrequencyFrameData> outputFrameDataList { get; }
        List<FrequencyFrame> lockedFrames { get; }
    }

    [BurstCompile]
    public class FrequencyFrameDataProvider : Processor<Unemployed>, IFrequencyFrameDataProvider
    {

        protected List<FrequencyFrame> m_lockedFrames = new List<FrequencyFrame>(10);
        public List<FrequencyFrame> lockedFrames { get { return m_lockedFrames; } }

        protected NativeList<FrequencyFrameData> m_outputFrameDataList = new NativeList<FrequencyFrameData>(10, Allocator.Persistent);
        public NativeList<FrequencyFrameData> outputFrameDataList { get { return m_outputFrameDataList; } }

        protected List<FrequencyFrame> m_frames;
        public List<FrequencyFrame> frames
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

            FrequencyFrame frame;
            for (int i = 0; i < frameCount; i++)
            {
                frame = m_frames[i];
                m_lockedFrames[i] = frame;
                m_outputFrameDataList.Add(frame);
            }            

        }

        protected override void InternalUnlock(){ }

        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {
            m_outputFrameDataList.Dispose();
        }

    }
}
