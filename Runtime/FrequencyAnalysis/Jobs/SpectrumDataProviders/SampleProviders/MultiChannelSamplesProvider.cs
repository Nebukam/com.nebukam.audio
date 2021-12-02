using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IMultiChannelSamplesProvider : IChannelSamplesProvider
    {

    }

    /// <summary>
    /// Job data provider responsible for extracting  raw audio data
    /// </summary>
    [BurstCompile]
    public class MultiChannelSamplesProvider : AbstractSamplesProvider<MultiChannelExtractionJob>, IMultiChannelSamplesProvider
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

        protected override int Prepare(ref MultiChannelExtractionJob job, float delta)
        {

            int result = base.Prepare(ref job, delta);

            int channelCount = m_channels.Length;

            MakeLength(ref m_nativeChannels, channelCount);

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
