using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameSFX : MonoBehaviour
{
    public static GameSFX Instance { get; private set; }

    [Header("Optional Replacement Clips")]
    [SerializeField] private AudioClip correctFoodClip;
    [SerializeField] private AudioClip wrongFoodClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float correctFoodVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float wrongFoodVolume = 0.45f;

    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.6f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        // Generate placeholders when no imported clips are assigned.
        if (correctFoodClip == null)
        {
            correctFoodClip = CreateTone(
                "CorrectFoodPlaceholder",
                750f,
                0.10f
            );
        }

        if (wrongFoodClip == null)
        {
            wrongFoodClip = CreateBuzz(
                "WrongFoodPlaceholder",
                260f,
                0.3f
            );
        }

        if (deathClip == null)
        {
            deathClip = CreateTone(
                "DeathPlaceholder",
                100f,
                0.35f
            );
        }
    }

    public void PlayCorrectFood()
    {
        audioSource.PlayOneShot(correctFoodClip, correctFoodVolume);
    }

    public void PlayWrongFood()
    {
        audioSource.PlayOneShot(wrongFoodClip, wrongFoodVolume);
    }

    public void PlayDeath()
    {
        audioSource.PlayOneShot(deathClip, deathVolume);
    }

    private AudioClip CreateTone(
        string clipName,
        float frequency,
        float duration
    )
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;

            // Fade the sound out to avoid an abrupt click at the end.
            float fade = 1f - ((float)i / sampleCount);

            samples[i] =
                Mathf.Sin(2f * Mathf.PI * frequency * time) *
                fade *
                0.5f;
        }

        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount,
            1,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }

    private AudioClip CreateBuzz(
    string clipName,
    float frequency,
    float duration
    )
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float fade = 1f - ((float)i / sampleCount);

            float firstTone =
                Mathf.Sin(2f * Mathf.PI * frequency * time);

            float secondTone =
                Mathf.Sin(2f * Mathf.PI * (frequency * 1.4f) * time);

            samples[i] =
                (firstTone + secondTone) *
                0.3f *
                fade;
        }

        AudioClip clip = AudioClip.Create(
            clipName,
            sampleCount,
            1,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);
        return clip;
    }
    }