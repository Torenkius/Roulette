using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GunController : MonoBehaviour
{
    private Queue<ShellType> magazine = new Queue<ShellType>();
    [Header("Player &6 Enemy References")]
    private PlayerCharacter player;
    [Header("Gun Settings")]
    public Transform firePoint;   // Ateş efekti veya mermi çıkış noktası

    [Header("Damage Settings")]
    public int baseDamage = 1;            // Normal hasar
    public bool damageMultiplierActive;   // Zehirli mermi vs. açarsa true

    // Son kimin ateş ettiği bilgisini tutmak istersen:
    public ShooterType lastShooter { get; private set; }
    private void Awake()
    {
        player=GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
    }

    // Şarjörü doldurur ve karıştırır
    public void LoadMagazine(int liveCount, int blankCount)
    {
        magazine.Clear();
        List<ShellType> shells = new List<ShellType>();

        for (int i = 0; i < liveCount; i++) shells.Add(ShellType.Live);
        for (int i = 0; i < blankCount; i++) shells.Add(ShellType.Blank);

        // Shuffle (Karıştırma)
        System.Random rng = new System.Random();
        shells = shells.OrderBy(a => rng.Next()).ToList();

        foreach (var shell in shells)
        {
            magazine.Enqueue(shell);
        }

        Debug.Log($"Magazine Loaded: {liveCount} Live, {blankCount} Blank");
    }

    /// <summary>
    /// Ateş etme fonksiyonu.
    /// - Hangi mermi türü sıkıldığını döner.
    /// - out parametresi ile bu şutun vereceği hasarı döndürür.
    /// - shooter ile Player mı Enemy mi sıktı bilinir.
    /// </summary>
    public ShellType Fire(ShooterType shooter, out int damage)
    {
        damage = 0;

        if (magazine.Count == 0)
        {
            Debug.LogError("Magazine is empty!");
            return ShellType.Blank; // Hata durumunda boş kabul edelim
        }

        lastShooter = shooter;

        ShellType currentShell = magazine.Dequeue();

        // Sadece canlı mermi hasar versin
        if (currentShell == ShellType.Live)
        {
            damage = baseDamage;

            // Damage multiplier aktifse hasarı 2 katına çıkar
            if (damageMultiplierActive)
            {
                damage *= 2;
                // Eğer sadece bir sonraki atışa etki etsin istiyorsan:
                damageMultiplierActive = false;
            }
        }
        if(shooter == ShooterType.Player)
        {
            //Enemy objesi alınıcak ai geldikten sonra takedamage fonksiyonu çağırılacak
        }
        else
        {
            player.TakeDamage(damage);
        }

        Debug.Log($"Fired: {currentShell} | Shooter: {shooter} | Damage: {damage}");

        // Burada animasyon veya ses tetiklenebilir
        return currentShell;
    }

    public int GetRemainingShells()
    {
        return magazine.Count;
    }
}
