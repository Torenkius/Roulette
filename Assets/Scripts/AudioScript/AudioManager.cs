using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    // Ses çalacak olan hoparlör bileþeni
    public AudioSource soundSource;

    [Header("Audio Clips")]
    public AudioClip background_sound;
    public AudioClip empty_gun_sound;
    public AudioClip healing_sound;
    public AudioClip gun_powder_sound;
    public AudioClip shield_sound;
    public AudioClip whip_sound;
    public AudioClip pickup_sound;
    public AudioClip full_gun_sound;
    public AudioClip reload_sound;
    public AudioClip chest_sound;
    public AudioClip die_sound;

    // --- FUNCTIONS ---
    void Awake()
    {
        // Bu yapý sayesinde AudioManager.instance diyerek her yerden eriþebiliriz
        if (instance == null)
        {
            instance = this;
        }
    }
    public void Play_background_sound()
    {
        soundSource.clip = background_sound;
        soundSource.loop = true;
        soundSource.Play();
    }

    public void Play_empty_gun_sound()
    {
        soundSource.PlayOneShot(empty_gun_sound);
    }

    public void Play_healing_sound()
    {
        soundSource.PlayOneShot(healing_sound);
    }

    public void Play_gun_powder_sound()
    {
        soundSource.PlayOneShot(gun_powder_sound);
    }

    public void Play_shield_sound()
    {
        soundSource.PlayOneShot(shield_sound);
    }

    public void Play_whip_sound()
    {
        soundSource.PlayOneShot(whip_sound);
    }

    public void Play_pickup_sound()
    {
        soundSource.PlayOneShot(pickup_sound);
    }

    public void Play_full_gun_sound()
    {
        soundSource.PlayOneShot(full_gun_sound);
    }

    public void Play_reload_sound()
    {
        soundSource.PlayOneShot(reload_sound);
    }

    public void Play_chest_sound()
    {
        soundSource.PlayOneShot(chest_sound);
    }

    public void Play_die_sound()
    {
        soundSource.PlayOneShot(die_sound);
    }
}