using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Açý Ayarlarý")]
    public float rotateSpeed = 90f; // saniyede derece

    void Update()
    {
        float horizontalInput = 0f;

        // Q = sola, E = saða
        if (Input.GetKey(KeyCode.Q))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            horizontalInput = 1f;
        }

        if (Mathf.Abs(horizontalInput) > 0f)
        {
            // Kamerayý kendi etrafýnda Y ekseninde döndür
            transform.Rotate(
                Vector3.up * horizontalInput * rotateSpeed * Time.deltaTime,
                Space.Self
            );
        }
    }
}
