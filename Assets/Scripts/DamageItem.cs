using UnityEngine;

public class DamageItem : ClickableItem
{
    public int damageAmount = 1;

    public override void OnClicked( )
    {
        Debug.Log(itemName + " týklandý! Hasar veriyor: " + damageAmount);
        player.damageX *= 2;
        Destroy(gameObject);
    }
}
