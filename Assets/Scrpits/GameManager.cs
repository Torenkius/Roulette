using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Components")]
    public GunController gun;
    public Participant player;
    public AIController ai;
    public UIManager uiManager; // UI Manager'ı sonra oluşturacağız

    [Header("Game Settings")]
    public int minShells = 4;
    public int maxShells = 7;

    // State Tracking
    private int currentLiveShells;
    private int currentBlankShells;
    private TurnOwner currentTurn;
    private bool isRoundOver = false;
    private bool isProcessingTurn = false;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        player.Setup();
        ai.Setup();
        StartRound();
    }

    public void StartRound()
    {
        isRoundOver = false;


        // Mermi sayılarını belirle (en az 1 dolu, en az 1 boş olsun)
        int totalShells = Random.Range(minShells, maxShells + 1);
        currentLiveShells = Random.Range(1, totalShells); // En az 1 dolu
        currentBlankShells = totalShells - currentLiveShells;


        // Eğer hepsi dolu olduysa 1 tane boş ekle (veya tam tersi)
        if (currentBlankShells == 0)
        {
            currentBlankShells = 1;
            totalShells++;
        }

        gun.LoadMagazine(currentLiveShells, currentBlankShells);


        uiManager.UpdateShellCounts(currentLiveShells, currentBlankShells);
        uiManager.UpdateHealthUI(player.currentHealth, ai.currentHealth);
        uiManager.LogMessage($"New Round! {currentLiveShells} Live, {currentBlankShells} Blank.");

        // İlk tur kimin? (Rastgele veya sabit)
        currentTurn = TurnOwner.Player;
        uiManager.UpdateTurnIndicator(currentTurn.ToString());

        StartTurn();
    }

    private void StartTurn()
    {
        if (player.isDead || ai.isDead) return;
        if (gun.GetRemainingShells() == 0)
        {
            StartRound(); // Mermi bittiyse yeni tur (round)
            return;
        }

        isProcessingTurn = false;
        uiManager.UpdateTurnIndicator(currentTurn.ToString());

        if (currentTurn == TurnOwner.AI)
        {
            StartCoroutine(ai.DecideMove(currentLiveShells, currentBlankShells, HandleAIMove));
        }
        else
        {
            // Oyuncu girişi bekleniyor (UI butonları aktif edilecek)
            uiManager.EnablePlayerControls(true);
        }
    }

    // Oyuncu Butonlarından Çağrılacak
    public void OnPlayerShootSelf()
    {
        if (currentTurn != TurnOwner.Player || isProcessingTurn) return;
        uiManager.EnablePlayerControls(false);
        ProcessShot(true);
    }

    public void OnPlayerShootOpponent()
    {
        if (currentTurn != TurnOwner.Player || isProcessingTurn) return;
        uiManager.EnablePlayerControls(false);
        ProcessShot(false);
    }

    private void HandleAIMove(AIController.AIMove move)
    {
        bool isSelf = (move == AIController.AIMove.ShootSelf);
        ProcessShot(isSelf);
    }

    private void ProcessShot(bool isSelfTarget)
    {
        isProcessingTurn = true;
        ShellType filledShell = gun.Fire();

        // İstatistiki güncelleme (AI'ın ne kaldığını bilmesi için)
        if (filledShell == ShellType.Live) currentLiveShells--;
        else currentBlankShells--;

        // Görsel güncelleme (Gizli bilgi olabilir ama şimdilik gösterelim)
        uiManager.UpdateShellCounts(currentLiveShells, currentBlankShells);
        uiManager.UpdateHealthUI(player.currentHealth, ai.currentHealth);

        Participant shooter = (currentTurn == TurnOwner.Player) ? (Participant)player : (Participant)ai;
        Participant opponent = (currentTurn == TurnOwner.Player) ? (Participant)ai : (Participant)player;
        Participant target = isSelfTarget ? shooter : opponent;

        string actionLog = $"{shooter.participantName} shot {(isSelfTarget ? "Subject Name Here" : "Opponent")}";

        if (filledShell == ShellType.Live)
        {
            // DOLU MERMİ: Hasar ver
            Debug.Log("BANG! Live Round.");
            uiManager.LogMessage(actionLog + " -> BANG! (Damage)");
            target.TakeDamage(1);

            // Eğer kendimize sıktıysak bile dolu mermide sıra geçer.
            SwitchTurn();
        }
        else
        {
            // BOŞ MERMİ: Klik sesi
            Debug.Log("Click. Blank Round.");
            uiManager.LogMessage(actionLog + " -> Click. (Blank)");

            // Kural: Kendine boş sıkarsan sıra sende kalır.
            if (isSelfTarget)
            {
                Debug.Log("Extra Turn!");
                uiManager.LogMessage("Extra Turn!");
                // Sıra değişmez, tekrar StartTurn
                Invoke(nameof(StartTurn), 1.5f);
            }
            else
            {
                // Rakibe boş sıktın -> Sıra geçer
                SwitchTurn();
            }
        }

        CheckGameEnd();
    }

    private void SwitchTurn()
    {
        currentTurn = (currentTurn == TurnOwner.Player) ? TurnOwner.AI : TurnOwner.Player;
        Invoke(nameof(StartTurn), 2.0f); // Biraz bekleme süresi
    }

    private void CheckGameEnd()
    {
        if (player.isDead)
        {
            uiManager.LogMessage("YOU DIED. Game Over.");
            uiManager.ShowGameOver(false);
            isRoundOver = true;
        }
        else if (ai.isDead)
        {
            uiManager.LogMessage("YOU WIN! Opponent Eliminated.");
            uiManager.ShowGameOver(true);
            isRoundOver = true;
        }
    }

    public void RestartGame()
    {
        // Sahneyi yeniden yüklemek en temiz yöntemdir
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
