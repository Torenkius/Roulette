using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour
{
    [Header("Açý Ayarlarý")]
    public float rotateSpeed = 90f; // saniyede derece (örn: 90 = 1 saniyede 90°)

    // Kamerayý hangi objeyle beraber döndüreceðiz (player gövdesi)
    private Transform pivot;

    void Awake()
    {
        // Eðer kamera bir objenin altýndaysa onu pivot olarak kullan
        // deðilse kamerayý pivot kabul ederiz
        pivot = transform.parent != null ? transform.parent : transform;
    }

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
            // Y ekseni etrafýnda döndür (saða-sola bakma)
            pivot.Rotate(Vector3.up * horizontalInput * rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}
