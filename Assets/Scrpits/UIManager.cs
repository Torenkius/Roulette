using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro kullandığımızı varsayalım

public class UIManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI logText;
    public TextMeshProUGUI shellInfoText; // Debug amaçlı
    public Button shootSelfButton;
    public Button shootOpponentButton;

    [Header("Stats UI")]
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI opponentHealthText;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;
    public Button restartButton;

    private void Start()
    {
        if(gameOverPanel) gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(bool playerWon)
    {
        if(gameOverPanel) gameOverPanel.SetActive(true);
        if(winnerText) 
        {
            winnerText.text = playerWon ? "YOU SURVIVED" : "YOU DIED";
            winnerText.color = playerWon ? Color.green : Color.red;
        }
        
        // Oyun bitince butonları kapat
        EnablePlayerControls(false);
    }

    public void UpdateTurnIndicator(string turnOwner)
    {
        if(turnText) turnText.text = $"Turn: {turnOwner}";
    }

    public void LogMessage(string message)
    {
        if(logText) logText.text = message;
        Debug.Log("[UI] " + message);
    }

    public void UpdateShellCounts(int live, int blank)
    {
        // Normalde oyuncu bunu tam bilmez ama debug için ekrana yazalım
        // Veya "known shells" mantığı eklenebilir.
        if(shellInfoText) shellInfoText.text = $"Live: {live} | Blank: {blank}";
    }

    public void EnablePlayerControls(bool enable)
    {
        if (shootSelfButton) shootSelfButton.interactable = enable;
        if (shootOpponentButton) shootOpponentButton.interactable = enable;
    }

    public void UpdateHealthUI(int playerHP, int opponentHP)
    {
        if (playerHealthText != null) playerHealthText.text = $"Player HP: {playerHP}";
        if (opponentHealthText != null) opponentHealthText.text = $"Opponent HP: {opponentHP}";
    }
}
