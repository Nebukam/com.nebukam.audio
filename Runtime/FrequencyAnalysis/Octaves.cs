using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;

namespace Nebukam.Audio.FrequencyAnalysis
{

    public enum Bins
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048
    }

    internal struct Octave
    {
        public int Hz;
        public int width;
        public float coverage;

        public Octave(int hz)
        {
            Hz = hz;
            coverage = 0f;
            width = 0;
        }

    }

    internal struct BandInfos
    {
        public float coverage;

        public int _256;
        public int _512;
        public int _1024;
        public int _2048;

        public int Length(Bins bins)
        {
            if (bins == Bins._256) return _256;
            if (bins == Bins._512) return _512;
            if (bins == Bins._1024) return _1024;
            if (bins == Bins._2048) return _2048;
            return 0;
        }

        public void Length(Bins bins, int size)
        {
            if (bins == Bins._256) { _256 = size; }
            if (bins == Bins._512) { _512 = size; }
            if (bins == Bins._1024) { _1024 = size; }
            if (bins == Bins._2048) { _2048 = size; }
        }

        public void ComputeCoverage()
        {
            _256 = math.max(1, (int)math.floor(coverage * (float)(int)Bins._256));
            _512 = math.max(1, (int)math.floor(coverage * (float)(int)Bins._512));
            _1024 = math.max(1, (int)math.floor(coverage * (float)(int)Bins._1024));
            _2048 = math.max(1, (int)math.floor(coverage * (float)(int)Bins._2048));
        }

        public void Increment(BandInfos other)
        {
            _256 += other._256;
            _512 += other._512;
            _1024 += other._1024;
            _2048 += other._2048;
        }

    }

    internal static class Octaves
    {

        internal const int m_maxHz = 22050;

        internal static Octave[] m_referenceOctaves = new Octave[]
        {
            new Octave( 16 ),
            new Octave( 20 ), // Start of audible range
            new Octave( 25 ),
            new Octave( 31 ),
            new Octave( 40 ),
            new Octave( 50 ),
            new Octave( 63 ),
            new Octave( 80 ),
            new Octave( 100 ),
            new Octave( 125 ),
            new Octave( 160 ),
            new Octave( 200 ),
            new Octave( 250 ),
            new Octave( 315 ),
            new Octave( 400 ),
            new Octave( 500 ),
            new Octave( 630 ),
            new Octave( 800 ),
            new Octave( 1000 ),
            new Octave( 1250 ),
            new Octave( 1600 ),
            new Octave( 2000 ),
            new Octave( 2500 ),
            new Octave( 3150 ),
            new Octave( 4000 ),
            new Octave( 5000 ),
            new Octave( 6300 ),
            new Octave( 8000 ),
            new Octave( 10000 ),
            new Octave( 12500 ),
            new Octave( 16000 ),
            new Octave( m_maxHz ) // & Up end of audible range
        };

        private static BandInfos[] m_bands8 = RemapBands(new BandInfos[8]);
        private static BandInfos[] m_bands16 = RemapBands(new BandInfos[16]);
        private static BandInfos[] m_bands32 = RemapBands(new BandInfos[32]);
        private static BandInfos[] m_bands64 = RemapBands(new BandInfos[64]);
        private static BandInfos[] m_bands128 = RemapBands(new BandInfos[128]);

        private static bool m_init = false;

        private static void Init()
        {

            if (m_init) { return; }
            m_init = true;

            Octave
                po = new Octave(0),
                o;

            for (int i = 0; i < m_referenceOctaves.Length; i++)
            {
                o = m_referenceOctaves[i];

                if (i != 0)
                    o.width = o.Hz - po.Hz;
                else
                    o.width = o.Hz;

                o.coverage = (float)o.width / (float)m_maxHz;
                m_referenceOctaves[i] = o;
                po = o;
            }

        }

        internal static BandInfos[] GetBandInfos(int band)
        {
            if (band == 8) return m_bands8;
            if (band == 16) return m_bands16;
            if (band == 32) return m_bands32;
            if (band == 64) return m_bands64;
            if (band == 128) return m_bands128;
            return null;
        }

        internal static BandInfos[] GetBandInfos(Bands band)
        {
            return GetBandInfos((int)band);
        }

        internal static BandInfos[] RemapBands(BandInfos[] bands)
        {

            Init();

            int
                numTarget = bands.Length,
                numOctaves = m_referenceOctaves.Length,
                index = 0;

            float
                iterationWidth = (float)numOctaves / (float)numTarget,
                weight;

            Octave octave;

            BandInfos
                bandSum = new BandInfos(),
                bandInfos;

            if (iterationWidth >= 1f)
            {
                for (int i = 0; i < numTarget; i++)
                {

                    weight = 0f;

                    for (int o = 0; o < iterationWidth; o++)
                    {
                        octave = m_referenceOctaves[index];
                        weight += octave.coverage;
                        index++;
                    }

                    bandInfos = bands[i];
                    bandInfos.coverage = weight;
                    bandInfos.ComputeCoverage();

                    bands[i] = bandInfos;

                    bandSum.Increment(bandInfos);

                }
            }
            else
            {
                float iSum = 0f;

                for (int i = 0; i < numTarget; i++)
                {

                    octave = m_referenceOctaves[index];

                    bandInfos = bands[i];
                    bandInfos.coverage = octave.coverage * iterationWidth;
                    bandInfos.ComputeCoverage();

                    bands[i] = bandInfos;

                    bandSum.Increment(bandInfos);

                    iSum += iterationWidth;
                    if (iSum >= 1f) { iSum = 0; index++; }

                }
            }

            SanitizeBands(bands, bandSum);

            return bands;

        }

        /// <summary>
        /// Make sure the sum of all bands is compliant with bin sizes
        /// </summary>
        /// <param name="bands"></param>
        internal static void SanitizeBands(BandInfos[] bands, BandInfos infos)
        {

            IEnumerable<Bins> bins = Enum.GetValues(typeof(Bins)).Cast<Bins>();
            int 
                legalMax, 
                currentMax, 
                diff;

            foreach (Bins bin in bins)
            {
                legalMax = (int)bin;
                currentMax = infos.Length(bin);
                diff = currentMax - legalMax;

                if (diff != 0)
                    Redistribute(bands, bin, diff);

            }

        }

        internal static void Redistribute(BandInfos[] bands, Bins bin, int amount)
        {

            BandInfos infos;

            if (amount < 0) // Need to add iterations
            {
                amount = math.abs(amount);
                for (int i = bands.Length - 1; i >= 0; i--)
                {

                    infos = bands[i];
                    int add = (int)math.ceil(amount * 0.5f);
                    infos.Length(bin, infos.Length(bin) + add);
                    bands[i] = infos;
                    amount -= add;

                    if(amount <= 0) { return; }
                }
            }
            else if (amount > 0) // Need to remove iterations
            {

                for(int i = bands.Length-1; i >= 0; i--)
                {

                    infos = bands[i];
                    int remove = (int)math.ceil(amount * 0.1f);
                    int val = infos.Length(bin);
                    int diff = val - remove;

                    if(diff <= 0)
                    {
                        remove = diff + 1;
                        diff = val - remove;
                    }

                    infos.Length(bin, diff);
                    bands[i] = infos;
                    amount -= remove;

                    if (amount <= 0) { return; }

                }
            }
        }

    }
}
