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
        protected Dictionary<FrequencyTable, FrequencyTableProcessor> m_tableProcessingChains = new Dictionary<FrequencyTable, FrequencyTableProcessor>();

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

        protected void LockFrame(FrequencyFrame frame)
        {

            FrequencyTableProcessor tableProcessor;
            FrequencyTable table = frame.table;

            if (!m_tableProcessingChains.TryGetValue(table, out tableProcessor))
            {

                if (tableLockIndex > m_childs.Count - 1) // Create new chain
                    Add(ref tableProcessor);
                else // Re-use existing chain
                    tableProcessor = m_childs[tableLockIndex] as FrequencyTableProcessor;

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

                    List<FrequencyFrame> frames = m_dataDictionaries[i].frames;

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

        protected override void Prepare(float delta) 
        {

        }

        protected override void Apply() 
        {
        
            // Go through each dictionaries frames


        }

        protected override void InternalUnlock() { }

    }
}
