// Copyright (c) 2019 Timothé Lapetite - nebukam@gmail.com
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
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace Nebukam.Audio.FrequencyAnalysis
{

    [BurstCompile]
    public struct FFTCoefficientsJob : IJob
    {

        public bool m_recompute;
        public NativeArray<float> m_params;
        public FFTWindow m_windowType;
        public NativeArray<float> m_outputCoefficients;
        
        public void Execute()
        {

            if (!m_recompute) { return; }

            /// Calculates a set of Windows coefficients for a given number of points and a window type to use.

            int count = m_outputCoefficients.Length;
            float N = count;

            switch (m_windowType)
            {
                case FFTWindow.None:
                case FFTWindow.Rectangular:

                    for (int i = 0; i < count; i++)
                        m_outputCoefficients[i] = 1f;

                    break;

                case FFTWindow.Bartlett:

                    for (int i = 0; i < count; i++)
                        m_outputCoefficients[i] = 2f / N * (N / 2f - abs(i - (N - 1f) / 2f));

                    break;

                case FFTWindow.Welch:
                    for (int i = 0; i < count; i++)
                        m_outputCoefficients[i] = 1f - pow(((2f * i) / N) - 1f, 2f);
                    break;

                case FFTWindow.Hann:
                case FFTWindow.Hanning:
                    SineExpansion(
                        m_outputCoefficients,
                        0.5f,
                        -0.5f);
                    break;

                case FFTWindow.Hamming:
                    SineExpansion(
                        m_outputCoefficients,
                        0.54f,
                        -0.46f);
                    break;

                case FFTWindow.BlackmanHarris:
                case FFTWindow.BH92:
                    SineExpansion(
                        m_outputCoefficients,
                        0.35875f,
                        -0.48829f,
                        0.14128f,
                        -0.01168f);
                    break;

                case FFTWindow.Nutall3:
                    SineExpansion(
                        m_outputCoefficients,
                        0.375f,
                        -0.5f,
                        0.125f);
                    break;

                case FFTWindow.Nutall3A:
                    SineExpansion(
                        m_outputCoefficients,
                        0.40897f,
                        -0.5f,
                        0.09103f);
                    break;

                case FFTWindow.Nutall3B:
                    SineExpansion(
                        m_outputCoefficients,
                        0.4243801f,
                        -0.4973406f,
                        0.0782793f);
                    break;

                case FFTWindow.Nutall4:
                    SineExpansion(
                        m_outputCoefficients,
                        0.3125f,
                        -0.46875f,
                        0.1875f,
                        -0.03125f);
                    break;

                case FFTWindow.Nutall4A:
                    SineExpansion(
                        m_outputCoefficients,
                        0.338946f,
                        -0.481973f,
                        0.161054f,
                        -0.018027f);
                    break;

                case FFTWindow.Nutall4B:
                    SineExpansion(
                        m_outputCoefficients,
                        0.355768f,
                        -0.487396f,
                        0.144232f,
                        -0.012604f);
                    break;

                case FFTWindow.SFT3F:
                    SineExpansion(
                        m_outputCoefficients,
                        0.26526f,
                        -0.5f,
                        0.23474f);
                    break;

                case FFTWindow.SFT4F:
                    SineExpansion(
                        m_outputCoefficients,
                        0.21706f,
                        -0.42103f,
                        0.28294f,
                        -0.07897f);
                    break;

                case FFTWindow.SFT5F:
                    SineExpansion(
                        m_outputCoefficients,
                        0.1881f,
                        -0.36923f,
                        0.28702f,
                        -0.13077f,
                        0.02488f);
                    break;

                case FFTWindow.SFT3M:
                    SineExpansion(
                        m_outputCoefficients,
                        0.28235f,
                        -0.52105f,
                        0.19659f);
                    break;

                case FFTWindow.SFT4M:
                    SineExpansion(
                        m_outputCoefficients,
                        0.241906f,
                        -0.460841f,
                        0.255381f,
                        -0.041872f);
                    break;

                case FFTWindow.SFT5M:
                    SineExpansion(
                        m_outputCoefficients,
                        0.209671f,
                        -0.407331f,
                        0.281225f,
                        -0.092669f,
                        0.0091036f);
                    break;

                case FFTWindow.FTNI:
                    SineExpansion(
                        m_outputCoefficients,
                        0.2810639f,
                        -0.5208972f,
                        0.1980399f);
                    break;

                case FFTWindow.FTHP:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.912510941f,
                        1.079173272f,
                        -0.1832630879f);
                    break;

                case FFTWindow.HFT70:
                    SineExpansion(
                        m_outputCoefficients,
                        1,
                        -1.90796f,
                        1.07349f,
                        -0.18199f);
                    break;

                case FFTWindow.FTSRS:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.93f,
                        1.29f,
                        -0.388f,
                        0.028f);
                    break;

                case FFTWindow.HFT90D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.942604f,
                        1.340318f,
                        -0.440811f,
                        0.043097f);
                    break;

                case FFTWindow.HFT95:
                    SineExpansion(
                        m_outputCoefficients,
                        1,
                        -1.9383379f,
                        1.3045202f,
                        -0.4028270f,
                        0.0350665f);
                    break;

                case FFTWindow.HFT116D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.9575375f,
                        1.4780705f,
                        -0.6367431f,
                        0.1228389f,
                        -0.0066288f);
                    break;

                case FFTWindow.HFT144D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.96760033f,
                        1.57983607f,
                        -0.81123644f,
                        0.22583558f,
                        -0.02773848f,
                        0.00090360f);
                    break;

                case FFTWindow.HFT169D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.97441842f,
                        1.65409888f,
                        -0.95788186f,
                        0.33673420f,
                        -0.06364621f,
                        0.00521942f,
                        -0.00010599f);
                    break;

                case FFTWindow.HFT196D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.979280420f,
                        1.710288951f,
                        -1.081629853f,
                        0.448734314f,
                        -0.112376628f,
                        0.015122992f,
                        -0.000871252f,
                        0.000011896f);
                    break;

                case FFTWindow.HFT223D:
                    SineExpansion(
                        m_outputCoefficients,
                        1f,
                        -1.98298997309f,
                        1.75556083063f,
                        -1.19037717712f,
                        0.56155440797f,
                        -0.17296769663f,
                        0.03233247087f,
                        -0.00324954578f,
                        0.00013801040f,
                        -0.00000132725f);
                    break;

                case FFTWindow.HFT248D:
                    SineExpansion(
                        m_outputCoefficients,
                        1,
                        -1.985844164102f,
                        1.791176438506f,
                        -1.282075284005f,
                        0.667777530266f,
                        -0.240160796576f,
                        0.056656381764f,
                        -0.008134974479f,
                        0.000624544650f,
                        -0.000019808998f,
                        0.000000132974f);
                    break;

                default:
                    break;
            }

            float sum = 0f;
            for(int i = 0; i < count; i++)
                sum += m_outputCoefficients[i];
            
            m_params[FFTParams.SCALE_FACTOR] = 1.0f / (sum / (float)count);

        }

        private void SineExpansion(
            NativeArray<float> coefficients,
            float c0,
            float c1 = 0f,
            float c2 = 0f,
            float c3 = 0f,
            float c4 = 0f,
            float c5 = 0f,
            float c6 = 0f,
            float c7 = 0f,
            float c8 = 0f,
            float c9 = 0f,
            float c10 = 0f)
        {

            float TAU = 2f * PI;
            int count = coefficients.Length;

            for (int i = 0; i < count; i++)
            {
                float wc = c0;
                float zi = TAU * i / count;
                wc += c1 * cos(zi);
                wc += c2 * cos(2f * zi);
                wc += c3 * cos(3f * zi);
                wc += c4 * cos(4f * zi);
                wc += c5 * cos(5f * zi);
                wc += c6 * cos(6f * zi);
                wc += c7 * cos(7f * zi);
                wc += c8 * cos(8f * zi);
                wc += c9 * cos(9f * zi);
                wc += c10 * cos(10f * zi);

                coefficients[i] = wc;
            }

        }


    }
}
