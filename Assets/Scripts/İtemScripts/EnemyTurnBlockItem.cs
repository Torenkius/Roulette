using UnityEngine;

public class EnemyTurnBlockItem : ClickableItem
{
    public override void OnClicked(ShooterType t)
    {
        // Turn sistemini yöneten bir manager'ýmýz olduðunu varsayýyorum
        GameManager gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        gameManager.isRoundFreeze = true;
        if (t == ShooterType.Player)
        {
            this.gameObject.transform.parent = player.itemholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isTake");
        }
        else if (t == ShooterType.Enemy)
        {
            enemy.animator.SetTrigger("isTake");
        }


        // Item kullanýldýktan sonra masadan kalksýn
        Destroy(gameObject,2f);
    }
}
