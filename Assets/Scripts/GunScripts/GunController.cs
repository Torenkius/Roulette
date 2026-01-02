using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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
    public Transform enemyGunHolder;// Ate? efekti veya mermi ??k?? noktas?

    [Header("Damage Settings")]
    public int baseDamage = 1;            // Normal hasar
    public bool damageMultiplierActive;   // Zehirli mermi vs. a?arsa true

    // Son kimin ate? etti?i bilgisini tutmak istersen:
    public ShooterType lastShooter { get; private set; }

    public ShellType LastFiredShell { get; private set; } //sonradan eklendi

    private void Awake()
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

        Debug.Log($"Magazine Loaded: {liveCount} Live, {blankCount} Blank");
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
            GunT.transform.parent = playerGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isGun");
            player.TakeDamage(damage);
            if (currentShell == ShellType.Blank) 
            { 
                gameManager.isRoundFreeze = true;
            }
        }
        else if (shooter == ShooterType.Enemy && isSelf)
        {
            GunT.transform.parent = enemyGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            enemy.animator.SetTrigger("isGun");
            if (currentShell == ShellType.Blank)
            {
                gameManager.isRoundFreeze = true;
            }

            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Player && !isSelf)
        {
            GunT.transform.parent = playerGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isGun");

            enemy.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Enemy && !isSelf)
        {
            GunT.transform.parent = enemyGunHolder;
            GunT.transform.localPosition = Vector3.zero;
            enemy.animator.SetTrigger("isGun");
            player.TakeDamage(damage);
        }
        else if (shooter == ShooterType.Enemy && isSelf)
        {
            // AI kendine s?k?yor: silah AI taraf?nda kals?n, AI animasyonu oynas?n
            GunT.transform.parent = enemyGunHolder;
            GunT.transform.localPosition = Vector3.zero;

            if (enemy != null && enemy.animator != null)
                enemy.animator.SetTrigger("isGun");

            // Hasar? AI kendisi al?r
            if (enemy != null)
                enemy.TakeDamage(damage);

            Debug.Log("AI kendine ate? etti. Damage: " + damage);
        }
        //sonradan eklendi ->
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