using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class GameAudio
{
    private const string MixerResourcePath = "Audio/GameAudioMixer";
    private const string MusicGroupName = "Music";
    private const string SfxGroupName = "SFX";
    private const string MusicEnabledKey = "TowerJumping.Audio.MusicEnabled";

    private static AudioMixer _mixer;
    private static AudioMixerGroup _musicGroup;
    private static AudioMixerGroup _sfxGroup;

    public static bool IsMusicEnabled => PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

    public static AudioMixerGroup MusicGroup => GetGroup(ref _musicGroup, MusicGroupName);
    public static AudioMixerGroup SfxGroup => GetGroup(ref _sfxGroup, SfxGroupName);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _mixer = null;
        _musicGroup = null;
        _sfxGroup = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        RouteSceneAudioSources();
    }

    public static void ConfigureMusicSource(AudioSource source)
    {
        if (source == null)
            return;

        AudioMixerGroup group = MusicGroup;
        if (group != null)
            source.outputAudioMixerGroup = group;
    }

    public static void ConfigureSfxSource(AudioSource source)
    {
        if (source == null)
            return;

        AudioMixerGroup group = SfxGroup;
        if (group != null)
            source.outputAudioMixerGroup = group;
    }

    public static void PlaySfx(AudioSource source, AudioClip clip, float volumeScale = 1f)
    {
        if (source == null || clip == null)
            return;

        ConfigureSfxSource(source);
        source.PlayOneShot(clip, Mathf.Max(0f, volumeScale));
    }

    public static bool TryPlayContinueSfx(ref PauseSimpleUI pauseSfxSource, AudioSource fallbackSource = null, AudioClip fallbackClip = null, float fallbackVolume = 1f)
    {
        if (pauseSfxSource == null)
            pauseSfxSource = Object.FindAnyObjectByType<PauseSimpleUI>();

        if (pauseSfxSource != null && pauseSfxSource.TryPlayContinueSfx())
            return true;

        if (fallbackSource == null || fallbackClip == null)
            return false;

        PlaySfx(fallbackSource, fallbackClip, fallbackVolume);
        return true;
    }

    public static void StopMusic(AudioSource source)
    {
        if (source == null)
            return;

        ConfigureMusicSource(source);
        source.Stop();
    }

    public static void StopAllMusic(AudioSource exceptSource = null)
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);

        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || source == exceptSource)
                continue;

            if (!IsMusicSource(source))
                continue;

            if (source.isPlaying)
                source.Stop();
        }
    }

    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicEnabledToScene();
    }

    public static void ApplyMusicEnabled(AudioSource source)
    {
        if (source == null)
            return;

        ConfigureMusicSource(source);

        if (IsMusicEnabled)
            PlayOrResumeMusic(source);
        else
            source.Pause();
    }

    public static void RouteSceneAudioSources()
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);

        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null)
                continue;

            if (LooksLikeMusicSource(source))
                ConfigureMusicSource(source);
            else
                ConfigureSfxSource(source);
        }

        ApplyMusicEnabledToScene();
    }

    private static void ApplyMusicEnabledToScene()
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);

        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];
            if (source == null || !IsMusicSource(source))
                continue;

            if (IsMusicEnabled)
                PlayOrResumeMusic(source);
            else
                source.Pause();
        }
    }

    private static void PlayOrResumeMusic(AudioSource source)
    {
        if (source == null)
            return;

        source.UnPause();

        if (!source.isPlaying && source.clip != null && source.playOnAwake)
            source.Play();
    }

    private static bool IsMusicSource(AudioSource source)
    {
        if (source == null)
            return false;

        AudioMixerGroup musicGroup = MusicGroup;
        if (musicGroup != null && source.outputAudioMixerGroup == musicGroup)
            return true;

        return LooksLikeMusicSource(source);
    }

    private static bool LooksLikeMusicSource(AudioSource source)
    {
        if (source == null)
            return false;

        string objectName = source.gameObject != null ? source.gameObject.name : string.Empty;
        if (ContainsAudioToken(objectName, "music"))
            return true;

        AudioClip clip = source.clip;
        string clipName = clip != null ? clip.name : string.Empty;

        return ContainsAudioToken(clipName, "music")
            || ContainsAudioToken(clipName, "loop")
            || ContainsAudioToken(clipName, "arcademenu")
            || ContainsAudioToken(clipName, "bonus");
    }

    private static bool ContainsAudioToken(string value, string token)
    {
        return !string.IsNullOrEmpty(value)
            && value.ToLowerInvariant().Contains(token);
    }

    private static AudioMixerGroup GetGroup(ref AudioMixerGroup cachedGroup, string groupName)
    {
        if (cachedGroup != null)
            return cachedGroup;

        AudioMixer mixer = GetMixer();
        if (mixer == null)
            return null;

        AudioMixerGroup[] matches = mixer.FindMatchingGroups(groupName);
        if (matches == null || matches.Length == 0)
            return null;

        cachedGroup = matches[0];
        return cachedGroup;
    }

    private static AudioMixer GetMixer()
    {
        if (_mixer != null)
            return _mixer;

        _mixer = Resources.Load<AudioMixer>(MixerResourcePath);
        if (_mixer == null)
            Debug.LogWarning($"[GameAudio] AudioMixer not found at Resources/{MixerResourcePath}.");

        return _mixer;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RouteSceneAudioSources();
    }
}
