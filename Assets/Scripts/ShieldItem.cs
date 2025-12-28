using UnityEngine;

public class ShieldItem : ClickableItem
{
    public override void OnClicked()
    {
        Debug.Log(itemName + " týklandý! Shield AKTÝF.");

        // Zaten açýksa tekrar açmaya gerek yok ama istersen üst üste bindirebilirsin
        if (!player.shieldActive)
        {
            player.shieldActive = true;
        }

        // Bu item kullanýldý, masadan kalksýn
        Destroy(gameObject);
    }
}
