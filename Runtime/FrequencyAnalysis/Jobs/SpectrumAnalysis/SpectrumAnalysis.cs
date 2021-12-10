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

using Nebukam.Collections;
using Nebukam.JobAssist;
using System.Collections.Generic;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumAnalysis : IProcessorGroup
    {
        /// <summary>
        /// Adds a table reference to the spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table wasn't registered yet and has been added, falsle if the table was already registered</returns>
        bool Add(FrequencyTable table);

        /// <summary>
        /// Removes a table reference from the Spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table was registered and has been removed, false if the table wasn't registered</returns>
        bool Remove(FrequencyTable table);

        IFrequencyTableProcessor this[FrequencyTable table] { get; }

        bool TryGetTableProcessor(FrequencyTable table, out IFrequencyTableProcessor processor);

    }

    /// <summary>
    /// A SpectrumAnalysis object takes one or more FrequencyTable as inputs and process spectrum data
    /// according to each of the tables
    /// </summary>
    public class SpectrumAnalysis : ProcessorGroup, ISpectrumAnalysis
    {

        protected bool m_recompute = true;

        #region Frequency Table management

        protected List<FrequencyTable> m_lockedTables = new List<FrequencyTable>();
        protected List<FrequencyTable> m_tables = new List<FrequencyTable>();
        protected Dictionary<FrequencyTable, IFrequencyTableProcessor> m_tableProcessors = new Dictionary<FrequencyTable, IFrequencyTableProcessor>();
        
        protected ListDictionary<FrequencyTable, SpectrumFrame> m_lockedFrameList = new ListDictionary<FrequencyTable, SpectrumFrame>();

        /// <summary>
        /// Adds a table reference to the spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table wasn't registered yet and has been added, false if the table was already registered</returns>
        public bool Add(FrequencyTable table)
        {

            if (m_tables.TryAddOnce(table))
            {
                m_recompute = true;
                return true;
            }

            return false;

        }

        /// <summary>
        /// Removes a table reference from the Spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table was registered and has been removed, false if the table wasn't registered</returns>
        public bool Remove(FrequencyTable table)
        {

            if (m_tables.TryRemove(table))
            {
                m_recompute = true;
                return true;
            }

            return false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IFrequencyTableProcessor this[FrequencyTable table]
        {
            get
            {
                IFrequencyTableProcessor proc;
                if (m_tableProcessors.TryGetValue(table, out proc))
                    return proc;
                else
                    return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="processor"></param>
        /// <returns></returns>
        public bool TryGetTableProcessor(FrequencyTable table, out IFrequencyTableProcessor processor)
        {
            return m_tableProcessors.TryGetValue(table, out processor);
        }

        #endregion

        #region Frame Data Dictionary management

        protected List<FrameDataDictionary> m_lockedFrameDataDictionary = new List<FrameDataDictionary>();
        protected List<FrameDataDictionary> m_dataDictionaries = new List<FrameDataDictionary>();
        //protected Dictionary<FrequencyTable, List<FrameDataDictionary>> m_

        /// <summary>
        /// Adds a FrameDataDictionary reference to the spectrum analysis, as well
        /// as any FrequencyTable this data dictionary frames requires.
        /// </summary>
        /// <param name="frameDictionary"></param>
        /// <returns>true if the table wasn't registered yet and has been added, false if the table was already registered</returns>
        public bool Add(FrameDataDictionary frameDictionary)
        {

            if (m_dataDictionaries.TryAddOnce(frameDictionary)) 
            {
                for (int i = 0, n = frameDictionary.frames.Count; i < n; i++)
                {
                    SpectrumFrame frame = frameDictionary.frames[i];
                    Add(frame.table);
                }

                m_recompute = true;
                return true;
            }

            return false;

        }

        /// <summary>
        /// Removes a FrameDataDictionary from the Spectrum analysis.
        /// Notes that it doesn't remove FrequencyTable dependencies added through Add(frameDictionary).
        /// </summary>
        /// <param name="frameDictionary"></param>
        /// <returns>true if the table was registered and has been removed, false if the table wasn't registered</returns>
        public bool Remove(FrameDataDictionary frameDictionary)
        {

            if (m_dataDictionaries.TryRemove(frameDictionary))
            {
                m_recompute = true;
                return true;
            }

            return false;

        }


        #endregion

        protected void LockFrames()
        {
            m_lockedFrameDataDictionary.Clear();
            m_lockedFrameList.Clear();

            for (int i = 0, n = m_dataDictionaries.Count; i< n; i++)
            {
                FrameDataDictionary dict = m_dataDictionaries[i];
                m_lockedFrameDataDictionary.Add(dict);

                List<SpectrumFrame> frames = dict.frames;
                for(int f = 0, fn = frames.Count; f < fn; f++)
                {
                    SpectrumFrame frame = frames[f];
                    m_lockedFrameList.TryAdd(frame.table, frame);
                }
            }
        }

        protected override void InternalLock()
        {
            
            //if (!m_recompute) { return; }

            LockFrames();

            int tableCount = 0;

            m_lockedTables.Clear();
            m_tableProcessors.Clear();

            // Create or re-use an FTableProcessor for 
            // each registered table

            for (int i = 0, n = m_tables.Count; i < n; i++)
            {

                FrequencyTable table = m_tables[i];
                FTableProcessor tableProcessor = null;

                m_lockedTables.Add(table);

                if (i <= Count - 1)
                    tableProcessor = m_childs[i] as FTableProcessor;
                else
                    Add(ref tableProcessor);

                m_tableProcessors[table] = tableProcessor;
                tableProcessor.table = table;
                tableProcessor.frames = m_lockedFrameList[table];
                tableCount++;

            }

            // Flush uneeded processors

            int childCount = Count;

            if (tableCount < childCount)
            {
                for (int i = tableCount; i < childCount; i++)
                    m_childs.Pop().Dispose();
            }

        }

    }
}
