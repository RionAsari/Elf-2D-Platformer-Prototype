using UnityEngine;
using UnityEngine.UI;

public class HealthbarBehaviour : MonoBehaviour
{
    public Slider Slider;          // Reference to the health slider
    public Color Low;             // Color for low health
    public Color Medium;          // Color for medium health
    public Color High;            // Color for high health
    public Vector3 Offset;        // Offset for health bar position

    public void SetHealth(float health, float maxHealth)
    {
        Debug.Log($"Health set to: {health}, Max Health: {maxHealth}"); // Debug log
        Slider.gameObject.SetActive(health < maxHealth); // Show the slider if health is less than max
        Slider.value = health;                             // Set current health
        Slider.maxValue = maxHealth;                       // Set max health

        // Change color based on the health percentage
        float healthPercentage = health / maxHealth;
        if (healthPercentage < 0.3f) // Low health
        {
            Slider.fillRect.GetComponentInChildren<Image>().color = Low;
        }
        else if (healthPercentage < 0.7f) // Medium health
        {
            Slider.fillRect.GetComponentInChildren<Image>().color = Medium;
        }
        else // High health
        {
            Slider.fillRect.GetComponentInChildren<Image>().color = High;
        }
    }

    // New method to update the position of the health bar
    public void UpdatePosition(Vector3 enemyPosition)
    {
        // Update the position of the health bar to follow the enemy
        Slider.transform.position = Camera.main.WorldToScreenPoint(enemyPosition + Offset);
    }

    void Update()
    {
        // You can remove the following code if you update the position in LightGrunt
        // Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
    }
}
