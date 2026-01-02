using UnityEngine;

public class StealEnemyItem : ClickableItem
{
    [Header("Referanslar")]
    public Transform enemyItemArea;
    public Transform playerItemArea;
    public Transform stolenitemArea;
    public override void OnClicked(ShooterType t)
    {
        if (t == ShooterType.Player)
        {
            this.gameObject.transform.parent = player.itemholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isTake");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_gun_powder_sound();
            }
        }
        else if (t == ShooterType.Enemy)
        {
            enemy.animator.SetTrigger("isTake");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_gun_powder_sound();
            }
        }

        if (enemyItemArea == null)
        {
            Debug.LogWarning("StealEnemyItem: enemyItemArea atanmadý!");
            return;
        }
        ClickableItem[] enemyItems = enemyItemArea.GetComponentsInChildren<ClickableItem>();
        if (enemyItems.Length == 0)
        {
            Debug.Log("Düþmanýn çalýnacak itemi yok.");
            return;
        }
        int randomIndex = Random.Range(0, enemyItems.Length);
        ClickableItem stolenItem = enemyItems[randomIndex];
        Debug.Log("Düþmandan çalýnan item: " + stolenItem.itemName);
        if (playerItemArea != null)
        {
            stolenItem.transform.parent = playerItemArea;
            stolenItem.transform.position= stolenitemArea.position;
            stolenItem.gameObject.layer = LayerMask.NameToLayer("Item");
        }
        Destroy(gameObject,2f);
    }
}
