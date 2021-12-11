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

using System.Collections.Generic;
using Nebukam;
using Nebukam.Collections;
using Nebukam.Signals;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFrameDataDictionary 
    {
        ISignal<IFrameDataDictionary, SpectrumFrame> onFrameAdded { get; }
        ISignal<IFrameDataDictionary, SpectrumFrame> onFrameRemoved { get; }
        ISignal<IFrameDataDictionary, SpectrumFrame, Sample> onDataUpdated { get; }

#if UNITY_EDITOR
        ISignal<IFrameDataDictionary, SpectrumFrame> onFrameUpdated { get; }
#endif

        ISignal<IFrameDataDictionary, FrequencyTable> onTableRecordAdded { get; }
        ISignal<IFrameDataDictionary, FrequencyTable> onTableRecordRemoved { get; }

        List<FrequencyTable> tablesRecord { get; }

        bool updateSignalEnabled { get; set; }
        int Count { get; }
        SpectrumFrame this[int i] { get; }
        Sample this[SpectrumFrame frame] { get; }

        /// <summary>
        /// Registers a list of FrequencyFrame in this dictionary
        /// to be updated by a FrequencyBandAnalyser
        /// </summary>
        /// <param name="list"></param>
        void Add(SpectrumFrameList list);

        /// <summary>
        /// Registers a single FrequencyFrame in this dictionary
        /// </summary>
        /// <param name="frame"></param>
        void Add(SpectrumFrame frame);

        /// <summary>
        /// Removes a single FrequencyFrame from this dictionary
        /// </summary>
        /// <param name="frame"></param>
        void Remove(SpectrumFrame frame);

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGet(SpectrumFrame frame, out float result);

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryGet(SpectrumFrame frame, out Sample result);

        /// <summary>
        /// Sets the value of a Sample
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="value"></param>
        void Set(SpectrumFrame frame, Sample value);

        /// <summary>
        /// Clears the lists & data
        /// </summary>
        void Clear();
    }

    public class FrameDataDictionary : IFrameDataDictionary
    {

        protected Signal<IFrameDataDictionary, SpectrumFrame> m_onFrameAdded = new Signal<IFrameDataDictionary, SpectrumFrame>();
        public ISignal<IFrameDataDictionary, SpectrumFrame> onFrameAdded { get { return m_onFrameAdded; } }

        protected Signal<IFrameDataDictionary, SpectrumFrame> m_onFrameRemoved = new Signal<IFrameDataDictionary, SpectrumFrame>();
        public ISignal<IFrameDataDictionary, SpectrumFrame> onFrameRemoved { get { return m_onFrameRemoved; } }

        protected Signal<IFrameDataDictionary, SpectrumFrame, Sample> m_onDataUpdated = new Signal<IFrameDataDictionary, SpectrumFrame, Sample>();
        public ISignal<IFrameDataDictionary, SpectrumFrame, Sample> onDataUpdated { get { return m_onDataUpdated; } }

#if UNITY_EDITOR

        protected Signal<IFrameDataDictionary, SpectrumFrame> m_onFrameUpdated = new Signal<IFrameDataDictionary, SpectrumFrame>();
        public ISignal<IFrameDataDictionary, SpectrumFrame> onFrameUpdated { get { return m_onFrameUpdated; } }

#endif

        protected Signal<IFrameDataDictionary, FrequencyTable> m_onTableRecordAdded = new Signal<IFrameDataDictionary, FrequencyTable>();
        public ISignal<IFrameDataDictionary, FrequencyTable> onTableRecordAdded { get { return m_onTableRecordAdded; } }

        protected Signal<IFrameDataDictionary, FrequencyTable> m_onTableRecordRemoved = new Signal<IFrameDataDictionary, FrequencyTable>();
        public ISignal<IFrameDataDictionary, FrequencyTable> onTableRecordRemoved { get { return m_onTableRecordRemoved; } }

        protected ListRecord<FrequencyTable> m_tablesRecord = new ListRecord<FrequencyTable>();
        public List<FrequencyTable> tablesRecord { get { return m_tablesRecord.list; } }

        protected bool m_dataUpdateSignalEnabled = false;
        public bool updateSignalEnabled
        {
            get { return m_dataUpdateSignalEnabled; }
            set { m_dataUpdateSignalEnabled = value; }
        }

        protected List<SpectrumFrame> m_frames = new List<SpectrumFrame>();
        protected Dictionary<SpectrumFrame, Sample> m_dataDic = new Dictionary<SpectrumFrame, Sample>();

        public List<SpectrumFrame> frames { get { return m_frames; } }

        public FrameDataDictionary()
        {

        }

        /// <summary>
        /// Registers a list of FrequencyFrame in this dictionary
        /// to be updated by a FrequencyBandAnalyser
        /// </summary>
        /// <param name="list"></param>
        public void Add(SpectrumFrameList list)
        {
            for (int i = 0, n = list.Frames.Count; i < n; i++)
                Add(list.Frames[i]);
        }

        /// <summary>
        /// Registers a single FrequencyFrame in this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Add(SpectrumFrame frame)
        {
            if (m_frames.TryAddOnce(frame))
            {
                m_dataDic[frame] = new Sample();
                m_onFrameAdded.Dispatch(this, frame);

                if(m_tablesRecord.Add(frame.table) == 1)
                    m_onTableRecordAdded.Dispatch(this, frame.table);
            }
        }

        /// <summary>
        /// Removes a single FrequencyFrame from this dictionary
        /// </summary>
        /// <param name="frame"></param>
        public void Remove(SpectrumFrame frame)
        {
            if (m_frames.TryRemove(frame))
            {
                m_dataDic.Remove(frame);
                m_onFrameRemoved.Dispatch(this, frame);

                if (m_tablesRecord.Remove(frame.table) == 0)
                    m_onTableRecordRemoved.Dispatch(this, frame.table);
            }
        }

        public int Count { get { return m_frames.Count; } }

        public SpectrumFrame this[int i] { get { return m_frames[i]; } }

        public Sample this[SpectrumFrame frame] { 
            get 
            {
                Sample result;
                if (m_dataDic.TryGetValue(frame, out result))
                    return result;
                else
                    return new Sample();
            }
        }

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(SpectrumFrame frame, out float result)
        {
            Sample s;

            if (m_dataDic.TryGetValue(frame, out s))
            {
                result = s;
                return true;
            }

            result = 0f;
            return false;
        }

        /// <summary>
        /// Tries to find a Sample associated with a given FrequencyFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGet(SpectrumFrame frame, out Sample result)
        {
            return m_dataDic.TryGetValue(frame, out result);
        }

        /// <summary>
        /// Sets the value of a Sample
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="value"></param>
        public void Set(SpectrumFrame frame, Sample value)
        {
            Sample previousSample;

            if (!m_dataDic.TryGetValue(frame, out previousSample) || !previousSample.ON)
                value.justTriggered = true;

            m_dataDic[frame] = value;

            if (m_dataUpdateSignalEnabled)
                m_onDataUpdated.Dispatch(this, frame, value);
        }

        /// <summary>
        /// Clears the lists & data
        /// </summary>
        public void Clear()
        {
            for(int i = 0, n = m_frames.Count; i < n; i++)
                m_onFrameRemoved.Dispatch(this, m_frames.Pop());

            for (int i = 0, n = m_tablesRecord.Count; i < n; i++)
                m_onTableRecordRemoved.Dispatch(this, m_tablesRecord.list.Pop());

            m_tablesRecord.Clear();
            m_frames.Clear();
            m_dataDic.Clear();
        }

        public override string ToString()
        {
            string str = "---\n";

            for (int i = 0; i < m_frames.Count; i++)
            {
                SpectrumFrame frame = m_frames[i];
                str += string.Format("[{0}] {1} = {2}\n", frame.name, frame.ID, m_dataDic[frame]);
            }

            return str;

        }

    }

}
