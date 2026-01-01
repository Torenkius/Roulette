using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GunController : MonoBehaviour
{
    public GameManager gameManager;
    private Queue<ShellType> magazine = new Queue<ShellType>();
    [Header("Player &6 Enemy References")]
    private PlayerCharacter player;
    private AIController enemy;
    [Header("Gun Settings")]
    public Transform firePoint;   // Ateþ efekti veya mermi çýkýþ noktasý

    [Header("Damage Settings")]
    public int baseDamage = 1;            // Normal hasar
    public bool damageMultiplierActive;   // Zehirli mermi vs. açarsa true

    // Son kimin ateþ ettiði bilgisini tutmak istersen:
    public ShooterType lastShooter { get; private set; }
    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
        enemy= GameObject.FindWithTag("Enemy").GetComponent<AIController>();
        LoadMagazine(5,0);
    }

    // Þarjörü doldurur ve karýþtýrýr
    public void LoadMagazine(int liveCount, int blankCount)
    {
        magazine.Clear();
        List<ShellType> shells = new List<ShellType>();

        for (int i = 0; i < liveCount; i++) shells.Add(ShellType.Live);
        for (int i = 0; i < blankCount; i++) shells.Add(ShellType.Blank);

        // Shuffle (Karýþtýrma)
        System.Random rng = new System.Random();
        shells = shells.OrderBy(a => rng.Next()).ToList();

        foreach (var shell in shells)
        {
            magazine.Enqueue(shell);
        }

        Debug.Log($"Magazine Loaded: {liveCount} Live, {blankCount} Blank");
    }

    /// <summary>
    /// Ateþ etme fonksiyonu.
    /// - Hangi mermi türü sýkýldýðýný döner.
    /// - out parametresi ile bu þutun vereceði hasarý döndürür.
    /// - shooter ile Player mý Enemy mi sýktý bilinir.
    /// </summary>
    public int Fire(ShooterType shooter, bool isSelf)
    {
        int damage = 0;

        if (magazine.Count == 0)
        {
            Debug.LogError("Magazine is empty!");
            return 0; // Hata durumunda boþ kabul edelim
        }

        lastShooter = shooter;

        ShellType currentShell = magazine.Dequeue();

        // Sadece canlý mermi hasar versin
        if (currentShell == ShellType.Live)
        {
            damage = baseDamage;

            // Damage multiplier aktifse hasarý 2 katýna çýkar
            if (damageMultiplierActive)
            {
                damage *= 2;
                // Eðer sadece bir sonraki atýþa etki etsin istiyorsan:
                damageMultiplierActive = false;
            }
        }
        if (shooter == ShooterType.Player&&isSelf)
        {
            player.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Enemy && isSelf)   
        {
            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Player && !isSelf)
        {
            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Enemy && !isSelf)
        {
            player.TakeDamage(damage);
        }
        

        Debug.Log($"Fired: {currentShell} | Shooter: {shooter} | Damage: {damage}");

        // Burada animasyon veya ses tetiklenebilir
        return damage;
    }

    public int GetRemainingShells()
    {
        return magazine.Count;
    }
}