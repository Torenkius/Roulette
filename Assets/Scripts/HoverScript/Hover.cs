using UnityEngine;
using TMPro;

public class HoverUp : MonoBehaviour
{
    [Header("Hover Ayarlarý")]
    public float hoverHeight = 0.2f;
    public float hoverSpeed = 10f;

    [Header("Ýsim Etiketi")]
    public TextMeshPro nameLabel;

    private Vector3 baseLocalPos;
    private bool isHovering = false;
    private Camera mainCam;

    void Awake()
    {
        baseLocalPos = transform.localPosition;

        if (nameLabel == null)
            nameLabel = GetComponentInChildren<TextMeshPro>(true);

        var clickable = GetComponent<ClickableItem>();
        if (clickable != null && nameLabel != null)
        {
            nameLabel.gameObject.SetActive(false);
        }

        mainCam = Camera.main;
    }

    void Update()
    {
        // Hover hareketi
        Vector3 targetPos = baseLocalPos;
        if (isHovering)
            targetPos = baseLocalPos + Vector3.up * hoverHeight;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * hoverSpeed
        );

        // LABEL'I KAMERAYA DÖNDÜR
        if (nameLabel != null && mainCam != null && nameLabel.gameObject.activeSelf)
        {
            // Tam kameraya baksýn:
            nameLabel.transform.rotation = Quaternion.LookRotation(
                nameLabel.transform.position - mainCam.transform.position
            );

            // Sadece Y ekseninde dönsün istersen (yukarý-aþaðý eðilmesin):
            // Vector3 dir = mainCam.transform.position - nameLabel.transform.position;
            // dir.y = 0;
            // nameLabel.transform.rotation = Quaternion.LookRotation(-dir);
        }
    }

    void OnMouseEnter()
    {
        isHovering = true;
        if (nameLabel != null)
            nameLabel.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        isHovering = false;
        if (nameLabel != null)
            nameLabel.gameObject.SetActive(false);
    }
}
