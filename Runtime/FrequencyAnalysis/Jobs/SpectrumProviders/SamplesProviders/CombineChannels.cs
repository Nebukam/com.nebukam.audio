using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class CombineChannels : AbstractSamplesProvider<CombineChannelsJob>, ISamplesProvider
    {

        protected NativeArray<int> m_nativeChannels = new NativeArray<int>(0, Allocator.Persistent);

        protected int[] m_channels = new int[0];
        public int[] channels
        {
            get { return m_channels; }
            set { m_channels = value; }
        }

        public void SetChannels(params int[] channelsList)
        {
            m_channels = channelsList;
        }

        protected override int Prepare(ref CombineChannelsJob job, float delta)
        {

            int result = base.Prepare(ref job, delta);

            Copy(ref m_channels, ref m_nativeChannels);
            job.channels = m_nativeChannels;

            return result;

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            m_nativeChannels.Dispose();
        }

    }
}
