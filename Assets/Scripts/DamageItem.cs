using UnityEditor.TextCore.Text;
using UnityEngine;

public class DamageItem : ClickableItem
{
    public int damageAmountX = 2;

    public override void OnClicked(ShooterType t)
    {
        Debug.Log(itemName + " týklandý! Hasar veriyor: " + damageAmountX);
         GameObject gunObject = GameObject.FindGameObjectWithTag("Gun");

        if (gunObject != null)
        {
            GunController gun = gunObject.GetComponent<GunController>();

            if (gun != null)
            {
                gun.damageMultiplierActive=true;
            }
            else
            {
                Debug.LogWarning("Gun scripti bulunamadý!");
            }
        }
        else
        {
            Debug.LogWarning("Gun tag'li obje bulunamadý!");
        }
        Destroy(gameObject);
    }
}
