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
using Unity.Jobs;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public interface IComplexProvider : IProcessor
    {
        NativeArray<ComplexFloat> outputComplexFloats { get; }
    }

    [BurstCompile]
    public abstract class AbstractComplexProvider<T> : Processor<T>, IComplexProvider
        where T : struct, IJob, IComplexJob
    {

        protected NativeArray<ComplexFloat> m_outputComplexFloats = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> outputComplexFloats { get { return m_outputComplexFloats; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected internal ISamplesProvider m_samplesProvider;

        #endregion

        protected override void InternalLock() { }

        protected override void Prepare(ref T job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_samplesProvider))
                {
                    throw new System.Exception("ISamplesProvider missing");
                }

                m_inputsDirty = false;

            }

            int spectrumLength = (int)m_samplesProvider.frequencyBins;

            MakeLength(ref m_outputComplexFloats, spectrumLength);

            job.complexFloats = m_outputComplexFloats;

        }

        protected override void InternalUnlock() { }

        protected override void InternalDispose()
        {
            m_outputComplexFloats.Dispose();
        }

    }

    public abstract class ComplexProviderParallel<T> : ParallelProcessor<T>
        where T : struct, IJobParallelFor, IComplexJob
    {

        protected NativeArray<ComplexFloat> m_complexFloats = new NativeArray<ComplexFloat>(0, Allocator.Persistent);
        public NativeArray<ComplexFloat> complexFloats { get { return m_complexFloats; } }

        #region Inputs

        protected bool m_inputsDirty = true;

        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion

        protected override void InternalLock() { }

        protected override int Prepare(ref T job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputSpectrumProvider))
                {
                    throw new System.Exception("ISpectrumProvider missing");
                }

                m_inputsDirty = false;

            }

            int spectrumLength = (int)m_inputSpectrumProvider.frequencyBins;

            MakeLength(ref m_complexFloats, spectrumLength);
            job.complexFloats = m_complexFloats;

            return spectrumLength;
        }

        protected override void InternalUnlock() { }

        protected override void Apply(ref T job) { }

        protected override void InternalDispose()
        {
            m_complexFloats.Dispose();
        }

    }
}
