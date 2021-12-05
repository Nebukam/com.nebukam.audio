using Nebukam.JobAssist;
using static Nebukam.JobAssist.CollectionsUtils;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public abstract class AbstractSpectrumModifier<T> : Processor<T>, ISpectrumModifier
        where T : struct, Unity.Jobs.IJob, ISpectrumModifierJob
    {

        #region Inputs

        protected bool m_inputsDirty = true;

        protected ISpectrumProvider m_inputSpectrumProvider;

        #endregion

        protected override void InternalLock() { }

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

        protected override void InternalUnlock() { }

        protected override void Apply(ref T job) { }

    }
}
