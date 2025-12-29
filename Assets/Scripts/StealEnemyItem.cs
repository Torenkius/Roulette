using UnityEngine;

public class StealEnemyItem : ClickableItem
{
    [Header("Referanslar")]
    public Transform enemyItemArea;
    public Transform playerItemArea;  
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
            stolenItem.transform.SetParent(playerItemArea);
            stolenItem.transform.localPosition = Vector3.zero;
        }
        Destroy(gameObject);
    }
}
