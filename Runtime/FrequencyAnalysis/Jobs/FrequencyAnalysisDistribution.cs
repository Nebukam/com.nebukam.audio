using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{
    public class FrequencyAnalysisDistribution : ProcessorGroup
    {

        protected bool m_recompute = true;

        protected List<FrequencyTable> m_lockedTables = new List<FrequencyTable>();
        protected Dictionary<FrequencyTable, FrequencyTableChain> m_tableProcessingChains = new Dictionary<FrequencyTable, FrequencyTableChain>();

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

        protected void LockFrame(FrequencyFrame frame)
        {

            FrequencyTableChain chain;
            if (!m_tableProcessingChains.TryGetValue(frame.table, out chain))
            {
                
            }

        }

        protected override void InternalLock()
        {

            if (m_recompute)
            {

                m_lockedTables.Clear();
                m_tableProcessingChains.Clear();

                int chainIndex = 0;

                for (int i = 0, ni = m_dataDictionaries.Count; i < ni; i++)
                {

                    List<FrequencyFrame> frames = m_dataDictionaries[i].frames;

                    for(int f = 0, nf = frames.Count; f < nf; f++)
                    {
                        
                        FrequencyFrame frame = frames[f];
                        FrequencyTableChain chain;

                        if (!m_tableProcessingChains.TryGetValue(frame.table, out chain))
                        {
                            
                            if(chainIndex > m_childs.Count - 1) // Create new chain
                                Add(ref chain);
                            else // Re-use existing chain
                                chain = m_childs[chainIndex] as FrequencyTableChain;

                            chain.table = frame.table;

                            chainIndex++;

                        }

                    }

                }

                // Flush extra chains

                int n = m_childs.Count;
                if (chainIndex > n - 1)
                {
                    for (int i = chainIndex; i < n; i++)
                        Remove(m_childs[chainIndex]).Dispose();
                }

            }

        }

        protected override void Prepare(float delta) { }

        protected override void Apply() { }

        protected override void InternalUnlock() { }

    }
}
