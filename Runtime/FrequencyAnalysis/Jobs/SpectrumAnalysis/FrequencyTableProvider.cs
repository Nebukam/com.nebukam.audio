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
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFTableProvider : IProcessor
    {
        NativeArray<FrequencyRange> outputRanges { get; }
        FrequencyTable table { get; set; }
    }

    [BurstCompile]
    public class FrequencyTableProvider : Processor<Unemployed>, IFTableProvider
    {

        protected bool m_recompute = true;

        protected List<FrequencyRange> m_lockedRanges = new List<FrequencyRange>(50);
        public List<FrequencyRange> lockedRanges { get { return m_lockedRanges; } }

        protected NativeArray<FrequencyRange> m_outputRanges = new NativeArray<FrequencyRange>(0, Allocator.Persistent);
        public NativeArray<FrequencyRange> outputRanges { get { return m_outputRanges; } }

        protected FrequencyTable m_table;
        public FrequencyTable table {
            get { return m_table; }
            set
            {
                if(m_table == value) { return; }
                m_table = value;
                m_recompute = true;
            }
        }

        protected override void InternalLock()
        {

            if (!m_recompute) { return; }

            m_lockedRanges.Clear();

            FrequencyRange[] ranges = m_table.ranges;
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

        protected override void InternalDispose()
        {
            m_outputRanges.Dispose();
        }

    }
}
