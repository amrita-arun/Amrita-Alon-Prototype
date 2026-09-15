using UnityEngine;

public class Eat : MonoBehaviour
{
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
        if (other.CompareTag("TriangleFood"))
        {
            if (myTag == "Triangle")
            {
                transform.localScale += Vector3.one * increase;
            }
            else
            {
                Shrink();
            }

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("CircleFood"))
        {
            if (myTag == "Circle")
            {
                transform.localScale += Vector3.one * increase;
            }
            else
            {
                Shrink();
            }

            Destroy(other.gameObject);
        }
    }

    void Shrink()
    {
        float newSize = transform.localScale.x - decrease;

        if (newSize <= minimumSize)
        {
            Debug.Log("GAME OVER!");
            Destroy(gameObject);
            return;
        }

        transform.localScale = Vector3.one * newSize;
    }
}