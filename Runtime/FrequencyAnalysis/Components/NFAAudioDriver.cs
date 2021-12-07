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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum DriveMode
    {
        Set,
        Increment
    }

    public enum TransformProperty
    {
        Position = 1,
        Rotation = 2,
        Scale = 4
    }

    [Flags]
    public enum TransformAxis
    {
        X = 1,
        Y = 2,
        Z = 4
    }

    [System.Serializable]
    [BurstCompile]
    public struct DriverSettings
    {
        public SpectrumFrame Frame;
        public List<DriverTransformation> Transformations;
        public List<DriverShader> ShaderProperties;

        public void Apply(NFAAudioDriver comp, Sample sample)
        {
            Transform transform = comp.transform;
            DriverTransformation transformDriver;
            float driverValue;
            for (int i = 0, n = Transformations.Count; i < n; i++)
            {
                transformDriver = Transformations[i];

                if (!transformDriver.Enabled) { continue; }

                driverValue = sample.Value(transformDriver.UseFrameOutput) * transformDriver.Multiplier;
                switch (transformDriver.Property)
                {

                    #region Position
                    case TransformProperty.Position:

                        float3 pos = transformDriver.Space == Space.Self ? transform.localPosition : transform.position;

                        if (transformDriver.Axis.HasFlag(TransformAxis.X))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { pos.x = driverValue; }
                            else
                            { pos.x += driverValue; }
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Y))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { pos.y = driverValue; }
                            else
                            { pos.y += driverValue; }
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Z))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { pos.z = driverValue; }
                            else
                            { pos.z += driverValue; }
                        }

                        if (transformDriver.Space == Space.Self)
                        { transform.localPosition = pos; }
                        else
                        { transform.position = pos; }

                        break;
                    #endregion

                    #region Rotation
                    case TransformProperty.Rotation:

                        float3 incrementEulers = new float3();
                        bool3 bSetEulers = new bool3(false, false, false);

                        if (transformDriver.Axis.HasFlag(TransformAxis.X))
                        {
                            bSetEulers.x = true;
                            incrementEulers.x = driverValue;
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Y))
                        {
                            bSetEulers.y = true;
                            incrementEulers.y = driverValue;
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Z))
                        {
                            bSetEulers.z = true;
                            incrementEulers.z = driverValue;
                        }

                        if (transformDriver.Mode == DriveMode.Set)
                        {
                            float pi180 = math.PI / 180f;
                            if (transformDriver.Space == Space.Self)
                            {
                                float3 rot = transform.localRotation.eulerAngles;
                                if (bSetEulers.x) rot.x = driverValue;
                                if (bSetEulers.y) rot.y = driverValue;
                                if (bSetEulers.z) rot.z = driverValue;
                                transform.localRotation = quaternion.Euler(rot * pi180);
                            }
                            else
                            {
                                float3 rot = transform.rotation.eulerAngles;
                                if (bSetEulers.x) rot.x = driverValue;
                                if (bSetEulers.y) rot.y = driverValue;
                                if (bSetEulers.z) rot.z = driverValue;
                                transform.rotation = quaternion.Euler(rot * pi180);
                            }
                        }
                        else
                        {
                            transform.Rotate(incrementEulers.x, incrementEulers.y, incrementEulers.z);
                        }


                        break;
                    #endregion

                    #region Scale
                    case TransformProperty.Scale:
                        float3 scale = transform.localScale;

                        if (transformDriver.Axis.HasFlag(TransformAxis.X))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { scale.x = driverValue; }
                            else
                            { scale.x += driverValue; }
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Y))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { scale.y = driverValue; }
                            else
                            { scale.y += driverValue; }
                        }

                        if (transformDriver.Axis.HasFlag(TransformAxis.Z))
                        {
                            if (transformDriver.Mode == DriveMode.Set)
                            { scale.z = driverValue; }
                            else
                            { scale.z += driverValue; }
                        }

                        transform.localScale = scale;

                        break;
                        #endregion

                }
            }

            DriverShader shaderDriver;
            for (int i = 0, n = ShaderProperties.Count; i < n; i++)
            {
                shaderDriver = ShaderProperties[i];

            }

        }

    }

    [System.Serializable]
    public struct DriverTransformation
    {
        public bool Enabled;
        public OutputType UseFrameOutput;
        public DriveMode Mode;
        public Space Space;
        public TransformProperty Property;
        public TransformAxis Axis;
        public float Multiplier;
    }

    [System.Serializable]
    public struct DriverShader
    {
        public bool Enabled;
        public OutputType UseFrameOutput;
        public DriveMode Mode;
        public float Multiplier;
    }

    [AddComponentMenu("N:Toolkit/Audio/Frequency Analysis/Audio Driver")]
    public class NFAAudioDriver : MonoBehaviour
    {

        public NFAAnalyser Analyser;
        public List<DriverSettings> Drivers;

        private void Awake()
        {
            if (Analyser == null) { return; }

            // Register frames
            for (int i = 0; i < Drivers.Count; i++)
                Analyser.dataDictionary.Add(Drivers[i].Frame);
        }

        private void Update()
        {
            if (Analyser == null) { return; }

            for (int i = 0, n = Drivers.Count; i < n; i++)
                Drivers[i].Apply(this, Analyser.dataDictionary.Get(Drivers[i].Frame));
        }




    }
}
