using Unity.VisualScripting;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth;
    public bool isDead = false;
    [Header("Click Settings")]
    public float clickDistance = 10f;          
    private LayerMask interactableLayer;
    [Header("Damage Settings")]
    public int damageX = 1;
    [Header("Shield")]
    public bool shieldActive = false;
    public bool canInteract = true;
    [Header("Animation&Sound")]
    public Animator animator;


    private Camera cam;

    void Awake()
    {
        animator= GetComponent<Animator>();
        interactableLayer =LayerMask.GetMask("Item");
        cam = Camera.main;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (!canInteract) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryClickItem();
        }
    }

    void TryClickItem() 
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, clickDistance, interactableLayer))
        {
           var item = hit.collider.GetComponent<ClickableItem>();

            if (item != null)
            {
                item.OnClicked(ShooterType.Player); 
            }
        }
    }
    public void SetUp()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(int amount)
    {
        if (shieldActive)
        {
            Debug.Log("Shield gelen hasarý engelledi!");
            shieldActive = false;
            return;
        }
        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);
        Debug.Log("Player damage aldý. Can: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        Debug.Log("Player iyileþti. Can: " + currentHealth);
    }
    void Die()
    {
        Debug.Log("Player öldü!");
    }
    public void setUp()
    {
               currentHealth = maxHealth;
    }
}
