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
