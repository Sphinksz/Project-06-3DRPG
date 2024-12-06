using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio
{
    public class AudioHandler : MonoBehaviour
    {
        [SerializeField] private List<AudioData> AudioData;
        [SerializeField] private AudioSource audioSource;

        public void PlayClip(string clipName, bool loop = false)
        {
            foreach (var t in AudioData.Where(t => t.Name == clipName))
            {
                audioSource.loop = loop;
                audioSource.PlayOneShot(t.Clip);
            }
        }

        public void EndCurrentClip()
        {
            audioSource.Stop();
        }

        public AudioData CreateNewAudioData(string clipName, AudioClip clip)
        {
            var newAudioData = new AudioData
            {
                Name = clipName,
                Clip = clip
            };
            return newAudioData;
        }

        public void AddAudioData(AudioData audioData)
        {
            AudioData.Add(audioData);
        }
    }
}