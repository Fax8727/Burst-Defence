using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuBGM;
    public AudioClip gameLevelBGM;

    [Header("SFX Clips")]
    public AudioClip buttonClickClip;
    public AudioClip shootClip;
    public AudioClip enemyHitClip;
    public AudioClip enemyDeathClip;
    public AudioClip playerHitClip;
    public AudioClip coreHitClip;
    public AudioClip burstClip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayClickSound()
    {
        PlaySFX(buttonClickClip);
    }
}