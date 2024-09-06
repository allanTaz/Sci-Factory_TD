using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        fillImage.fillAmount = currentHealth / maxHealth;
    }

    private void LateUpdate()
    {
        // Make the health bar face the camera
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}