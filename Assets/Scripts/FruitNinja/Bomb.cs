using UnityEngine;

public class Bomb : MonoBehaviour
{
    public int points = -1;
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().Explode();
            FindObjectOfType<GameManager>().IncreaseScore(points);

            Destroy(gameObject);
        }
    }
}
