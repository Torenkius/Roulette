using UnityEngine;

public abstract class ClickableItem : MonoBehaviour
{
    public string itemName = "Item";
    public  PlayerCharacter player;
    private void Awake()
    {
        player= GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
    }
    public abstract void OnClicked();
}
