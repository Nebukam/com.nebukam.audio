using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using System.Numerics;
using System;

namespace Nebukam.Audio.FrequencyAnalysis
{

    

    public struct SamplingInfos
    {

        public int numSamples;
        public float duration;
        public int sampleRate; // Sample per second
        public int pointCount; // Number of samples used for sample analysis
        public int iterations; // Number of analysis iterations required to cover a single second
        public float timeStep; // Timestep of the analysis at the current rate

        /// <summary>
        /// Number of analysis iterations required to cover a given duration
        /// with the current parameters
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public int GetIterations(float duration)
        {
            return (int)(iterations * duration);
        }

        public float GetClampedTime(float time)
        {
            return time;
        }

        public SamplingInfos(FrequencyAnalyser analyser, AudioClip clip)
        {
            numSamples = clip.samples;
            duration = clip.length;
            sampleRate = (int)(numSamples / duration);
            pointCount = (int)analyser.pointCount;
            iterations = sampleRate / (int)analyser.pointCount;
            timeStep = 1f / iterations;
        }

    }

    public struct SpectrumInfos
    {

        public int frequency;
        public int sampleFrequency;
        public int numChannels;
        public int numSamples;
        public int pointCount;

        public int coverage; // Number of samples required to cover all channels

        public SpectrumInfos(FrequencyAnalyser analyser, AudioClip clip)
        {
            frequency = clip.frequency;
            numSamples = clip.samples;
            numChannels = clip.channels;
            pointCount = (int)analyser.pointCount;
            coverage = pointCount * numChannels; // Signals are per-channel inlined in data
            sampleFrequency = frequency / (int)analyser.frequencyBins;
        }

        public void EnsureCoverage(ref float[] array)
        {
            if (array.Length != coverage) { array = new float[coverage]; }
        }

        public int TimeIndex(float time)
        {
            return (int)(frequency * time);
        }

    }

    public class FrequencyAnalyser
    {

        #region data

        protected Bins m_frequencyBins = Bins._512;
        protected uint m_pointCount = 1024;
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

        public Bins frequencyBins { get { return m_frequencyBins; } }

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

        public uint pointCount { get { return m_pointCount; } }

        /// <summary>
        /// Return the float sample array matching selected bands
        /// </summary>
        /// <param name="bands"></param>
        /// <returns></returns>
        public float[] GetBands(Bands bands)
        {
            if (bands == Bands._8) return m_freqBands8;
            if (bands == Bands._16) return m_freqBands16;
            if (bands == Bands._32) return m_freqBands32;
            if (bands == Bands._64) return m_freqBands64;
            if (bands == Bands._128) return m_freqBands128;
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

        public FrequencyAnalyser(Bins bins = Bins._512, FFTWindow FFTW = FFTWindow.BlackmanHarris)
        {
            Init(bins, FFTW);
        }

        public void Init(Bins bins = Bins._512, FFTWindow FFTW = FFTWindow.BlackmanHarris)
        {

            m_frequencyBins = bins;
            m_pointCount = (uint)((int)m_frequencyBins * 2);
            m_windowType = FFTW;

            m_samples = new float[(int)m_frequencyBins];
            m_sampleBuffer = new float[(int)m_frequencyBins];

            m_fwdWindowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, m_pointCount);
            m_scaleFactor = DSP.Window.ScaleFactor.Signal(m_fwdWindowCoefs);
            m_multiChannelSamples = new float[m_pointCount * 2]; //assume 2-channel audio as default
            m_fwdSpectrumChunks = new float[m_pointCount];

            m_FFT.Initialize(m_pointCount);

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
            // 10 - 60 hz - 1.3
            // 60 - 250 - 4
            // 250 - 500 - 5.8
            // 500 - 2000 - 34
            // 2000 - 4000 - 46
            // 4000 - 6000 - 46
            // 6000 - 20000 - 325

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

        #region Alt band update

        protected void UpdateFrequencyBands()
        {

            // 22050 / 512 = 43hz per sample
            // 10 - 60 hz
            // 60 - 250
            // 250 - 500
            // 500 - 2000
            // 2000 - 4000
            // 4000 - 6000
            // 6000 - 20000

            UpdateFrequencyBand(m_freqBands8);
            UpdateFrequencyBand(m_freqBands16);
            UpdateFrequencyBand(m_freqBands32);
            UpdateFrequencyBand(m_freqBands64);
            UpdateFrequencyBand(m_freqBands128);


        }

        protected void UpdateFrequencyBand(float[] freqBands)
        {

            int 
                bandCount = freqBands.Length,
                index = 0;

            BandInfos[] bands = Octaves.GetBandInfos(bandCount);

            for (int i = 0; i < bandCount; i++)
            {
                float average = 0;

                for (int s = 0, n = bands[i].Length(m_frequencyBins); s < n; s++)
                {
                    average += m_samples[index] * (index + 1);
                    index++;
                }

                average /= index;
                freqBands[i] = average * m_scale;
            }

        }

        #endregion

        /// <summary>
        /// Update the sampling values & spectrum data based on the current audioSource & clip
        /// </summary>
        /// <param name="forward"></param>
        public void Analyse(float forward = 0f)
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
                    ReadCombinedSpectrumData(m_audioSource.clip, audioSource.time + forward);

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

            //UpdateFreqBands8();
            //UpdateFreqBands64();
            UpdateFrequencyBands();

        }

