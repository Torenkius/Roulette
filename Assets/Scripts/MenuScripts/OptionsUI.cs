using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuRoot;
    [SerializeField] private GameObject optionsPanel;

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Dropdown displayDropdown;

    private const string VolumeKey = "opt_volume";
    private const string DisplayKey = "opt_display"; // 0 = Windowed, 1 = Fullscreen

    void Start()
    {
        if (optionsPanel) optionsPanel.SetActive(false);

        // Dropdown boşsa seçenekleri ekle
        if (displayDropdown != null)
        {
            if (displayDropdown.options.Count == 0)
            {
                displayDropdown.options.Add(new TMP_Dropdown.OptionData("FullScreen"));
                displayDropdown.options.Add(new TMP_Dropdown.OptionData("Windowed"));
            }

            // 🔹 ÖNEMLİ: Event'i buradan bağlıyoruz
            displayDropdown.onValueChanged.RemoveAllListeners();
            displayDropdown.onValueChanged.AddListener(OnDisplayChanged);
        }

        // Kayıtlı ayarları yükle
        float vol = PlayerPrefs.GetFloat(VolumeKey, 0.7f);
        int disp = PlayerPrefs.GetInt(DisplayKey, 0); // default: Windowed

        // UI'ya bas
        if (volumeSlider) volumeSlider.value = vol;

        if (displayDropdown)
        {
            // 0–1 aralığına sıkıştır
            disp = Mathf.Clamp(disp, 0, displayDropdown.options.Count - 1);

            displayDropdown.value = disp;
            displayDropdown.RefreshShownValue();
        }

        // Uygula
        ApplyVolume(vol);
        ApplyDisplay(disp);
    }

    public void OpenOptions()
    {
        if (mainMenuRoot) mainMenuRoot.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel) optionsPanel.SetActive(false);
        if (mainMenuRoot) mainMenuRoot.SetActive(true);
    }

    public void OnVolumeChanged(float v)
    {
        ApplyVolume(v);
        PlayerPrefs.SetFloat(VolumeKey, v);
        PlayerPrefs.Save();
    }

    public void OnDisplayChanged(int idx)
    {
        Debug.Log("OnDisplayChanged çağrıldı, idx = " + idx);

        ApplyDisplay(idx);
        PlayerPrefs.SetInt(DisplayKey, idx);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    private void ApplyDisplay(int idx)
    {
        idx = Mathf.Clamp(idx, 0, 1);

        // 1 = Fullscreen, 0 = Windowed
        if (idx == 0)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }

        Debug.Log($"ApplyDisplay -> idx={idx}, mode={Screen.fullScreenMode}, full={Screen.fullScreen}");
    }
}
