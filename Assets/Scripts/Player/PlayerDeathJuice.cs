using System.Collections;
using UnityEngine;

public class PlayerDeathJuice : MonoBehaviour
{
    [Header("Death Animation")]
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private float disappearDuration = 0.35f;
    [SerializeField] private float popMultiplier = 1.25f;
    [SerializeField] private float minimumPopSize = 0.4f;

    [Header("Death Shake")]
    [SerializeField] private float shakeDuration = 0.35f;
    [SerializeField] private float shakeStrength = 0.3f;

    private bool isDying;

    public void BeginDeath()
    {
        if (isDying)
            return;

        isDying = true;

        DisablePlayer();

        if (GameSFX.Instance != null)
        {
            GameSFX.Instance.PlayDeath();
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(
                shakeDuration,
                shakeStrength
            );
        }

        StartCoroutine(DeathRoutine());
    }

    private void DisablePlayer()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        foreach (Collider playerCollider in GetComponents<Collider>())
        {
            playerCollider.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    private IEnumerator DeathRoutine()
    {
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        Vector3 startingScale = transform.localScale;

        float popSize = Mathf.Max(
            startingScale.x * popMultiplier,
            minimumPopSize
        );

        Vector3 poppedScale = Vector3.one * popSize;
        poppedScale.z = 1f;

        float elapsed = 0f;

        // Quickly expand before disappearing.
        while (elapsed < popDuration)
        {
            float progress = elapsed / popDuration;

            transform.localScale = Vector3.Lerp(
                startingScale,
                poppedScale,
                progress
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = poppedScale;
        elapsed = 0f;

        Color startingColor = spriteRenderer != null
            ? spriteRenderer.color
            : Color.white;

        // Shrink and fade away.
        while (elapsed < disappearDuration)
        {
            float progress = elapsed / disappearDuration;

            transform.localScale = Vector3.Lerp(
                poppedScale,
                Vector3.zero,
                progress
            );

            if (spriteRenderer != null)
            {
                Color fadingColor = startingColor;
                fadingColor.a = 1f - progress;
                spriteRenderer.color = fadingColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}