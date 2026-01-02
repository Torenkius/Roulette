using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

// ShooterType'ý GunController'da tanýmladýysan oradan kullanýyoruz:
// public enum ShooterType { Player, Enemy }
// public enum ShellType { Live, Blank }

public class AIController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;          // Max can
    public int currentHealth;          // Mevcut can
    public bool isDead = false;        // Ölü mü?
    public bool shieldActive = false;  // Kalkan aktif mi?

    [Header("References")]
    public GameManager gameManager;         // Oyun yöneticisi
    public PlayerCharacter player;          // Oyuncu referansý
    public GunController gun;               // Ortak silah
    public List<ClickableItem> myItems;     // AI item listesi
    public Animator animator;               // AI animatörü
    public Transform HealHolder;            // Heal item konumu
    public Transform itemHolder;            // Diðer item konumu

    [Header("AI Simple Improvements")]
    public bool useSmartItemPriority = true;    // Akýllý item seçimi açýk mý?
    [Range(1, 5)] public int healWhenHpAtOrBelow = 2;      // HP <= X iken heal kullan
    [Range(1, 5)] public int useFreezeWhenHpAtOrBelow = 2; // HP <= X iken freeze kullan

    [Header("AI Settings")]
    public float thinkDelay = 0.75f;       // Düþünme beklemesi
    [Range(0f, 1f)]
    public float selfShotChance = 0.2f;    // Kendine sýkma þansý

    [Header("Health UI")]
    public GameObject healthIconPrefab;    // Can ikonu prefab
    public Transform healthContainer;      // Ýkonlarýn parent objesi
    private List<GameObject> activeIcons = new List<GameObject>(); // Ýkon listesi

    private void Awake()
    {
        // AI'nin can ikonlarýný (UI) baþlatýr: maxHealth kadar ikon üretip listeye ekler
        InitializeHealthUI();

        // Animator component'ini alýr ve baþlangýçta "bekleme" animasyon trigger'ýný verir
        animator = GetComponent<Animator>();
        animator.SetTrigger("isWait");

        // AI'nin mevcut canýný maksimum cana eþitler (oyun baþý full HP)
        currentHealth = maxHealth;

        // Eðer Inspector'dan GunController atanmadýysa sahnede bulup otomatik baðlar
        if (gun == null)
            gun = FindObjectOfType<GunController>();

        // Eðer Inspector'dan PlayerCharacter atanmadýysa sahnede bulup otomatik baðlar
        if (player == null)
            player = FindObjectOfType<PlayerCharacter>();

        // myItems listesi Inspector'dan verilmediyse null olmasýn diye boþ liste oluþturur
        if (myItems == null)
            myItems = new List<ClickableItem>();
    }

    public void StartTurn()
    {
        // Eðer AI ölmediyse, tur davranýþýný coroutine olarak baþlatýr
        // (Coroutine kullanmamýzýn sebebi: bekleme/düþünme süresi ekleyebilmek)
        if (!isDead)
            StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        // AI tur baþýnda bir süre bekler: "düþünüyor" hissi verir
        yield return new WaitForSeconds(thinkDelay);

        // 1) Önce elinde item varsa bir tane kullanmayý dener
        // (UseItemIfAny içinde item seçme mantýðý var)
        UseItemIfAny();

        // Item kullandýktan sonra küçük bir ekstra bekleme: animasyon/akýþ daha doðal olur
        yield return new WaitForSeconds(0.25f);

        // 2) Sonra ateþ eder (kendine mi oyuncuya mý sýkacaðýný Shoot belirler)
        Shoot();
    }

    private void UseItemIfAny()
    {
        // Item yoksa hiçbir þey yapma
        if (myItems == null || myItems.Count == 0)
        {
            Debug.Log("AI item yok, item kullanmadý.");
            return;
        }

        // Akýllý öncelik açýk ise: sýrayla kontrol ederek en mantýklý item'i seç
        if (useSmartItemPriority)
        {
            // 1) Can düþükse önce heal dene
            if (currentHealth <= healWhenHpAtOrBelow)
            {
                var heal = FindFirstItem<HealItem>();
                if (heal != null)
                {
                    Debug.Log("AI caný düþük, HealItem kullanýyor.");
                    UseItem(heal);
                    return;
                }
            }

            // 2) Can düþükse Freeze (tur engelleme) dene
            if (currentHealth <= useFreezeWhenHpAtOrBelow)
            {
                var freeze = FindFirstItem<EnemyTurnBlockItem>();
                if (freeze != null)
                {
                    Debug.Log("AI caný düþük, EnemyTurnBlockItem (Freeze) kullanýyor.");
                    UseItem(freeze);
                    return;
                }
            }

            // 3) Damage boost varsa kullan (daha fazla vurma þansý)
            var dmg = FindFirstItem<DamageItem>();
            if (dmg != null)
            {
                Debug.Log("AI DamageItem kullanýyor (damage boost).");
                UseItem(dmg);
                return;
            }

            // 4) Steal varsa en sona býrakýp kullan
            var steal = FindFirstItem<StealEnemyItem>();
            if (steal != null)
            {
                Debug.Log("AI StealEnemyItem kullanýyor (item çalma).");
                UseItem(steal);
                return;
            }

            // Akýllý öncelik açýk ama uygun item bulunamadýysa random'a düþecek
            Debug.Log("AI akýllý öncelikte uygun item bulamadý, random seçecek.");
        }

        // ===== Eski davranýþ (fallback): RANDOM =====
        int r = Random.Range(0, myItems.Count);
        Debug.Log("AI random item seçti: " + myItems[r].GetType().Name);
        UseItem(myItems[r]);
    }

    void Shoot()
    {
        if (gun == null || isDead)
            return;

        // Basit mantýk: çoðunlukla player'a, bazen kendine sýk
        bool shootPlayer = Random.value > selfShotChance;

        if (shootPlayer)
        {
            // Rakibe sýk
            gun.Fire(ShooterType.Enemy, false);

            // Hamleyi yazdýr
            Debug.Log("AI oyuncuya sýkýyor. Damage: ");

            // Rakibe sýkýnca blank bile gelse tur karþýya geçsin (normal EndTurn)
        }
        else
        {
            // Kendine sýk
            gun.Fire(ShooterType.Enemy, true);

            // Hamleyi yazdýr
            Debug.Log("AI kendine sýkýyor. Damage: ");

            // Eðer kendine sýkýp BLANK geldiyse tur sende kalsýn:
            if (gun.LastFiredShell == ShellType.Blank)
            {
                gameManager.isRoundFreeze = true; // tur deðiþmesin
                Debug.Log("AI blank yedi, tur onda kaldý.");
            }
            else
            {
                Debug.Log("AI live yedi, tur karþýya geçecek.");
            }
        }

        // Turn her zaman tek noktadan bitsin (çifte EndTurn bugýný engeller)
        gameManager.EndTurn();
    }

    public void Heal(int amount)
    {
        // AI öldüyse iyileþemez, direkt çýk
        if (isDead) return;

        // Caný arttýr
        currentHealth += amount;

        // Max caný geçmesin diye sýnýrla
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Konsola bilgi yazdýr (test/debug için)
        Debug.Log("AI iyileþti. Can: " + currentHealth);

        // Eðer AudioManager sahnede varsa iyileþme sesini çal
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_healing_sound();
        }

        // UI'daki can ikonlarýný güncelle (kaç ikon açýk kalacak vs.)
        UpdateHealthUI();
    }

    public void setUp()
    {
        // AI'yi maç baþý/yeniden baþlatma için sýfýrlar:
        // - Caný fulle
        // - Ölüm durumunu kaldýr
        // - Kalkaný kapat
        currentHealth = maxHealth;
        isDead = false;
        shieldActive = false;
    }

    void Die()
    {
        // AI'nin öldüðünü iþaretler
        isDead = true;

        // Konsola bilgi yazdýr (debug/test)
        Debug.Log("AI öldü!");

        // Ýstersen burada:
        // - ölüm animasyonu oynatabilir
        // - collider/AI kontrolünü kapatabilir
        // - karakteri devre dýþý býrakabilirsin

        // Eðer AudioManager sahnede varsa ölüm sesini çal
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_die_sound();
        }
    }

    void InitializeHealthUI()
    {
        // Oyun baþýnda AI'nin maxHealth deðerine göre UI'da can ikonlarýný oluþturur
        // Örn: maxHealth = 5 ise 5 tane ikon üretir
        for (int i = 0; i < maxHealth; i++)
        {
            // Prefab'ý healthContainer altýnda instantiate eder (Horizontal/Vertical Layout olabilir)
            GameObject newIcon = Instantiate(healthIconPrefab, healthContainer);

            // Üretilen ikonlarý listede tutarýz ki sonra aç/kapat yapabilelim
            activeIcons.Add(newIcon);
        }
    }

    public void UpdateHealthUI()
    {
        // currentHealth kaç ise o kadar ikonu açýk tutar, geri kalanýný kapatýr
        // Örn: currentHealth = 3 ise ilk 3 ikon açýk, diðerleri kapalý olur
        for (int i = 0; i < activeIcons.Count; i++)
        {
            activeIcons[i].SetActive(i < currentHealth);
        }
    }

    private ClickableItem FindFirstItem<T>() where T : ClickableItem
    {
        // myItems listesinde aranan tipe (T) ait ilk item'i bulur ve döndürür
        // Bulamazsa null döner
        for (int i = 0; i < myItems.Count; i++)
        {
            if (myItems[i] is T) return myItems[i];
        }
        return null;
    }

    private void UseItem(ClickableItem item)
    {
        // Eðer item yoksa (null) hiçbir þey yapmadan çýk
        if (item == null) return;

        // Burada AI'nin hangi item'i kullandýðýný yazdýrabiliriz (debug / ekranda olay takibi için)
        Debug.Log("AI item kullandý: " + item.GetType().Name);

        // Item'in kendi etkisini çalýþtýrýr:
        // Örn: HealItem -> AI iyileþtirir
        //     DamageItem -> silahýn hasarýný artýrýr
        //     EnemyTurnBlockItem -> tur dondurma açar
        item.OnClicked(ShooterType.Enemy);

        // Kullanýlan item'i AI'nin envanter listesinden kaldýrýr (tekrar kullanamasýn)
        myItems.Remove(item);

        // Not: Item scriptlerinin çoðu zaten kendini Destroy ediyor.
        // Eðer etmeyen olursa, burada Destroy(item.gameObject) eklenebilir.
    }

    public void TakeDamage(int amount)
    {
        // Eðer AI zaten öldüyse, tekrar hasar uygulamayýz
        if (isDead) return;

        // Eðer kalkan açýksa bu hasarý tamamen engeller
        // ve kalkaný kapatýr (tek kullanýmlýk gibi)
        if (shieldActive)
        {
            Debug.Log("AI shield hasarý engelledi!");
            shieldActive = false;
            return; // hasar almadan çýk
        }

        // Kalkan yoksa gelen hasarý canýndan düþ
        currentHealth -= amount;

        // Can 0'ýn altýna inmesin diye sýnýrla
        currentHealth = Mathf.Max(currentHealth, 0);

        // Konsola bilgi yazdýr (debug / olay takibi)
        Debug.Log("AI damage aldý. Can: " + currentHealth);

        // Can 0 veya daha az olduysa ölme fonksiyonunu çaðýr
        if (currentHealth <= 0)
            Die();

        // UI'daki can ikonlarýný güncelle
        UpdateHealthUI();
    }

}
