using UnityEngine;

public class VolleyProjectile : MonoBehaviour
{
    public float speed = 5f; 
    public float damage = 10f; 
    public float lifetime = 5f; 

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy after a certain time to avoid infinite existence
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                
            }
            Destroy(gameObject); // Destroy the projectile after hitting the player
        }
    }
}
