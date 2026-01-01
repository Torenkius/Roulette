using System.Collections;
using UnityEngine;

public class GunReturnToTable : MonoBehaviour
{
    public float returnDelay = 4f;   // kaç saniye sonra masaya dönsün

    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Vector3 originalLocalScale;
    private int originalLayer;

    private Collider col;

    void Awake()
    {
        // MASADAKÝ HALÝNÝ KAYDET
        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        originalLocalScale = transform.localScale;
        originalLayer = gameObject.layer;

        col = GetComponent<Collider>();
    }

    // Elde kullanmaya baþladýðýnda burayý çaðýr (timer baþlasýn)
    public void StartReturnTimer()
    {
        StopAllCoroutines();
        StartCoroutine(ReturnAfterDelay());
    }

    private IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        // Eski parent'a dön (local uzayda)
        transform.SetParent(originalParent, false);

        // Eski local poz/rot/scale'i geri yükle
        transform.localPosition = originalLocalPos;
        transform.localRotation = originalLocalRot;
        transform.localScale = originalLocalScale;

        // Layer'ý da MASAdaki haline getir (ör: "Item")
        gameObject.layer = originalLayer;

        // Collider kapandýysa aç
        if (col != null)
            col.enabled = true;

        Debug.Log("Silah masaya geri döndü ve tekrar týklanabilir olmalý.");
    }
}
