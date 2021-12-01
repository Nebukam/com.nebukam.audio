using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nebukam.Editor;
using static Nebukam.Editor.EditorDrawer;
using UnityEngine;
using UnityEditor;

namespace Nebukam.Audio.Editor
{
    public static class AudioPlayer
    {

        const string ID = "-::N:AudioPlayer::-";

        private static AudioPlayerComponent m_audioPlayer;

        private static void __EnsurePlayerExists()
        {
            if (m_audioPlayer == null)
            {
                GameObject GO = GameObject.Find(ID);

                if(GO != null)
                    m_audioPlayer = GO.GetComponent<AudioPlayerComponent>();                
            }

            if (m_audioPlayer == null)
            {
                GameObject go = new GameObject(ID);
                m_audioPlayer = go.AddComponent<AudioPlayerComponent>();
                m_audioPlayer.Init();
                go.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private static AudioPlayerComponent audio 
        {
            get
            {
                __EnsurePlayerExists();
                return m_audioPlayer;
            }
        }

        public static bool isPlaying
        {
            get{ return audio.isPlaying; }
        }

        public static float currentTime
        {
            get { return audio.currentTime; }
            set { audio.currentTime = value; }
        }

        public static AudioClip clip
        {
            get { return audio.audioClip; }
            set { audio.audioClip = value; }
        }

        public static void Play()
        {
            audio.Play();
        }

        public static void Play(AudioClip clip, float time = 0f)
        {
            audio.Play(clip, time);
        }

        public static void Pause(bool toggle = false)
        {
            if (toggle)
            {
                if (!audio.isPlaying)
                    audio.UnPause();
                else
                    audio.Pause();
            }
            else
            {
                audio.Pause();
            }
        }

        public static void Stop()
        {
            audio.Stop();
        }

    }
}
