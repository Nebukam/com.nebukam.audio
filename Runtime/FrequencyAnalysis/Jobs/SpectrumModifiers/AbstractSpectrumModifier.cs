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

    public interface ISpectrumModifierJob
    {

    }

    public interface ISpectrumModifier : IProcessor
    {

    }

    [BurstCompile]
    public abstract class AbstractSpectrumModifier<T> : Processor<T>, ISpectrumModifier
        where T : struct, Unity.Jobs.IJob, ISpectrumModifierJob
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion

        protected override void Prepare(ref T job, float delta)
        {

            if (m_inputsDirty)
            {

                if (!TryGetFirstInCompound(out m_inputSpectrumProvider))
                {
                    throw new System.Exception("ISpectrumProvider missing");
                }

                m_inputsDirty = false;

            }

        }

        protected override void Apply(ref T job) { }

    }
}
