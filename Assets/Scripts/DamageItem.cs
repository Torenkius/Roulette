using UnityEditor.TextCore.Text;
using UnityEngine;

public class DamageItem : ClickableItem
{
    public int damageAmountX = 2;

    public override void OnClicked( )
    {
        Debug.Log(itemName + " týklandý! Hasar veriyor: " + damageAmountX);
         GameObject gunObject = GameObject.FindGameObjectWithTag("Gun");

        if (gunObject != null)
        {
            Gun gun = gunObject.GetComponent<Gun>();

            if (gun != null)
            {
                gun.IncreaseDamageMultiplier(damageAmountX);
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
