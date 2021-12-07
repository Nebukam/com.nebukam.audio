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
#endif