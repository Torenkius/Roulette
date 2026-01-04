using System.Collections;
using UnityEngine;
using TMPro;

public class HUDLog : MonoBehaviour
{
    public static HUDLog Instance;

    [Header("UI")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ? Root GameObject'i kalýcý yap (Canvas veya en tepedeki obje)
        DontDestroyOnLoad(transform.root.gameObject);

        if (messageText != null)
            messageText.text = "";
    }


    public void ShowMessage(string msg)
    {
        if (messageText == null)
        {
            Debug.LogWarning("HUDLog: messageText atanmamýþ!");
            return;
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowMessageRoutine(msg));
    }

    private IEnumerator ShowMessageRoutine(string msg)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = msg;

        yield return new WaitForSeconds(messageDuration);

        messageText.text = "";
        currentRoutine = null;
    }

    // Kolay kullanmak için statik helper (opsiyonel ama tavsiye)
    public static void Log(string msg)
    {
        if (Instance == null)
        {
            Debug.LogWarning("HUDLog.Instance yok, sahnede HUDLog var mý?");
            return;
        }

        Instance.ShowMessage(msg);
    }
}
