using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrequencyTableProvider : IProcessor
    {
        NativeArray<FrequencyRange> outputRanges { get; }
        FrequencyTable table { get; set; }
    }

    [BurstCompile]
    public class FrequencyTableDataProvider : Processor<Unemployed>, IFrequencyTableProvider
    {

        protected bool m_recompute = true;

        protected List<FrequencyRange> m_lockedRanges = new List<FrequencyRange>(50);
        public List<FrequencyRange> lockedRanges { get { return m_lockedRanges; } }

        protected NativeArray<FrequencyRange> m_outputRanges = new NativeArray<FrequencyRange>(0, Allocator.Persistent);
        public NativeArray<FrequencyRange> outputRanges { get { return m_outputRanges; } }

        protected FrequencyTable m_frequencyTable;
        public FrequencyTable table {
            get { return m_frequencyTable; }
            set
            {
                if(m_frequencyTable == value) { return; }
                m_frequencyTable = value;
                m_recompute = true;
            }
        }

        protected override void InternalLock()
        {

            if (!m_recompute) { return; }

            m_lockedRanges.Clear();

            FrequencyRange[] ranges = m_frequencyTable.ranges;
            int rangeCount = ranges.Length;

            for(int i = 0; i < rangeCount; i++)
                m_lockedRanges.Add(ranges[i]);

        }

        protected override void Prepare(ref Unemployed job, float delta)
        {

            if (m_recompute) 
            {
                Copy(m_lockedRanges, ref m_outputRanges);
                m_recompute = false;
            }

        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {
            m_outputRanges.Dispose();
        }

    }
}
