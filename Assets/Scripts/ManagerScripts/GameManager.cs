using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public SceneLoader sc;
    public PlayerCharacter player;
    public AIController ai;
    public UIManager uiManager;
    public float delay=5f;
    bool gameOverHandled = false;// İstersen boş bırak, sadece varsa kullanılır

    public TurnOwner CurrentTurn { get; private set; }
    public bool IsGameOver { get;  set; }
    public bool isRoundFreeze = false;

    private void Start()
    {
        StartGame();
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_chest_sound();
        }
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_background_sound();
        }
    }

    public void StartGame()
    {
        IsGameOver = false;

        if (player != null) player.SetUp();
        if (ai != null) ai.setUp();

        // İstersen ilk turu random da yapabilirsin
        CurrentTurn = TurnOwner.Player;
        UpdateTurnUI();

        StartTurn();
    }

    /// <summary>
    /// Sıra başladığında ne olacağı:
    /// - Player sırasıysa: Hiçbir şey yapma (sadece UI güncelle)
    /// - AI sırasıysa: ai.StartTurn() çağır
    /// </summary>
    private void StartTurn()
    {
        if (IsGameOver) return;

        UpdateTurnUI();
        isRoundFreeze=false;
        if (CurrentTurn == TurnOwner.AI && ai != null)
        {
            ai.StartTurn();
            player.canInteract = false;// AI kendi turunu kendi yönetecek
        }
        else if (CurrentTurn == TurnOwner.Player && player != null)
        {
            player.canInteract = true; // Player kendi turunu kendi yönetecek
        }
        // Player sırasıysa GameManager hiçbir şey yapmıyor.
        // Player’ın kendi scriptleri/INPUT’u turu oynayıp bittiğinde EndTurn() çağırmalı.
    }

    /// <summary>
    /// Herhangi bir taraf turunu bitirdiğinde çağrılır.
    /// Player’ın atış scripti ya da AI, turu bitirdiğinde burayı çağıracak.
    /// </summary>
    public void EndTurn()
    {
        if (IsGameOver && !gameOverHandled)
        {
            gameOverHandled = true;
            if (player.currentHealth <= 0)
            {
                StartCoroutine(LoadEndScreenDieWithDelay(delay));
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play_lose_sound();
                }
            }
                
            else if(ai.currentHealth<=0)
            {
                StartCoroutine(LoadEndScreenWonWithDelay(delay));
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.Play_win_sound();
                }
            }
        }
        if (isRoundFreeze)
        {
            StartTurn();
        }
        else
        {
            CurrentTurn = (CurrentTurn == TurnOwner.Player) ? TurnOwner.AI : TurnOwner.Player;
            StartTurn();

        }
            
    }
    private IEnumerator LoadEndScreenDieWithDelay(float  delay)
    {
        // 5 saniye bekle
        yield return new WaitForSeconds(delay);

        sc.LoadScene("EndScreenYouDied");
    }

    private IEnumerator LoadEndScreenWonWithDelay(float delay)
    {
        // 5 saniye bekle
        yield return new WaitForSeconds(delay);

        sc.LoadScene("EndScreenYouWon");
    }

    private void UpdateTurnUI()
    {
        uiManager?.UpdateTurnIndicator(CurrentTurn.ToString());
        uiManager?.UpdateHealthUI(player.currentHealth, ai.currentHealth);
    }

    // İstersen PlayerCharacter / AIController ölümde buraları çağırabilir
    public void OnPlayerDied()
    {
        IsGameOver = true;
        uiManager?.LogMessage("YOU DIED. Game Over.");
        uiManager?.ShowGameOver(false);
    }

    public void OnAIDied()
    {
        IsGameOver = true;
        uiManager?.LogMessage("YOU WIN! Opponent Eliminated.");
        uiManager?.ShowGameOver(true);
    }

    public void RestartGame()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
