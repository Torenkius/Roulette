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
    private const string DisplayKey = "opt_display"; // 0 windowed, 1 fullscreen

    void Start()
    {
        if (optionsPanel) optionsPanel.SetActive(false);

        // Dropdown opsiyonları boşsa doldur
        if (displayDropdown != null && displayDropdown.options.Count == 0)
        {
            displayDropdown.options.Add(new TMP_Dropdown.OptionData("Windowed"));
            displayDropdown.options.Add(new TMP_Dropdown.OptionData("Fullscreen"));
        }

        // Kayıtlı ayarları yükle
        float vol = PlayerPrefs.GetFloat(VolumeKey, 0.7f);
        int disp = PlayerPrefs.GetInt(DisplayKey, 1);

        // UI'ya bas
        if (volumeSlider) volumeSlider.value = vol;
        if (displayDropdown)
        {
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
        // 1 = Fullscreen, 0 = Windowed
        Screen.fullScreenMode = (idx == 1) ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
}
