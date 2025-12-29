using UnityEngine;

public class EnemyTurnBlockItem : ClickableItem
{
    public override void OnClicked(ShooterType t)
    {
        // Turn sistemini yöneten bir manager'ýmýz olduðunu varsayýyorum
        GameManager gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        gameManager.isRoundFreeze = true;

        // Item kullanýldýktan sonra masadan kalksýn
        Destroy(gameObject);
    }
}
