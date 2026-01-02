using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHoverSFX : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private float minInterval = 0.05f;

    private float lastTime;

    void Awake()
    {
        if (source == null)
            source = FindFirstObjectByType<AudioSource>(); // istersen Inspector'dan bağla
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

    // Klavye/gamepad ile seçilince de çalsın diye (opsiyonel ama güzel)
    public void OnSelect(BaseEventData eventData)
    {
        PlayHover();
    }

    private void PlayHover()
    {
        if (source == null || hoverClip == null) return;

        // Time.timeScale=0 iken de çalışsın diye unscaledTime kullanıyoruz
        if (Time.unscaledTime - lastTime < minInterval) return;
        lastTime = Time.unscaledTime;

        source.PlayOneShot(hoverClip);
    }
}
