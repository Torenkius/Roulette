using UnityEngine;

public class Gun : ClickableItem
{
    [Header("Damage Settings")]
    public int baseDamage = 20;
    public int currentDamage;

    private void Awake()
    {
        currentDamage = baseDamage;
    }

    public override void OnClicked(ShooterType type)
    {
        GunController gun = this.GetComponent<GunController>();
        gun.Fire(type, out currentDamage);
         player.TakeDamage(currentDamage);
        Debug.Log("Damage Verildi  "+currentDamage);
    }

   
}
