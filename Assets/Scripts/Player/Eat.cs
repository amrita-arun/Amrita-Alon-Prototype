using UnityEngine;

/// <summary>
/// Handles eating food to grow, and shrinking when the wrong food is eaten.
/// All game-over / win logic now lives in GameManager — this script only
/// reports the "reached minimum size" event when it happens.
/// </summary>
public class PlayerEat : MonoBehaviour
{
    [Header("Growth Settings")]
    public float increase = 0.5f;
    public float decrease = 0.5f;
    public float minimumSize = 0.1f;

    private string myTag;

    void Awake()
    {
        myTag = gameObject.tag;
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't process eating during the countdown or after the game has ended.
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        if (other.CompareTag("TriangleFood"))
        {
            HandleFood(matchingTag: "Triangle", food: other.gameObject);
        }
        else if (other.CompareTag("CircleFood"))
        {
            HandleFood(matchingTag: "Circle", food: other.gameObject);
        }
    }

    private void HandleFood(string matchingTag, GameObject food)
    {
        if (myTag == matchingTag)
        {
            Grow();
        }
        else
        {
            Shrink();
        }

        Destroy(food);
    }

    private void Grow()
    {
        transform.localScale += Vector3.one * increase;
    }

    private void Shrink()
    {
        float newSize = Mathf.Max(transform.localScale.x - decrease, minimumSize);
        transform.localScale = Vector3.one * newSize;

        if (newSize <= minimumSize)
        {
            // Report it first (while this object still exists) so GameManager can
            // record the winner, freeze the survivor, and show the overlay.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerReachedMinimumSize(myTag);
            }

            // Then destroy this player, same as the original Eat.cs behavior.
            Destroy(gameObject);
        }
    }
}