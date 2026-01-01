using UnityEngine;

public class Gun : ClickableItem
{
    private GunController gun;
    public GameManager gameManager;
    private bool isArmed = false;              // Silah seçildi mi?
    private bool justArmedThisFrame = false;   // Ayný frame'de hemen ateþ etmesin
    private ShooterType currentShooter;        // Player mý, Enemy mi kullandý?

    private void Awake()
    {
        gun = GetComponent<GunController>();
        if (gun == null)
        {
            Debug.LogError("GunController bu objede bulunamadý!");
        }
    }

    public override void OnClicked(ShooterType type)
    {
        // Bu fonksiyon sadece silaha týkladýðýn anda ÇALIÞIYOR
        Debug.Log(itemName + " týklandý! Silah SEÇÝLDÝ, hedef bekleniyor.");

        currentShooter = type;
        isArmed = true;
        justArmedThisFrame = true;  // Bu frame içinde Update'de click okumayalým
    }

    private void Update()
    {
        if (!isArmed) return;

        // Ýlk frame'i atla ki, silaha týkladýðýn sol click hemen ateþ gibi sayýlmasýn
        if (justArmedThisFrame)
        {
            justArmedThisFrame = false;
            return;
        }

        // Hedef seçimi:
        // Sol týk: Enemy'ye
        if (Input.GetMouseButtonDown(0))
        {
            FireAt(false); // isSelf = false
        }
        // Sað týk: Kendine
        else if (Input.GetMouseButtonDown(1))
        {
            FireAt(true); // isSelf = true
        }
    }

    private void FireAt(bool isself)
    {
        if (gun == null)
        {
            Debug.LogError("GunController referansý yok, ateþ edemiyorum.");
            isArmed = false;
            return;
        }

        // Burada GunController.Fire'im nasýl tanýmlýysa ona göre çaðýr
        // ÖNCEKÝ mesajlarda sana þöyle önermiþtim:
        // public ShellType Fire(ShooterType shooter, out int damage);
        // Eðer sen bool alan bir versiyon yazdýysan ona göre deðiþtir.

        Debug.Log(isself ? "KENDÝNE ateþ edildi." : "DÜÞMANA ateþ edildi.");

        // ÖRNEK: Eðer senin Fire imzan þu ise: Fire(ShooterType shooter, bool isSelf)
        gun.Fire(currentShooter, isself);

        // Bu silah kullanýldý, tekrar hedef beklemesin
        isArmed = false;

        // Tur sistemi kullanýyorsan burada GameManager.EndTurn() çaðýrabilirsin:
         gameManager.EndTurn();
    }
}
