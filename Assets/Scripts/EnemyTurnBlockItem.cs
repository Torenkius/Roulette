using UnityEngine;

public class EnemyTurnBlockItem : ClickableItem
{
    public override void OnClicked()
    {
        // Turn sistemini yöneten bir manager'ýmýz olduðunu varsayýyorum
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.blockNextEnemyTurn = true;
            Debug.Log(itemName + " kullanýldý! Bir sonraki enemy turn bloklandý.");
        }
        else
        {
            Debug.LogWarning("TurnManager.Instance bulunamadý!");
        }

        // Item kullanýldýktan sonra masadan kalksýn
        Destroy(gameObject);
    }
}
