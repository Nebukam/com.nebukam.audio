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
    public class SpectrumAnalysis : ProcessorGroup
    {

        protected bool m_recompute = true;

        protected List<FrequencyTable> m_lockedTables = new List<FrequencyTable>();
        protected Dictionary<FrequencyTable, SpectrumPostProcessor> m_tableProcessingChains = new Dictionary<FrequencyTable, SpectrumPostProcessor>();

        protected List<FrameDataDictionary> m_dataDictionaries = new List<FrameDataDictionary>();
        

        public void Add(FrameDataDictionary frameDataDict)
        {
            if (m_dataDictionaries.Contains(frameDataDict)) { return; }

            m_dataDictionaries.Add(frameDataDict);
            m_recompute = true;
        }

        public void Remove(FrameDataDictionary frameDataDict)
        {
            int index = m_dataDictionaries.IndexOf(frameDataDict);

            if (index == -1) { return; }
            m_dataDictionaries.RemoveAt(index);
            m_recompute = true;
        }

        #region Inputs



        #endregion

        protected int tableLockIndex = 0;

        protected void LockFrame(SpectrumFrame frame)
        {

            SpectrumPostProcessor tableProcessor;
            FrequencyTable table = frame.table;

            if (!m_tableProcessingChains.TryGetValue(table, out tableProcessor))
            {

                if (tableLockIndex > m_childs.Count - 1) // Create new chain
                    Add(ref tableProcessor);
                else // Re-use existing chain
                    tableProcessor = m_childs[tableLockIndex] as SpectrumPostProcessor;

                tableProcessor.table = table;

                m_tableProcessingChains[table] = tableProcessor;
                m_lockedTables.Add(table);                

                tableLockIndex++;

            }

        }

        protected override void InternalLock()
        {

            if (m_recompute)
            {

                tableLockIndex = 0;

                m_lockedTables.Clear();
                m_tableProcessingChains.Clear();

                for (int i = 0, ni = m_dataDictionaries.Count; i < ni; i++)
                {

                    List<SpectrumFrame> frames = m_dataDictionaries[i].frames;

                    for(int f = 0, nf = frames.Count; f < nf; f++)
                        LockFrame(frames[f]);

                }

                // Flush uneeded processors

                int n = m_childs.Count;
                if (tableLockIndex > n - 1)
                {
                    for (int i = tableLockIndex; i < n; i++)
                        Remove(m_childs[tableLockIndex]).Dispose();
                }

            }

        }

    }
}
