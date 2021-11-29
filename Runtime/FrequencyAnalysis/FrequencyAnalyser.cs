using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Numerics;
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public class FrequencyAnalyser
    {

        #region data

        protected int m_frequencyBins = 512;
        protected uint m_fwdBufferSize = 1024;
        protected FFTWindow m_windowType = FFTWindow.BlackmanHarris;

        protected float[] m_samples;
        protected float[] m_sampleBuffer;

        protected float[] m_fwdWindowCoefs;
        protected float[] m_multiChannelSamples;
        protected float[] m_fwdSpectrumChunks;
        protected float m_scaleFactor;

        protected float[] m_freqBands8 = new float[8];
        protected float[] m_freqBands16 = new float[16];
        protected float[] m_freqBands32 = new float[32];
        protected float[] m_freqBands64 = new float[64];
        protected float[] m_freqBands128 = new float[128];

        protected AudioSource m_audioSource;

        public int frequencyBins { get { return m_frequencyBins; } }

        public FFTWindow windowType
        {
            get { return m_windowType; }
            set { m_windowType = value; }
        }

        public AudioSource audioSource
        {
            get { return m_audioSource; }
            set { m_audioSource = value; }
        }

        public float[] freqBands8 { get { return m_freqBands8; } }
        public float[] freqBands16 { get { return m_freqBands16; } }
        public float[] freqBands32 { get { return m_freqBands32; } }
        public float[] freqBands64 { get { return m_freqBands64; } }
        public float[] freqBands128 { get { return m_freqBands128; } }

        /// <summary>
        /// Return the float sample array matching selected bands
        /// </summary>
        /// <param name="bands"></param>
        /// <returns></returns>
        public float[] GetBands(Bands bands)
        {
            if (bands == Bands.Eight) return m_freqBands8;
            if (bands == Bands.Sixteen) return m_freqBands16;
            if (bands == Bands.ThirtyTwo) return m_freqBands32;
            if (bands == Bands.SixtyFour) return m_freqBands64;
            if (bands == Bands.HundredTwentyEight) return m_freqBands128;
            return m_freqBands128;
        }

        public FFT m_FFT = new FFT();

        #endregion

        #region transforms

        protected bool m_doSmooth = true;
        protected float m_smoothDownRate = 10f;
        protected float m_scale = 1f;

        public bool doSmooth
        {
            get { return m_doSmooth; }
            set { m_doSmooth = value; }
        }

        public float smoothDownRate
        {
            get { return m_smoothDownRate; }
            set { m_smoothDownRate = value; }
        }

        public float scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        #endregion

        public FrequencyAnalyser(int bins = 512, FFTWindow FFTW = FFTWindow.BlackmanHarris)
        {

            m_frequencyBins = bins;
            m_fwdBufferSize = (uint)(m_frequencyBins * 2);
            m_windowType = FFTW;

            m_samples = new float[m_frequencyBins];
            m_sampleBuffer = new float[m_frequencyBins];

            m_fwdWindowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, m_fwdBufferSize);
            m_scaleFactor = DSP.Window.ScaleFactor.Signal(m_fwdWindowCoefs);
            m_multiChannelSamples = new float[m_fwdBufferSize * 2]; //assume 2-channel audio first
            m_fwdSpectrumChunks = new float[m_fwdBufferSize];

            m_FFT.Initialize(m_fwdBufferSize);

        }

        #region Frequency bands update

        protected void UpdateFreqBands8()
        {
            // 22050 / 512 = 43hz per sample
            // 10 - 60 hz
            // 60 - 250
            // 250 - 500
            // 500 - 2000
            // 2000 - 4000
            // 4000 - 6000
            // 6000 - 20000

            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                float average = 0;
                int sampleCount = (int)pow(2, i) * 2;

                if (i == 7)
                {
                    sampleCount += 2;
                }

                for (int j = 0; j < sampleCount; j++)
                {
                    average += m_samples[count] * (count + 1);
                    count++;
                }

                average /= count;
                m_freqBands8[i] = average * m_scale;
            }
        }

        protected void UpdateFreqBands64()
        {
            // 22050 / 512 = 43hz per sample
            // 10 - 60 hz
            // 60 - 250
            // 250 - 500
            // 500 - 2000
            // 2000 - 4000
            // 4000 - 6000
            // 6000 - 20000

            int count = 0;
            int sampleCount = 1;
            int power = 0;

            for (int i = 0; i < 64; i++)
            {
                float average = 0;

                if (i == 16 || i == 32 || i == 40 || i == 48 || i == 56)
                {
                    power++;
                    sampleCount = (int)pow(2, power);
                    if (power == 3)
                        sampleCount -= 2;
                }

                for (int j = 0; j < sampleCount; j++)
                {
                    average += m_samples[count] * (count + 1);
                    count++;
                }

                average /= count;
                m_freqBands64[i] = average * m_scale;
            }

        }

        #endregion

        /// <summary>
        /// Update the sampling values & spectrum data based on the current audioSource & clip
        /// </summary>
        /// <param name="forward"></param>
        public void Analyze(float forward = 0f)
        {
            //---   POPULATE SAMPLES
            forward = abs(forward);

            if (m_audioSource == null)
            {
                for (int i = 0; i < m_samples.Length; i++)
                    m_samples[i] = 0f;

            }
            else
            {
                if (forward == 0f)
                    m_audioSource.GetSpectrumData(m_sampleBuffer, 0, m_windowType);
                else
                    GetSpectrumData(m_audioSource.clip, m_sampleBuffer, audioSource.time + forward);

                if (m_doSmooth)
                {
                    float time = Time.deltaTime;

                    for (int i = 0; i < m_samples.Length; i++)
                    {
                        if (m_sampleBuffer[i] > m_samples[i])
                            m_samples[i] = m_sampleBuffer[i];
                        else
                            m_samples[i] = lerp(m_samples[i], m_sampleBuffer[i], time * m_smoothDownRate);
                    }
                }
                else
                {
                    for (int i = 0; i < m_samples.Length; i++)
                    {
                        m_samples[i] = m_sampleBuffer[i];
                    }
                }

            }

            UpdateFreqBands8();
            UpdateFreqBands64();

        }

        /// <summary>
        /// Update the sampling values & spectrum data based on the current audioSource
        /// </summary>
        /// <param name="time"></param>
        public void AnalyzeAt(AudioClip clip, float time)
        {
            //---   POPULATE SAMPLES


            GetSpectrumData(clip, m_sampleBuffer, time);


            for (int i = 0; i < m_samples.Length; i++)
            {
                m_samples[i] = m_sampleBuffer[i];
            }

            /*
            if (m_doSmooth)
            {
                float time = Time.deltaTime;

                for (int i = 0; i < m_samples.Length; i++)
                {
                    if (m_sampleBuffer[i] > m_samples[i])
                        m_samples[i] = m_sampleBuffer[i];
                    else
                        m_samples[i] = lerp(m_samples[i], m_sampleBuffer[i], time * m_smoothDownRate);
                }
            }
            else
            {
                for (int i = 0; i < m_samples.Length; i++)
                {
                    m_samples[i] = m_sampleBuffer[i];
                }
            }
            */

            UpdateFreqBands8();
            UpdateFreqBands64();

        }

        /// <summary>
        /// Update the values in the given samplingData with
        /// the most recently computed ones
        /// </summary>
        /// <param name="frameDictionary"></param>
        public void UpdateFrameData(FrameDataDictionary frameDictionary)
        {
            List<FrequencyFrame> frames = frameDictionary.frames;
            FrequencyFrame frame;
            for (int i = 0, n = frames.Count; i < n; i++)
            {
                frame = frames[i];
                frameDictionary.Set(frame, ReadFrame(frame));
            }
        }

        /// <summary>
        /// Compute a single Sample data based on a given SamplingDefinition
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public Sample ReadFrame(FrequencyFrame frame)
        {

            Sample sample = new Sample();
            sample.output = frame.output;

            float ampBegin = frame.amplitude.x;
            float ampEnd = ampBegin + frame.amplitude.y;
            float peak = 0f;
            float sum = 0f;
            float bandValue, bandValueRemapped;

            float[] bands = GetBands(frame.bands);

            int sampleWidth = math.max(1, frame.frequency.y);
            int seekLength = clamp(frame.frequency.x + sampleWidth, 0, bands.Length);
            int reached = 0;

            for (int i = frame.frequency.x; i < seekLength; i++)
            {
                bandValue = bands[i];

                if (bandValue >= ampBegin) { reached++; }

                bandValue = clamp(bandValue, ampBegin, ampEnd);
                bandValueRemapped = map(bandValue, ampBegin, ampEnd, 0f, 1f);
                peak = max(peak, bandValueRemapped);

                sum += bandValueRemapped;
            }

            float _average = sum / sampleWidth;

            if (frame.tolerance == Tolerance.Strict && reached != sampleWidth)
            {
                sample.average = 0f;
                sample.peak = 0f;
                sample.trigger = 0f;
                sample.sum = 0f;
            }
            else
            {
                float scale = frame.scale;

                sample.average = _average * scale;
                sample.peak = peak * scale;
                sample.trigger = (peak > 0f ? 1f : 0f) * scale;
                sample.sum = sum * scale;
            }

            return sample;

        }

        #region FFT & Sampling

        protected void GetSpectrumData(AudioClip clip, float[] buffer, float seek)
        {

            int sampleRate = clip.frequency;
            int numChannels = clip.channels;
            int multiChannelSampleSize = (int)m_fwdBufferSize * numChannels;
            int offset = (int)((float)sampleRate * seek);

            if (m_multiChannelSamples.Length != multiChannelSampleSize) //Re-adjust buffer
                m_multiChannelSamples = new float[multiChannelSampleSize];

            clip.GetData(m_multiChannelSamples, offset);

            int numProcessed = 0;
            float combinedChannelAverage = 0f;
            for (int i = 0; i < m_multiChannelSamples.Length; i++)
            {
                combinedChannelAverage += m_multiChannelSamples[i];

                // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                if ((i + 1) % numChannels == 0)
                {
                    m_fwdSpectrumChunks[numProcessed] = (combinedChannelAverage / numChannels) * m_fwdWindowCoefs[numProcessed];
                    numProcessed++;
                    combinedChannelAverage = 0f;

                }
            }

            ComplexFloat[] fftSpectrum = m_FFT.Execute(m_fwdSpectrumChunks);

            for (int i = 0, n = buffer.Length; i < n; i++)
            {
                buffer[i] = (float)(fftSpectrum[i].Magnitude * m_scaleFactor);
            }

        }

        #endregion

        public float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

    }

}