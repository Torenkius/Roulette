using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


public class GunController : MonoBehaviour
{
    public GunReturnToTable gunReturnToTable;
    public GameManager gameManager;
    private Queue<ShellType> magazine = new Queue<ShellType>();
    [Header("Player &6 Enemy References")]
    private PlayerCharacter player;
    private AIController enemy;
    [Header("Gun Settings")]
    public Transform firePoint;
    public Transform GunT;
    public Transform playerGunHolder;
    public Transform enemyGunHolder;
    public ShellVisualSpawner spawner;// Ate? efekti veya mermi ??k?? noktas?

    [Header("Damage Settings")]
    public int baseDamage = 1;            // Normal hasar
    public bool damageMultiplierActive;   // Zehirli mermi vs. a?arsa true

    // Son kimin ate? etti?i bilgisini tutmak istersen:
    public ShooterType lastShooter { get; private set; }

    public ShellType LastFiredShell { get; private set; } //sonradan eklendi

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
        enemy = GameObject.FindWithTag("Enemy").GetComponent<AIController>();
        LoadMagazine();
    }
    private void Update()
    {
        
    }

    // ?arj?r? doldurur ve kar??t?r?r
    public void LoadMagazine()
    {
        magazine.Clear();
        List<ShellType> shells = new List<ShellType>();
        System.Random rng = new System.Random();
        int liveCount;
        int blankCount;
        liveCount= rng.Next(1,4);
        blankCount = rng.Next(0, 3);// 1-3 aral??????nda canl? mermi

        for (int i = 0; i < liveCount; i++) shells.Add(ShellType.Live);
        for (int i = 0; i < blankCount; i++) shells.Add(ShellType.Blank);
      
        // Shuffle (Kar??t?rma)
        
        shells = shells.OrderBy(a => rng.Next()).ToList();

        foreach (var shell in shells)
        {
            magazine.Enqueue(shell);
        }
        spawner.SpawnShells(liveCount, blankCount);
        string str=string.Format(
    " Þarjörde {0} dolu {1} boþ mermi var",
    liveCount,
    blankCount
);

        HUDLog.Instance.ShowMessage(str);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_reload_sound();
        }
    }

    /// <summary>
    /// Ate? etme fonksiyonu.
    /// - Hangi mermi t?r? s?k?ld???n? d?ner.
    /// - out parametresi ile bu ?utun verece?i hasar? d?nd?r?r.
    /// - shooter ile Player m? Enemy mi s?kt? bilinir.
    /// </summary>
    public int Fire(ShooterType shooter, bool isSelf)
    {
        int damage = 0;
        lastShooter = shooter;

        ShellType currentShell = magazine.Dequeue();
        LastFiredShell = currentShell; //sonradan eklendi


        // Sadece canl? mermi hasar versin
        if (currentShell == ShellType.Live) 
        {
            damage = baseDamage;

            // Damage multiplier aktifse hasar? 2 kat?na ??kar
            if (damageMultiplierActive)
            {
                damage *= 2;
                // E?er sadece bir sonraki at??a etki etsin istiyorsan:
                damageMultiplierActive = false;
            }
        }
        if (shooter == ShooterType.Player && isSelf)
        {
            HUDLog.Instance.ShowMessage("Kendine Ateþ Ettin");
            GunT.transform.parent = playerGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            GunT.transform.rotation = playerGunHolder.rotation;
            GunT.transform.rotation = playerGunHolder.rotation * Quaternion.Euler(0f, 180f, 0f);
            player.animator.SetTrigger("isGun");
            player.TakeDamage(damage);
            if (currentShell == ShellType.Blank) 
            { 
                gameManager.isRoundFreeze = true;
            }
        }
        else if (shooter == ShooterType.Enemy && isSelf)
        {
            HUDLog.Instance.ShowMessage("Rakibin kendine  Ateþ Etti");
            GunT.transform.parent = enemyGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            GunT.transform.rotation = enemyGunHolder.rotation;
            GunT.transform.rotation = enemyGunHolder.rotation * Quaternion.Euler(0f, 180f, 0f);
            enemy.animator.SetTrigger("isGun");
            if (currentShell == ShellType.Blank)
            {
                gameManager.isRoundFreeze = true;
            }

            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Player && !isSelf)
        {
            HUDLog.Instance.ShowMessage("Rakibine Ateþ Ettin");
            GunT.transform.parent = playerGunHolder;
            GunT.transform.rotation= playerGunHolder.rotation;
            GunT.transform.localPosition= Vector3.zero;
            player.animator.SetTrigger("isGun");

            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Enemy && !isSelf)
        {
            HUDLog.Instance.ShowMessage("Rakibine sana  Ateþ Etti");
            GunT.transform.parent = enemyGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            GunT.transform.rotation = enemyGunHolder.rotation;
            enemy.animator.SetTrigger("isGun");
            player.TakeDamage(damage);
        }
        if (gunReturnToTable != null)
            gunReturnToTable.StartReturnTimer();
        else
            Debug.LogWarning("gunReturnToTable NULL! GunController Inspector'dan ata.");
        //buraya kadar <-

        Debug.Log($"Fired: {currentShell} | Shooter: {shooter} | Damage: {damage}");

        // Burada animasyon veya ses tetiklenebilir
        if (damage == 0)
        {
            damageMultiplierActive = false;
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_empty_gun_sound();
            }
        }
        else
        {
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_full_gun_sound();
            }
        }
        if (magazine.Count == 0)
        {
            LoadMagazine();

        }
        return damage;
    }

    public int GetRemainingShells()
    {
        return magazine.Count;
    }
}