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
    public int maxHealth = 5;
    public int currentHealth;
    public bool isDead = false;
    public bool shieldActive = false;

    [Header("References")]
    public GameManager gameManager;
    public PlayerCharacter player;           // Ýnsan oyuncu
    public GunController gun;               // Ortak silah
    public List<ClickableItem> myItems;
    public Animator animator;
    public Transform HealHolder;
    public Transform itemHolder;
    // AI'nin kullanabileceði itemler

    [Header("AI Settings")]
    public float thinkDelay = 0.75f;        // Tur baþladýðýnda biraz beklesin
    [Range(0f, 1f)]
    public float selfShotChance = 0.2f;     // %20 kendine sýkma ihtimali (istersen deðiþ)

    [Header("Health UI")]
    public GameObject healthIconPrefab; // Ýçinde UI Image olan bir Prefab
    public Transform healthContainer;   // Tahtadaki Horizontal Layout Group objesi
    private List<GameObject> activeIcons = new List<GameObject>();

    private void Awake()
    {
        InitializeHealthUI();
        animator = GetComponent<Animator>();
        animator.SetTrigger("isWait");
        currentHealth = maxHealth;

        if (gun == null)
            gun = FindObjectOfType<GunController>();

        if (player == null)
            player = FindObjectOfType<PlayerCharacter>();

        if (myItems == null)
            myItems = new List<ClickableItem>();
    }

    // Dýþarýdan GameManager yerine bunu çaðýrýrsýn:
    // örn: turn geldiðinde ai.StartTurn();
    public void StartTurn()
    {
        if (!isDead)
            StartCoroutine(TurnRoutine());
    }

    private IEnumerator TurnRoutine()
    {
        // Biraz beklesin, “düþünüyor” gibi
        yield return new WaitForSeconds(thinkDelay);

        // 1) Varsa bir item kullansýn
        UseItemIfAny();

        yield return new WaitForSeconds(0.25f);

        // 2) Sonra ateþ etsin
        Shoot();
    }

    // ======================
    // Item kullanýmý
    // ======================
    void UseItemIfAny()
    {
        if (myItems == null || myItems.Count == 0)
            return;

        // Þimdilik: rastgele bir item seçip kullansýn
        int index = Random.Range(0, myItems.Count);
        var item = myItems[index];
        if(item.name=="HealItem")
        {
            item.transform.parent = HealHolder;
            item.transform.localPosition = Vector3.zero;

        }
        else
        {
            item.transform.parent = itemHolder;
            item.transform.localPosition = Vector3.zero;
        }
        

        if (item == null)
            return;

        // ÖNEMLÝ: AI item kullanýrken
        item.OnClicked(ShooterType.Enemy);

        // Tek kullanýmlýk gibi düþünüyorsan listeden çýkar
        myItems.RemoveAt(index);
    }

    // ======================
    // Ateþ etme
    // ======================
    void Shoot()
    {
        if (gun == null || isDead)
            return;
        // Basit mantýk: çoðunlukla player'a, bazen kendine sýk
        bool shootPlayer = Random.value > selfShotChance;

        if (shootPlayer)
        {
            gun.Fire(ShooterType.Enemy, false);
            Debug.Log("AI oyuncuya sýkýyor. Damage: ");
        }
        else
        {
            gun.Fire(ShooterType.Enemy, true);
            Debug.Log("AI kendine sýkýyor! Damage: ");
            
        }
        gameManager.EndTurn();
    }

    // ======================
    // Health fonksiyonlarý
    // ======================
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log("AI iyileþti. Can: " + currentHealth);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_healing_sound();
        }
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (shieldActive)
        {
            Debug.Log("AI shield hasarý engelledi!");
            shieldActive = false;
            return;
        }

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("AI damage aldý. Can: " + currentHealth);
        if (currentHealth <= 0)
            Die();

        UpdateHealthUI();
    }
    public void setUp()
    {
        currentHealth = maxHealth;
        isDead = false;
        shieldActive = false;
    }

    void Die()
    {
        isDead = true;
        Debug.Log("AI öldü!");
        // Buraya animasyon / disable vs. koyabilirsin
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play_die_sound();
        }
    }
    void InitializeHealthUI()
    {
        // Baþlangýçta maxHealth kadar ikon oluþtur ve listeye ekle
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newIcon = Instantiate(healthIconPrefab, healthContainer);
            activeIcons.Add(newIcon);
        }
    }

    public void UpdateHealthUI()
    {
        // Can azaldýkça ikonlarý kapatýr, arttýkça açar
        for (int i = 0; i < activeIcons.Count; i++)
        {
            activeIcons[i].SetActive(i < currentHealth);
        }
    }
}
