using UnityEngine;

public class HealItem : ClickableItem
{
    public int healAmount = 1;
    public override void OnClicked()
    {
        
        Debug.Log(itemName + " týklandý! Can veriyor: " + healAmount);
        player.Heal(healAmount);
        Destroy(gameObject);
    }
}
