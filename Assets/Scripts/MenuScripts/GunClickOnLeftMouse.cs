using UnityEngine;
using UnityEngine.InputSystem;

public class GunClickOnLeftMouse : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip gunClip;

    [Header("Optional")]
    [SerializeField] private bool ignoreWhenPointerOverUI = true;
    [SerializeField] private float minInterval = 0.08f;

    private float lastTime;

    void Awake()
    {
        if (source == null) source = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.unscaledTime - lastTime < minInterval) return;

            if (ignoreWhenPointerOverUI &&
                UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            lastTime = Time.unscaledTime;
            if (source != null && gunClip != null)
                source.PlayOneShot(gunClip);
        }
    }
}
