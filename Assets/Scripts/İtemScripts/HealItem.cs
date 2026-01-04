using UnityEngine;

public class HealItem : ClickableItem
{
    public int healAmount = 1;
    public override void OnClicked(ShooterType t)
    {
        
        Debug.Log(itemName + " týklandý! Can veriyor: " + healAmount);
        if (t == ShooterType.Player)
        {
            HUDLog.Instance.ShowMessage("Kendini iyileþtirdin +" + healAmount);
            this.gameObject.transform.parent = player.healholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isDrink");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_healing_sound();
            }
            player.Heal(healAmount);
        }
        else
        {
            HUDLog.Instance.ShowMessage("Rakibin kendini iyileþtirdi +" + healAmount);
            this.gameObject.transform.parent = enemy.HealHolder;
            this.gameObject.transform.localPosition = Vector3.zero;
            enemy.animator.SetTrigger("isDrink");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_healing_sound();
            }
            enemy.Heal(healAmount);
        }
        Destroy(gameObject,5f); 

    }
}
