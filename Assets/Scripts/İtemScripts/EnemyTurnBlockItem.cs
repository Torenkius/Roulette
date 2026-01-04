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
            HUDLog.Instance.ShowMessage("Rakibin sýrasý donduruldu sýra tekrar sana geçicek");
            this.gameObject.transform.parent = player.itemholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isTake");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_whip_sound();
            }
        }
        else if (t == ShooterType.Enemy)
        {
            HUDLog.Instance.ShowMessage("Rakip sýraný dondurdu sýra tekrar rakibinde");
            this.gameObject.transform.parent = enemy.itemHolder;
            this.gameObject.transform.localPosition = Vector3.zero;
            enemy.animator.SetTrigger("isTake");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_whip_sound();
            }
        }


        // Item kullanýldýktan sonra masadan kalksýn
        Destroy(gameObject,2f);
    }
}
