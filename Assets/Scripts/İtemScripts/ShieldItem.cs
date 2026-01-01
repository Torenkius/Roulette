using UnityEngine;

public class ShieldItem : ClickableItem
{
    public override void OnClicked(ShooterType t)
    {
        Debug.Log(itemName + " týklandý! Shield AKTÝF.");

        // Zaten açýksa tekrar açmaya gerek yok ama istersen üst üste bindirebilirsin
        if (t == ShooterType.Enemy)
        {
            enemy.animator.SetTrigger("isTake");
            enemy.shieldActive = true;
        }
        if (t==ShooterType.Player)
        {
            this.gameObject.transform.parent = player.itemholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isTake");
            player.shieldActive = true;
        }


        // Bu item kullanýldý, masadan kalksýn
        Destroy(gameObject,2f);
    }
}
