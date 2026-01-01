using UnityEngine;

public class HealItem : ClickableItem
{
    public int healAmount = 1;
    public override void OnClicked(ShooterType t)
    {
        
        Debug.Log(itemName + " týklandý! Can veriyor: " + healAmount);
        if (t == ShooterType.Player)
        {
            this.gameObject.transform.parent = player.healholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isDrink");
            player.Heal(healAmount);
        }
        else
        {
           
            enemy.animator.SetTrigger("isDrink");
            enemy.Heal(healAmount);
        }
        Destroy(gameObject,5f); 

    }
}
