using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;

// ShooterType'? GunController'da tan?mlad?ysan oradan kullan?yoruz:
// public enum ShooterType { Player, Enemy }
// public enum ShellType { Live, Blank }

public class AIController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;          // Max can
    public int currentHealth;          // Mevcut can
    public bool isDead = false;        // ?l? m??
    public bool shieldActive = false;  // Kalkan aktif mi?

    [Header("References")]
    public GameManager gameManager;         // Oyun y?neticisi
    public PlayerCharacter player;          // Oyuncu referans?
    public GunController gun;               // Ortak silah
    public List<ClickableItem> myItems;     // AI item listesi
    public Animator animator;               // AI animat?r?
    public Transform HealHolder;            // Heal item konumu
    public Transform itemHolder; // Di?er item konumu

    [Header("AI Simple Improvements")]
    public bool useSmartItemPriority = true;    // Ak?ll? item se?imi a??k m??
    [Range(1, 5)] public int healWhenHpAtOrBelow = 2;      // HP <= X iken heal kullan
    [Range(1, 5)] public int useFreezeWhenHpAtOrBelow = 2; // HP <= X iken freeze kullan

    [Header("AI Settings")]
    public float thinkDelay = 0.75f;       // D???nme beklemesi
    [Range(0f, 1f)]
    public float selfShotChance = 0.2f;    // Kendine s?kma ?ans?

    [Header("Health UI")]
    public GameObject healthIconPrefab;    // Can ikonu prefab
    public Transform healthContainer;      // ?konlar?n parent objesi
    private List<GameObject> activeIcons = new List<GameObject>(); // ?kon listesi

    private void Awake()
    {
        // AI'nin can ikonlar?n? (UI) ba?lat?r: maxHealth kadar ikon ?retip listeye ekler
        InitializeHealthUI();

        // Animator component'ini al?r ve ba?lang??ta "bekleme" animasyon trigger'?n? verir
        animator = GetComponent<Animator>();
        animator.SetTrigger("isWait");

        // AI'nin mevcut can?n? maksimum cana e?itler (oyun ba?? full HP)
        currentHealth = maxHealth;

        // E?er Inspector'dan GunController atanmad?ysa sahnede bulup otomatik ba?lar
        if (gun == null)
            gun = FindObjectOfType<GunController>();

        // E?er Inspector'dan PlayerCharacter atanmad?ysa sahnede bulup otomatik ba?lar
        if (player == null)
            player = FindObjectOfType<PlayerCharacter>();

        // myItems listesi Inspector'dan verilmediyse null olmas?n diye bo? liste olu?turur
        if (myItems == null)
            myItems = new List<ClickableItem>();
    }

    public void StartTurn()
    {
        // E?er AI ?lmediyse, tur davran???n? coroutine olarak ba?lat?r
        // (Coroutine kullanmam?z?n sebebi: bekleme/d???nme s?resi ekleyebilmek)
        if (!isDead)
            StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        // AI tur ba??nda bir s?re bekler: "d???n?yor" hissi verir
        yield return new WaitForSeconds(thinkDelay);

        // 1) ?nce elinde item varsa bir tane kullanmay? dener
        // (UseItemIfAny i?inde item se?me mant??? var)
        UseItemIfAny();

        // Item kulland?ktan sonra k???k bir ekstra bekleme: animasyon/ak?? daha do?al olur
        yield return new WaitForSeconds(0.25f);

        // 2) Sonra ate? eder (kendine mi oyuncuya m? s?kaca??n? Shoot belirler)
        Shoot();
    }

    private void UseItemIfAny()
    {
        // Item yoksa hi?bir ?ey yapma
        if (myItems == null || myItems.Count == 0)
        {
            Debug.Log("AI item yok, item kullanmad?.");
            return;
        }

        // Ak?ll? ?ncelik a??k ise: s?rayla kontrol ederek en mant?kl? item'i se?
        if (useSmartItemPriority)
        {
            // 1) Can d???kse ?nce heal dene
            if (currentHealth <= healWhenHpAtOrBelow)
            {
                HealItem healItem = myItems.OfType<HealItem>().FirstOrDefault();
                if (healItem != null)
                {
                    Debug.Log("AI can? d???k, HealItem kullan?yor.");
                    UseItem(healItem);
                    return;
                }
            }

            // 2) Can d???kse Freeze (tur engelleme) dene
            if (currentHealth <= useFreezeWhenHpAtOrBelow)
            {
                EnemyTurnBlockItem freeze = myItems.OfType<EnemyTurnBlockItem>().FirstOrDefault();
                if (freeze != null)
                {
                    Debug.Log("AI can? d???k, EnemyTurnBlockItem (Freeze) kullan?yor.");
                    UseItem(freeze);
                    return;
                }
            }

            // 3) Damage boost varsa kullan (daha fazla vurma ?ans?)
            DamageItem dmg = myItems.OfType<DamageItem>().FirstOrDefault();
            if (dmg != null)
            {
                Debug.Log("AI DamageItem kullanýyor (damage boost).");
                UseItem(dmg);
                return;
            }

            // 4) Steal varsa en sona b?rak?p kullan
            StealEnemyItem steal = myItems.OfType<StealEnemyItem>().FirstOrDefault();
            if (steal != null)
            {
                Debug.Log("AI StealEnemyItem kullan?yor (item ?alma).");
                UseItem(steal);
                return;
            }

            // Ak?ll? ?ncelik a??k ama uygun item bulunamad?ysa random'a d??ecek
            Debug.Log("AI ak?ll? ?ncelikte uygun item bulamad?, random se?ecek.");
        }

        // ===== Eski davran?? (fallback): RANDOM =====
        int r = Random.Range(0, myItems.Count);
        Debug.Log("AI random item se?ti: " + myItems[r].GetType().Name);
        UseItem(myItems[r]);
    }

    void Shoot()
    {
        if (gun == null || isDead)
            return;

        // Basit mant?k: ?o?unlukla player'a, bazen kendine s?k
        bool shootPlayer = Random.value > selfShotChance;

        if (shootPlayer)
        {
            // Rakibe s?k
            gun.Fire(ShooterType.Enemy, false);

            // Hamleyi yazd?r
            Debug.Log("AI oyuncuya s?k?yor. Damage: ");

            // Rakibe s?k?nca blank bile gelse tur kar??ya ge?sin (normal EndTurn)
        }
        else
        {
            // Kendine s?k
            gun.Fire(ShooterType.Enemy, true);

            // Hamleyi yazd?r
            Debug.Log("AI kendine s?k?yor. Damage: ");
        }

        // Turn her zaman tek noktadan bitsin (?ifte EndTurn bug?n? engeller)
        gameManager.EndTurn();
    }

    public void Heal(int amount)
    {
        // AI ?ld?yse iyile?emez, direkt ??k
        if (isDead) return;

        // Can? artt?r
        currentHealth += amount;

        // Max can? ge?mesin diye s?n?rla
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Konsola bilgi yazd?r (test/debug i?in)
        Debug.Log("AI iyile?ti. Can: " + currentHealth);

        // E?er AudioManager sahnede varsa iyile?me sesini ?al
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_healing_sound();
        }

        // UI'daki can ikonlar?n? g?ncelle (ka? ikon a??k kalacak vs.)
        UpdateHealthUI();
    }

    public void setUp()
    {
        // AI'yi ma? ba??/yeniden ba?latma i?in s?f?rlar:
        // - Can? fulle
        // - ?l?m durumunu kald?r
        // - Kalkan? kapat
        currentHealth = maxHealth;
        isDead = false;
        shieldActive = false;
    }

    void Die()
    {

        // AI'nin ?ld???n? i?aretler
        isDead = true;
        animator.SetTrigger("isDead");

        // Konsola bilgi yazd?r (debug/test)
        Debug.Log("AI ?ld?!");

        // ?stersen burada:
        // - ?l?m animasyonu oynatabilir
        // - collider/AI kontrol?n? kapatabilir
        // - karakteri devre d??? b?rakabilirsin

        // E?er AudioManager sahnede varsa ?l?m sesini ?al
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_die_sound();
        }
    }

    void InitializeHealthUI()
    {
        // Oyun ba??nda AI'nin maxHealth de?erine g?re UI'da can ikonlar?n? olu?turur
        // ?rn: maxHealth = 5 ise 5 tane ikon ?retir
        for (int i = 0; i < maxHealth; i++)
        {
            // Prefab'? healthContainer alt?nda instantiate eder (Horizontal/Vertical Layout olabilir)
            GameObject newIcon = Instantiate(healthIconPrefab, healthContainer);

            // ?retilen ikonlar? listede tutar?z ki sonra a?/kapat yapabilelim
            activeIcons.Add(newIcon);
        }
    }

    public void UpdateHealthUI()
    {
        // currentHealth ka? ise o kadar ikonu a??k tutar, geri kalan?n? kapat?r
        // ?rn: currentHealth = 3 ise ilk 3 ikon a??k, di?erleri kapal? olur
        for (int i = 0; i < activeIcons.Count; i++)
        {
            activeIcons[i].SetActive(i < currentHealth);
        }
    }

    private ClickableItem FindFirstItem<T>() where T : ClickableItem
    {
        // myItems listesinde aranan tipe (T) ait ilk item'i bulur ve d?nd?r?r
        // Bulamazsa null d?ner
        for (int i = 0; i < myItems.Count; i++)
        {
            if (myItems[i] is T) return myItems[i];
        }
        return null;
    }

    private void UseItem(ClickableItem item)
    {
        // E?er item yoksa (null) hi?bir ?ey yapmadan ??k
        if (item == null) return;

        // Burada AI'nin hangi item'i kulland???n? yazd?rabiliriz (debug / ekranda olay takibi i?in)
        Debug.Log("AI item kulland?: " + item.GetType().Name);

        // Item'in kendi etkisini ?al??t?r?r:
        // ?rn: HealItem -> AI iyile?tirir
        //     DamageItem -> silah?n hasar?n? art?r?r
        //     EnemyTurnBlockItem -> tur dondurma a?ar
        item.OnClicked(ShooterType.Enemy);

        // Kullan?lan item'i AI'nin envanter listesinden kald?r?r (tekrar kullanamas?n)
        myItems.Remove(item);

        // Not: Item scriptlerinin ?o?u zaten kendini Destroy ediyor.
        // E?er etmeyen olursa, burada Destroy(item.gameObject) eklenebilir.
    }

    public void TakeDamage(int amount)
    {
        // E?er AI zaten ?ld?yse, tekrar hasar uygulamay?z
        if (isDead) return;

        // E?er kalkan a??ksa bu hasar? tamamen engeller
        // ve kalkan? kapat?r (tek kullan?ml?k gibi)
        if (shieldActive)
        {
            Debug.Log("AI shield hasar? engelledi!");
            shieldActive = false;
            return; // hasar almadan ??k
        }

        // Kalkan yoksa gelen hasar? can?ndan d??
        currentHealth -= amount;

        // Can 0'?n alt?na inmesin diye s?n?rla
        currentHealth = Mathf.Max(currentHealth, 0);

        // Konsola bilgi yazd?r (debug / olay takibi)
        Debug.Log("AI damage ald?. Can: " + currentHealth);

        // Can 0 veya daha az olduysa ?lme fonksiyonunu ?a??r
        if (currentHealth <= 0)
            Die();

        // UI'daki can ikonlar?n? g?ncelle
        UpdateHealthUI();
    }

}