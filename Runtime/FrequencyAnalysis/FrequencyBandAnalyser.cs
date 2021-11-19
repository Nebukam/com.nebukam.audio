using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace Nebukam.FrequencyAnalysis
{

    public enum RangeType
    {
        Trigger, // 0f-1f
        Peak, // 0.0f...1.0f
        Average, // 0.5f
        Sum // Sums all values in frame.
    }

    public struct Sample
    {

        public float average;
        public float trigger;
        public float peak;
        public float sum;

        public RangeType range;

        public bool ON { get { return trigger > 0f; } }

        public static implicit operator float(Sample s)
        {

            if (s.range == RangeType.Trigger) { return s.trigger; }
            if (s.range == RangeType.Peak) { return s.peak; }
            if (s.range == RangeType.Average) { return s.average; }
            if (s.range == RangeType.Sum) { return s.sum; }

            return s.average;

        }


    }

    public enum Bands
    {
        Eight, // 8
        SixtyFour // 64
    }

    public enum Tolerance
    {
        Loose, // Any
        Strict // All frequencies need to be accounted for
    }

    [System.Serializable]
    public struct SamplingDefinition
    {

        public const float maxAmplitude8 = 3.0f;
        public const float maxAmplitude64 = 1.0f;

        [Tooltip("Band reference (8 or 64)")]
        public Bands bands;

        [Tooltip("How to process the data within the frame")]
        public RangeType range;

        [Tooltip("How tolerant is the sampling result. Strict = all frequency must have a value")]
        public Tolerance tolerance;

        [Tooltip("Horizontal start & width of the frame (limited by bands)")]
        public int2 frequency;

        [Tooltip("Vertical start & height of the frame (limited by frequency)")]
        public float2 amplitude;

        [Tooltip("Raw output scale")]
        public float scale;

        public string ID;

        [Tooltip("Debug Color")]
        public Color color;

        static public void Sanitize(ref SamplingDefinition def)
        {

            def.frequency = new int2(
                clamp(def.frequency.x, 0, 63),
                clamp(def.frequency.y, 0, 63));

            def.amplitude = new float2(
                clamp(def.amplitude.x, 0f, 3f),
                clamp(def.amplitude.y, 0f, 3f));

        }

    }

    public class FrequencyBandAnalyser
    {

        #region data

        protected int m_frequencyBins = 512;
        protected FFTWindow m_windowType = FFTWindow.BlackmanHarris;

        float[] m_samples;
        float[] m_sampleBuffer;

        protected float[] m_freqBands8;
        protected float[] m_freqBands64;

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

        public float[] freqBands64 { get { return m_freqBands64; } }

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

        public FrequencyBandAnalyser(int bins = 512, FFTWindow FFTW = FFTWindow.BlackmanHarris)
        {
            m_frequencyBins = 512;// bins;
            m_windowType = FFTW;

            m_freqBands8 = new float[8];
            m_freqBands64 = new float[64];

            m_samples = new float[m_frequencyBins];
            m_sampleBuffer = new float[m_frequencyBins];
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
        /// Update the sampling & spectrum data based from a given audioSource
        /// </summary>
        public void Update()
        {
            //---   POPULATE SAMPLES
            if (m_audioSource == null)
            {
                for (int i = 0; i < m_samples.Length; i++)
                {
                    m_samples[i] = 0f;
                }
            }
            else
            {

                m_audioSource.GetSpectrumData(m_sampleBuffer, 0, m_windowType);

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
        /// Update the values in the given samplingData with
        /// the most recently computed ones
        /// </summary>
        /// <param name="samplingData"></param>
        public void UpdateSamplingData(SamplingData samplingData)
        {

            List<SamplingDefinitionList> lists = samplingData.lists;

            for (int l = 0, ln = lists.Count; l < ln; l++)
            {

                List<SamplingDefinition> defList = lists[l].Definitions;
                SamplingDefinition def;
                for (int i = 0, n = defList.Count; i < n; i++)
                {
                    def = defList[i];
                    samplingData.Set(def.ID, ReadDefinition(def));

                }

            }
        }

        /// <summary>
        /// Compute a single Sample data based on a given SamplingDefinition
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public Sample ReadDefinition(SamplingDefinition def)
        {

            Sample sample = new Sample();
            sample.range = def.range;

            float _peak = 0f;
            float _floor = def.amplitude.x;
            float _ceiling = _floor + def.amplitude.y;
            float _sum = 0f;

            int _width = max(1, def.frequency.y);
            int _sn = clamp(def.frequency.x + _width, 0, 63);
            int _reached = 0;

            for (int s = def.frequency.x; s < _sn; s++)
            {
                float sampleValue = m_freqBands64[s];

                if (sampleValue >= _floor) { _reached++; }

                sampleValue = clamp(sampleValue, _floor, _ceiling);
                float mappedValue = map(sampleValue, _floor, _ceiling, 0f, 1f);

                if (mappedValue > _peak) { _peak = mappedValue; }
                _sum += mappedValue;
            }

            float _average = _sum / ((def.frequency.y == 0 ? 1 : def.frequency.y));

            if(def.tolerance == Tolerance.Strict && _reached != _width)
            {
                sample.average = 0f;
                sample.peak = 0f;
                sample.trigger = 0f;
                sample.sum = 0f;
            }
            else
            {
                float scale = def.scale;

                sample.average = _average * scale;
                sample.peak = _peak * scale;
                sample.trigger = (_peak > 0f ? 1f : 0f) * scale;
                sample.sum = _sum * scale;
            }

            return sample;

        }

        public float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

    }

}