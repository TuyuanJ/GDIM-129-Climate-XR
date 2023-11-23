using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MagnetFishing
{
    public class AudioManager : MonoBehaviour/*Singleton<AudioManager>*/
    {
        [SerializeField] private AudioClip _ambientSound;
        //[SerializeField] private AudioClip _loopingMusic;

        private static AudioManager _audioManager;

        private readonly static int _audioSourceNum = 10;
        private Queue<AudioSource> _audioSources = new Queue<AudioSource>();

        private void Awake()
        {
            _audioManager = this;
        }

        private void OnEnable()
        {
            if (transform.childCount > 0) return;

            for (int i = 0; i < _audioSourceNum; i++)
            {
                var audioSourceGameObject = new GameObject("AudioSource " + i);
                audioSourceGameObject.transform.SetParent(transform);
                var audioSource = audioSourceGameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                _audioSources.Enqueue(audioSource);
            }
        }

        private void Start()
        {
            PlayClip(_ambientSound, true, false, 0.075f);
            //PlayClip(_loopingMusic, true, false, 0.3f);
        }

        public static void PlayClip(AudioClip clip, bool looping, bool randPitch, float volume = 0.5f, float pitch = 1f)
        {
            if (clip == null) throw new Exception("Cannot PLAY " + clip.name + " because it's null");

            foreach (Transform audioSource in _audioManager.transform)
            {
                var source = audioSource.GetComponent<AudioSource>();

                if (source.clip == null)
                {
                    source.clip = clip;
                    AudioHandle(looping, randPitch, source, clip, volume, pitch);
                    return;
                }
                else if (source.clip == clip)
                {
                    AudioHandle(looping, randPitch, source, clip, volume, pitch);
                    return;
                }
            }

            throw new Exception("Cannot PLAY " + clip.name + " because there are no available Audio Sources to play from");
        }

        public void StopClip(AudioClip clipToStop)
        {
            if (clipToStop == null) return;

            foreach (Transform audioSource in transform)
            {
                var source = audioSource.GetComponent<AudioSource>();

                if (source.clip == clipToStop)
                {
                    if (source.isPlaying)
                        source.Stop();
                    source.clip = null;
                    return;
                }
            }
        }

        private static void AudioHandle(bool looping, bool randPitch, AudioSource source, AudioClip clip, float volume, float pitch)
        {
            pitch = randPitch ? Random.Range(pitch - 0.1f, pitch + 0.6f) : pitch;

            if (looping)
            {
                source.volume = Mathf.Min(1, volume);
                source.pitch = Mathf.Min(3, pitch);
                source.loop = looping;
                if (!source.isPlaying)
                    source.Play();
            }
            else
            {
                _audioManager.StartCoroutine(ClipHandle(source, clip, source.clip.length, pitch, volume));
            }
        }

        private static IEnumerator ClipHandle(AudioSource source, AudioClip clip, float clipLength, float pitch, float volume)
        {
            source.pitch = Mathf.Min(3, pitch);
            source.PlayOneShot(clip, Mathf.Min(1, volume));
            yield return new WaitForSeconds(clipLength);
            source.clip = null;
        }
    }
}
