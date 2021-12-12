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
using Nebukam.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Nebukam;
using Nebukam.Signals;
using static Nebukam.JobAssist.Extensions;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IFramesReader : IProcessor
    {
        FrequencyTable table { get; set; }
        void Add(IFrameDataDictionary frameDataDictionary);
        void Remove(IFrameDataDictionary frameDataDictionary);
        bool cacheFrameData { get; set; }
    }

    public class SpectrumFramesReader : ProcessorGroup, IFramesReader
    {

        protected bool m_recompute = true;
        protected bool m_refreshLockedFrames = true;

        protected FrequencyTable m_table = null;
        public FrequencyTable table
        {
            get { return m_table; }
            set { m_table = value; }
        }

        protected HashSet<IFrameDataDictionary> m_dictionariesHash = new HashSet<IFrameDataDictionary>();
        protected ListDictionary<SpectrumFrame, IFrameDataDictionary> m_frameMap = new ListDictionary<SpectrumFrame, IFrameDataDictionary>();
        public ListRecord<SpectrumFrame> m_frames = new ListRecord<SpectrumFrame>(10);

        public List<SpectrumFrame> m_lockedFrames = new List<SpectrumFrame>(10);
        
        protected NativeArray<SpectrumFrameData> m_inputFrameData = default;

        protected NativeArray<Sample> m_outputFrameSamples = default;
        public NativeArray<Sample> outputFrameSamples { get { return m_outputFrameSamples; } }

        public bool cacheFrameData { get; set; } = true;

        protected ReadBands m_readBands;
        protected ReadBrackets m_readBrackets;
        protected ReadSpectrum m_readSpectrum;

        public SpectrumFramesReader()
        {
            Add(ref m_readBands);
            Add(ref m_readBrackets);
            Add(ref m_readSpectrum);

            m_onFrameAddedDelegate = OnFrameAdded;
            m_onFrameRemovedDelegate = OnFrameRemoved;
        }

        private void Add(SpectrumFrame frame, IFrameDataDictionary container)
        {
            if (frame.table != m_table) { return; }

            m_frameMap.Add(frame, container);

            int amount = m_frames.Add(frame);
            if (amount == 1)
                m_refreshLockedFrames = true;

        }

        private void Remove(SpectrumFrame frame, IFrameDataDictionary container)
        {
            if (frame.table != m_table) { return; }

            m_frameMap.Remove(frame, container);

            int amount = m_frames.Remove(frame);
            if (amount == 0)
                m_refreshLockedFrames = true;   
        }

        public void Add(IFrameDataDictionary dictionary)
        {
            if (m_dictionariesHash.TryAddOnce(dictionary))
            {
                for (int i = 0, n = dictionary.Count; i < n; i++)
                    Add(dictionary[i], dictionary);
            }
        }

        public void Remove(IFrameDataDictionary dictionary)
        {
            if (m_dictionariesHash.TryRemove(dictionary))
            {
                for (int i = 0, n = dictionary.Count; i < n; i++)
                    Remove(dictionary[i], dictionary);
            }
        }

        private SignalDelegates.Signal<IFrameDataDictionary, SpectrumFrame> m_onFrameAddedDelegate;
        private void OnFrameAdded(IFrameDataDictionary dictionary, SpectrumFrame frame)
        {
            Add(frame, dictionary);
        }

        private SignalDelegates.Signal<IFrameDataDictionary, SpectrumFrame> m_onFrameRemovedDelegate;
        private void OnFrameRemoved(IFrameDataDictionary dictionary, SpectrumFrame frame)
        {
            Remove(frame, dictionary);
        }

        protected override void InternalLock()
        {

            if (!m_refreshLockedFrames) { return; }
            m_refreshLockedFrames = false;

            m_lockedFrames.Clear();

            List<SpectrumFrame> frames = m_frames.list;
            int count = frames.Count;

            MakeLength(ref m_inputFrameData, count);
            MakeLength(ref m_outputFrameSamples, count);

            for (int i = 0, n = count; i < n; i++)
            {
                m_lockedFrames.Add(frames[i]);
                m_inputFrameData[i] = m_lockedFrames[i];
            }

            enabled = m_lockedFrames.Count > 0;
            m_recompute = true;

        }

        protected override void Prepare(float delta)
        {

            if (!cacheFrameData)
            {
                int count = m_lockedFrames.Count;
                for (int i = 0, n = count; i < n; i++)
                    m_inputFrameData[i] = m_lockedFrames[i];
            }

            m_readBands.inputFrameData = m_inputFrameData;
            m_readBands.outputFrameSamples = m_outputFrameSamples;

            m_readBrackets.inputFrameData = m_inputFrameData;
            m_readBrackets.outputFrameSamples = m_outputFrameSamples;

            m_readSpectrum.inputFrameData = m_inputFrameData;
            m_readSpectrum.outputFrameSamples = m_outputFrameSamples;

        }

        protected override void Apply()
        {
            for(int i = 0, n = m_lockedFrames.Count; i < n; i++)
            {

                SpectrumFrame frame = m_lockedFrames[i];

                List<IFrameDataDictionary> list;
                if (!m_frameMap.TryGet(frame, out list)) { continue; }

                Sample sample = m_outputFrameSamples[i];

                for(int j = 0, jn = list.Count; j < jn; j++)
                    list[j].Set(frame, sample);

            }
        }

        protected override void InternalDispose()
        {
            base.InternalDispose();
            m_inputFrameData.Release();
            m_outputFrameSamples.Release();
        }

    }
}