        /// <summary>
        /// Update the sampling values & spectrum data based on the current audioSource
        /// </summary>
        /// <param name="time"></param>
        public void AnalyseAt(AudioClip clip, float time)
        {
            //---   POPULATE SAMPLES


            ReadCombinedSpectrumData(clip, time);


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

            //UpdateFreqBands8();
            //UpdateFreqBands64();
            UpdateFrequencyBands();

        }

        #region Frame reading

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
            float inputScale = frame.inputScale;
            float bandValue, bandValueRemapped;

            float[] bands = GetBands(frame.bands);

            int sampleWidth = math.max(1, frame.frequency.y);
            int seekLength = clamp(frame.frequency.x + sampleWidth, 0, bands.Length);
            int reached = 0;


            for (int i = frame.frequency.x; i < seekLength; i++)
            {
                bandValue = bands[i] * inputScale;

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
                float scale = frame.outputScale;

                sample.average = _average * scale;
                sample.peak = peak * scale;
                sample.trigger = (peak > 0f ? 1f : 0f) * scale;
                sample.sum = sum * scale;
            }

            return sample;

        }

        /// <summary>
        /// Update the values in the given samplingData with
        /// the most recently computed ones
        /// </summary>
        /// <param name="frameDictionary"></param>
        public void ReadDataDictionary(FrameDataDictionary frameDictionary)
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
        /// Analyze & read a time range into a two-dimensional Sample array.
        /// Warning : this is super expensive.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="time"></param>
        /// <param name="duration"></param>
        /// <param name="sampleArray"></param>
        public void ReadRange(AudioClip clip, FrameDataDictionary frames, float time, float duration, ref Sample[,] sampleArray)
        {

            List<FrequencyFrame> frameList = frames.frames;
            SamplingInfos infos = new SamplingInfos(this, clip);
            int iterations = infos.GetIterations(duration);
            int frameCount = frameList.Count;

            if (sampleArray.GetLength(0) != iterations
                || sampleArray.GetLength(1) != frameCount)
                sampleArray = new Sample[iterations, frameCount];

            for (int i = 0; i < iterations; i++)
            {
                AnalyseAt(clip, time + i * infos.timeStep);

                for (int f = 0; f < frameCount; f++)
                    sampleArray[i, f] = ReadFrame(frameList[f]);
            }

        }

        #endregion

        #region FFT & Sampling

        protected SpectrumInfos ReadCombinedSpectrumData(AudioClip clip, float time)
        {

            SpectrumInfos spectrum = new SpectrumInfos(this, clip);
            spectrum.EnsureCoverage(ref m_multiChannelSamples);

            int offsetSamples = spectrum.TimeIndex(time);
            if(m_multiChannelSamples.Length + offsetSamples >= clip.samples) { return spectrum; }

            clip.GetData(m_multiChannelSamples, offsetSamples);

            int index = 0;
            float combined = 0f;

            for (int i = 0; i < m_multiChannelSamples.Length; i++)
            {
                combined += m_multiChannelSamples[i];

                // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
                if ((i + 1) % spectrum.numChannels == 0)
                {
                    m_fwdSpectrumChunks[index] = (combined / spectrum.numChannels) * m_fwdWindowCoefs[index];
                    index++;
                    combined = 0f;

                }
            }

            ComplexFloat[] fftSpectrum = m_FFT.Execute(m_fwdSpectrumChunks);

            for (int i = 0, n = m_sampleBuffer.Length; i < n; i++)
                m_sampleBuffer[i] = (float)(fftSpectrum[i].Magnitude * m_scaleFactor);

            return spectrum;

        }

        /// <summary>
        /// Fill a buffer
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="chan"></param>
        /// <param name="buffer"></param>
        /// <param name="time"></param>
        protected SpectrumInfos ReadChannelSpectrumData(AudioClip clip, int chan, float time)
        {

            SpectrumInfos spectrum = new SpectrumInfos(this, clip);
            spectrum.EnsureCoverage(ref m_multiChannelSamples);

            int offsetSamples = spectrum.TimeIndex(time);
            if (m_multiChannelSamples.Length + offsetSamples >= clip.samples) { return spectrum; }

            clip.GetData(m_multiChannelSamples, spectrum.TimeIndex(time));

            int index = 0;
            for (int i = 0; i < m_multiChannelSamples.Length; i+= spectrum.numChannels)
            {
                m_fwdSpectrumChunks[index] = m_multiChannelSamples[i+chan] * m_fwdWindowCoefs[index];
                index++;
            }

            ComplexFloat[] fftSpectrum = m_FFT.Execute(m_fwdSpectrumChunks);

            for (int i = 0, n = m_sampleBuffer.Length; i < n; i++)
                m_sampleBuffer[i] = (float)(fftSpectrum[i].Magnitude * m_scaleFactor);

            return spectrum;

        }

        #endregion

        #region utils

        internal void EnsureSize(ref float[] array, int size)
        {
            if(array.Length != size) { array = new float[size]; }
        }

        internal float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        #endregion

    }

}