using UnityEngine;

public class StealEnemyItem : ClickableItem
{
    [Header("Referanslar")]
    public Transform enemyItemArea;
    public Transform playerItemArea;
    public Transform stolenitemArea;
    public override void OnClicked(ShooterType t)
    {
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
        if (t == ShooterType.Player)
        {
            string str = string.Format("Rakibin  {0} itemini çaldýn!", stolenItem.itemName);
            HUDLog.Instance.ShowMessage(str);
            this.gameObject.transform.parent = player.itemholder;
            this.gameObject.transform.localPosition = Vector3.zero;
            player.animator.SetTrigger("isTake");
            enemy.myItems.Remove(stolenItem);   
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_gun_powder_sound();
            }
        }
        else if (t == ShooterType.Enemy)
        {
            string str = string.Format("Düþman  {0} itemini çaldý!", stolenItem.itemName);
            HUDLog.Instance.ShowMessage(str);
            this.gameObject.transform.parent = enemy.itemHolder;
            this.gameObject.transform.localPosition = Vector3.zero;
            enemy.myItems.Add(stolenItem);
            enemy.animator.SetTrigger("isTake");
            if (AudioManager.instance != null)
            {
                AudioManager.instance.Play_gun_powder_sound();
            }
        }
        Destroy(gameObject,2f);
    }
}
