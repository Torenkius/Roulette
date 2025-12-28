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

    public void IncreaseDamageMultiplier(int multiplier)
    {
        currentDamage *= multiplier;
        Debug.Log("Silah hasarý arttý! Yeni damage: " + currentDamage);
    }

    public void ResetDamage()
    {
        currentDamage = baseDamage;
    }
    public override void OnClicked()
    {
        Debug.Log("Damage Verildi  "+currentDamage);
    }

   
}
