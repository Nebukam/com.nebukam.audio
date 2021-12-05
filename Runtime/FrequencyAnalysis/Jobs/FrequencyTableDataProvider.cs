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
        NativeArray<FrequencyRange> outputFrequencyRanges { get; }
        FrequencyTable frequencyTable { get; set; }
    }

    [BurstCompile]
    public class FrequencyTableDataProvider : Processor<Unemployed>, IFrequencyTableProvider
    {

        protected bool m_recompute = true;

        protected List<FrequencyRange> m_lockedRanges = new List<FrequencyRange>(10);
        public List<FrequencyRange> lockedRanges { get { return m_lockedRanges; } }

        protected NativeArray<FrequencyRange> m_outputFrequencyRanges = new NativeArray<FrequencyRange>(0, Allocator.Persistent);
        public NativeArray<FrequencyRange> outputFrequencyRanges { get { return m_outputFrequencyRanges; } }

        protected FrequencyTable m_frequencyTable;
        public FrequencyTable frequencyTable {
            get { return m_frequencyTable; }
            set
            {
                if(m_frequencyTable == value) { return; }
                m_frequencyTable = value;
                m_recompute = true;
            }
        }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected IFrequencyFrameDataProvider m_inputFrequencyFrameDataProvider;

        #endregion

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

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputFrequencyFrameDataProvider))
                {
                    throw new System.Exception("IFrequencyFrameDataProvider missing");
                }

                m_inputsDirty = false;

            }

            //TODO : Gather Tables references from Frames
            // Need two arrays :
            // First is table info (start index, size)
            // Second is inlined aggregation of all tables found

            if (m_recompute) 
            {
                Copy(ref m_lockedRanges, ref m_outputFrequencyRanges);
                m_recompute = false;
            }
        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref Unemployed job) { }

        protected override void InternalDispose()
        {
            m_outputFrequencyRanges.Dispose();
        }

    }
}
