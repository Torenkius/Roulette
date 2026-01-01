using UnityEngine;

public class ShieldItem : ClickableItem
{
    public override void OnClicked(ShooterType t)
    {
        Debug.Log(itemName + " týklandý! Shield AKTÝF.");

        // Zaten açýksa tekrar açmaya gerek yok ama istersen üst üste bindirebilirsin
        if (t == ShooterType.Enemy)
        {
            enemy.shieldActive = true;
        }
        if (t==ShooterType.Player)
        {
            player.shieldActive = true;
        }

        // Bu item kullanýldý, masadan kalksýn
        Destroy(gameObject);
    }
}
