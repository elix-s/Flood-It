using Common.AssetsSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading.Tasks;
using DG.DemiEditor;
using DG.Tweening;

namespace Common.AudioService
{
    public class AudioService
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IAssetUnloader _assetUnloader;
        private readonly IObjectResolver _container;

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        public AudioService(IAssetProvider assetProvider, IAssetUnloader assetUnloader, IObjectResolver container)
        {
            _assetProvider = assetProvider;
            _assetUnloader = assetUnloader;
            _container = container;
        }

        public async UniTask InstantiateAudioSources(string musicSourceAddress, string sfxSourceAddress = null)
        {
            if (!musicSourceAddress.IsNullOrEmpty())
            {
                GameObject musicPrefab = await _assetProvider.GetAssetAsync<GameObject>(musicSourceAddress);

                if (musicPrefab != null)
                {
                    _assetUnloader.AddResource(musicPrefab);
                    _musicSource = _container.Instantiate(musicPrefab).GetComponent<AudioSource>();
                }
            }

            if (!sfxSourceAddress.IsNullOrEmpty())
            {
                GameObject sfxPrefab = await _assetProvider.GetAssetAsync<GameObject>(sfxSourceAddress);

                if (sfxPrefab != null)
                {
                    _assetUnloader.AddResource(sfxPrefab);
                    _sfxSource = _container.Instantiate(sfxPrefab).GetComponent<AudioSource>();
                }
            }
        }

        public async void PlayMusic(string musicClipAddress, bool loop = true, float fadeInDuration = 0f)
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
                
                var musicClip = await _assetProvider.GetAssetAsync<AudioClip>(musicClipAddress);
                _musicSource.clip = musicClip;
                _musicSource.loop = loop;

                if (fadeInDuration > 0f)
                {
                    _musicSource.volume = 0f;
                    _musicSource.Play();
                    _musicSource.DOFade(_musicVolume, fadeInDuration);
                }
                else
                {
                    _musicSource.volume = _musicVolume;
                    _musicSource.Play();
                }
            }
            else
            {
                Debug.LogWarning("Music AudioSource not instantiated.");
            }
        }

        public void StopMusic(float fadeOutDuration = 0f)
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                if (fadeOutDuration > 0f)
                {
                    _musicSource.DOFade(0f, fadeOutDuration)
                        .OnComplete(() => _musicSource.Stop());
                }
                else
                {
                    _musicSource.Stop();
                }
            }
        }

        public void PlaySfx(AudioClip sfxClip)
        {
            if (_sfxSource != null)
            {
                _sfxSource.PlayOneShot(sfxClip, _sfxVolume);
            }
            else
            {
                Debug.LogWarning("SFX AudioSource not instantiated.");
            }
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);

            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }

        public void PauseMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.UnPause();
            }
        }
    }
}
