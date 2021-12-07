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

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Nebukam.Audio.Editor
{
    public class AudioPlayerComponent : MonoBehaviour
    {

        private AudioSource m_audioSource;
        private AudioClip m_audioClip;
        private bool m_paused = false;

        public AudioClip audioClip
        {
            get { return m_audioClip; }
            set { 
                m_audioClip = value;
                m_audioSource.clip = m_audioClip;
            }
        }

        public AudioSource audioSource { get { return m_audioSource; } }

        public float currentTime
        { 
            get { return m_audioSource.time; }
            set { m_audioSource.time = value; }
        }

        public bool isPlaying
        {
            get { return m_audioSource.isPlaying; }
        }

        internal void Init()
        {
            m_audioSource = this.gameObject.AddComponent<AudioSource>();
            m_audioSource.loop = true;
        }

        public void Play()
        {
            m_audioSource.Play();
        }

        public void Play(AudioClip clip, float time)
        {
            audioClip = clip;
            currentTime = time;
            m_audioSource.Play();
        }

        public void Pause()
        {
            if (!m_audioSource.isPlaying) { return; }
            m_audioSource.Pause();
        }

        public void UnPause()
        {
            if (m_audioSource.isPlaying) { return; }
            m_audioSource.UnPause();
        }

        public void Stop()
        {
            m_audioSource.Stop();
        }

        private void OnDestroy()
        {
            audioClip = null;
        }


    }
}
#endif