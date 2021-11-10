using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Unity.Mathematics.math;


namespace Nebukam.FrequencyAnalysis
{

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
        protected SamplingData m_samplingData;

        public int frequencyBins { get { return m_frequencyBins; } }

        public FFTWindow windowType {
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
        
        public SamplingData samplingData 
        {
            get { return m_samplingData; }
            set { m_samplingData = value; }
        }

        #endregion

        #region transforms

        protected bool m_doSmooth = true;
        protected float m_smoothDownRate = 100f;
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

        // Update is called once per frame
        public void Update()
        {
            //---   POPULATE SAMPLES
            if(m_audioSource == null)
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
                    for (int i = 0; i < m_samples.Length; i++)
                    {
                        if (m_sampleBuffer[i] > m_samples[i])
                            m_samples[i] = m_sampleBuffer[i];
                        else
                            m_samples[i] = lerp(m_samples[i], m_sampleBuffer[i], Time.deltaTime * m_smoothDownRate);
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
            if(m_samplingData != null) { UpdateSamplingData(); }

        }

        protected void UpdateSamplingData()
        {
            List<SamplingDefinition> defList = m_samplingData.definitions;
            SamplingDefinition def;
            for(int i = 0, n = defList.Count; i < n; i++)
            {
                def = defList[i];
                float peak = 0f;
                float average = 0f;
                float _floor = def.amplitude.x;
                float _ceiling = _floor + def.amplitude.y;
                float finalValue = 0f;

                int width = max(1, def.frequency.y);
                int sn = clamp(def.frequency.x + width, 0, 63);
                int reached = 0;

                for (int s = def.frequency.x; s < sn; s++)
                {
                    float sampleValue = m_freqBands64[s];

                    if(sampleValue >= _floor) { reached++; }

                    sampleValue = clamp(sampleValue, _floor, _ceiling);
                    float mappedValue = map(sampleValue, _floor, _ceiling, 0f, 1f);

                    if(mappedValue > peak) { peak = mappedValue; }
                    average += mappedValue;
                }

                average = average / ((def.frequency.y == 0 ? 1 : def.frequency.y));

                if(def.range == RangeType.AVERAGE)
                {
                    finalValue = average;
                }
                else if (def.range == RangeType.PEAK)
                {
                    finalValue = peak;
                }
                else if (def.range == RangeType.TRIGGER)
                {
                    finalValue = peak > 0f ? 1f : 0f;
                }

                if (def.tolerance == Tolerance.STRICT && reached != width) { finalValue = 0f; }

                m_samplingData.Set(def.ID, finalValue*def.scale);

            }
        }

        public float GetBandValue(int index, Bands bands)
        {
            if (bands == Bands.EIGHT)
            {
                return m_freqBands8[index];
            }
            else
            {
                return m_freqBands64[index];
            }
        }

        public float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

    }

}