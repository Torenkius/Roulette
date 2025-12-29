using UnityEngine;

public class HealItem : ClickableItem
{
    public int healAmount = 1;
    public override void OnClicked(ShooterType t)
    {
        
        Debug.Log(itemName + " týklandý! Can veriyor: " + healAmount);
        if (t == ShooterType.Player)
        {
            player.Heal(healAmount);
        }
        else
        {
           // enemy.Heal(healAmount);
        }
            Destroy(gameObject);
    }
}
