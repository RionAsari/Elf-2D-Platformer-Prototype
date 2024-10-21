using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float lifetime = 5f; // Time before the arrow is destroyed
    public float colliderDelay = 0.2f; // Time before the collider is enabled
    private Collider2D arrowCollider; // Arrow's collider
    private float damage; // Damage dealt by the arrow
    public float chargeLevel; // Added for managing charge levels
    public bool isSpecialArrow = false; // Flag to identify if the arrow is special

    private void Start()
    {
        // Get the collider component
        arrowCollider = GetComponent<Collider2D>();

        // Disable the collider initially
        arrowCollider.enabled = false;

        // Enable collider after delay
        StartCoroutine(EnableColliderAfterDelay());

        // Destroy the arrow after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    // Coroutine to enable collider after a short delay
    private IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(colliderDelay);
        arrowCollider.enabled = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if it collides with an enemy
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("LightGrunt"))
        {
            LightGrunt enemy = collision.gameObject.GetComponent<LightGrunt>();
            if (enemy != null)
            {
                if (isSpecialArrow)
                {
                    // Disable the enemy if it's a special arrow
                    StartCoroutine(enemy.DisableEnemy(5f)); // Disable for 5 seconds
                }
                else
                {
                    // Calculate damage based on charge level for regular arrows
                    int damageToDeal = CalculateDamage();
                    damageToDeal = Mathf.Min(damageToDeal, Mathf.FloorToInt(enemy.maxHealth));
                    enemy.TakeDamage(damageToDeal); // Call method to reduce health
                }
            }
        }

        // Destroy the arrow upon hitting an object
        Destroy(gameObject);
    }

    // Calculate damage based on charge level
    private int CalculateDamage()
    {
        if (chargeLevel >= 0.01f && chargeLevel < 0.5f) // 1%-49% charge
        {
            return 25;
        }
        else if (chargeLevel >= 0.5f && chargeLevel < 1f) // 50%-99% charge
        {
            return 50;
        }
        else if (chargeLevel >= 1f) // 100% charge
        {
            return 100;
        }
        return 0; // No damage if charge level is 0
    }

    // Function to set the damage of the arrow
    public void SetDamage(float arrowDamage)
    {
        damage = arrowDamage; // Set damage value
    }

    // Function to set the charge level of the arrow
    public void SetChargeLevel(float level)
    {
        chargeLevel = level; // Set charge level
    }
}
