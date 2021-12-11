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
using Nebukam.Signals;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface ISpectrumAnalysis : IProcessorGroup
    {
        /// <summary>
        /// Adds a table reference to the spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table wasn't registered yet and has been added, falsle if the table was already registered</returns>
        IFrequencyTableProcessor Add(FrequencyTable table);

        /// <summary>
        /// Removes a table reference from the Spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>true if the table was registered and has been removed, false if the table wasn't registered</returns>
        IFrequencyTableProcessor Remove(FrequencyTable table);

        IFrequencyTableProcessor this[FrequencyTable table] { get; }

        bool TryGetTableProcessor(FrequencyTable table, out IFrequencyTableProcessor processor);

    }

    /// <summary>
    /// A SpectrumAnalysis object takes one or more FrequencyTable as inputs and process spectrum data
    /// according to each of the tables
    /// </summary>
    public class SpectrumAnalysis : ProcessorGroup, ISpectrumAnalysis
    {

        #region Frequency Table management

        protected ListRecord<FrequencyTable> m_tablesRecord = new ListRecord<FrequencyTable>();
        protected Dictionary<FrequencyTable, IFrequencyTableProcessor> m_tableProcessors = new Dictionary<FrequencyTable, IFrequencyTableProcessor>();

        /// <summary>
        /// Adds a table reference to the spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>Associated IFrequencyTableProcessor</returns>
        public IFrequencyTableProcessor Add(FrequencyTable table)
        {

            if (m_locked)
            {
                throw new System.Exception("Attempting to add a new FrequencyTable reference while the analysis is locked.");
            }

            if (m_tablesRecord.Add(table) == 1)
            {
                
                IFrequencyTableProcessor proc = new FTableProcessor();
                proc.table = table;
                Add(proc);

                m_tableProcessors[table] = proc;

                return proc;
            }

            return m_tableProcessors[table];

        }

        /// <summary>
        /// Removes a table reference from the Spectrum analysis
        /// </summary>
        /// <param name="table"></param>
        /// <returns>Associated IFrequencyTableProcessor</returns>
        public IFrequencyTableProcessor Remove(FrequencyTable table)
        {

            if (m_locked)
            {
                throw new System.Exception("Attempting to remove a FrequencyTable reference while the analysis is locked.");
            }

            IFrequencyTableProcessor proc = m_tableProcessors[table];

            if (m_tablesRecord.Remove(table) == 0)
            {
                m_tableProcessors.Remove(table);
                proc.Dispose();
                return proc;
            }

            return proc;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public IFrequencyTableProcessor this[FrequencyTable table]{ get{ return m_tableProcessors[table]; } }

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

        protected List<IFrameDataDictionary> m_dataDictionaries = new List<IFrameDataDictionary>();

        /// <summary>
        /// Adds a FrameDataDictionary reference to the spectrum analysis, as well
        /// as any FrequencyTable this data dictionary frames requires.
        /// </summary>
        /// <param name="frameDictionary"></param>
        /// <returns>true if the table wasn't registered yet and has been added, false if the table was already registered</returns>
        public bool Add(IFrameDataDictionary frameDictionary)
        {

            if (m_dataDictionaries.TryAddOnce(frameDictionary)) 
            {
                for (int i = 0, n = frameDictionary.tablesRecord.Count; i < n; i++)
                {
                    FrequencyTable table = frameDictionary.tablesRecord[i];
                    Add(table).framesReader.Add(frameDictionary);
                }

                frameDictionary.onTableRecordAdded.Add(m_onTableRecordAddedDelegate);
                frameDictionary.onTableRecordRemoved.Add(m_onTableRecordRemovedDelegate);

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
        public bool Remove(IFrameDataDictionary frameDictionary)
        {

            if (m_dataDictionaries.TryRemove(frameDictionary))
            {

                for (int i = 0, n = frameDictionary.tablesRecord.Count; i < n; i++)
                {
                    FrequencyTable table = frameDictionary.tablesRecord[i];
                    Remove(table).framesReader.Remove(frameDictionary);
                }

                frameDictionary.onTableRecordAdded.Remove(m_onTableRecordAddedDelegate);
                frameDictionary.onTableRecordRemoved.Remove(m_onTableRecordRemovedDelegate);

                return true;
            }

            return false;

        }

        private SignalDelegates.Signal<IFrameDataDictionary, FrequencyTable> m_onTableRecordAddedDelegate;
        private void OnTableRecordAdded(IFrameDataDictionary dictionary, FrequencyTable table)
        {
            Add(table).framesReader.Add(dictionary);
        }

        private SignalDelegates.Signal<IFrameDataDictionary, FrequencyTable> m_onTableRecordRemovedDelegate;
        private void OnTableRecordRemoved(IFrameDataDictionary dictionary, FrequencyTable table)
        {
            Remove(table).framesReader.Remove(dictionary);
        }

        #endregion

        public SpectrumAnalysis()
        {
            m_onTableRecordAddedDelegate = OnTableRecordAdded;
            m_onTableRecordRemovedDelegate = OnTableRecordRemoved;
        }
    }
}
