using UnityEngine;

/// <summary>
/// Handles player-to-player knockback and continuous collision shake.
/// Attach this only to the CirclePlayer.
/// </summary>
public class PlayerCollisionJuice : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float smallerPlayerKnockback = 3f;
    [SerializeField] private float largerPlayerKnockback = 10f;
    [SerializeField] private float equalSizeKnockback = 6f;
    [SerializeField] private float knockbackDuration = 0.18f;
    [SerializeField] private float knockbackCooldown = 0.4f;

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.14f;
    [SerializeField] private float shakeStrength = 0.14f;
    [SerializeField] private float shakeInterval = 0.12f;

    private PlayerMovement thisMovement;
    private float nextAllowedKnockbackTime;
    private float nextAllowedShakeTime;

    public static bool PlayersAreTouching { get; private set; }

    private void Awake()
    {
        thisMovement = GetComponent<PlayerMovement>();
    }

    private void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Triangle"))
    {
        PlayersAreTouching = true;
    }

    HandlePlayerContact(collision);
}
    private void OnCollisionStay(Collision collision)
    {
        HandlePlayerContact(collision);
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Triangle"))
        {
            PlayersAreTouching = false;
        }
    }

    private void OnDisable()
    {
        PlayersAreTouching = false;
    }
    private void HandlePlayerContact(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Triangle"))
            return;

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        // Shake repeatedly while the players remain in contact.
        if (Time.time >= nextAllowedShakeTime)
        {
            nextAllowedShakeTime = Time.time + shakeInterval;

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(shakeDuration, shakeStrength);
            }
        }

        // Apply knockback less frequently than the visual shake.
        if (Time.time < nextAllowedKnockbackTime)
            return;

        PlayerMovement otherMovement =
            collision.gameObject.GetComponent<PlayerMovement>();

        if (thisMovement == null || otherMovement == null)
            return;

        nextAllowedKnockbackTime = Time.time + knockbackCooldown;

        Vector3 directionToCircle =
            transform.position - collision.transform.position;

        directionToCircle.z = 0f;

        if (directionToCircle.sqrMagnitude < 0.001f)
        {
            directionToCircle = Vector3.left;
        }

        directionToCircle.Normalize();

        Vector3 directionToTriangle = -directionToCircle;

        float circleSize = transform.localScale.x;
        float triangleSize = collision.transform.localScale.x;

        if (Mathf.Approximately(circleSize, triangleSize))
        {
            thisMovement.ApplyKnockback(
                directionToCircle * equalSizeKnockback,
                knockbackDuration
            );

            otherMovement.ApplyKnockback(
                directionToTriangle * equalSizeKnockback,
                knockbackDuration
            );
        }
        else if (circleSize < triangleSize)
        {
            thisMovement.ApplyKnockback(
                directionToCircle * smallerPlayerKnockback,
                knockbackDuration
            );

            otherMovement.ApplyKnockback(
                directionToTriangle * largerPlayerKnockback,
                knockbackDuration
            );
        }
        else
        {
            thisMovement.ApplyKnockback(
                directionToCircle * largerPlayerKnockback,
                knockbackDuration
            );

            otherMovement.ApplyKnockback(
                directionToTriangle * smallerPlayerKnockback,
                knockbackDuration
            );
        }
    }
}