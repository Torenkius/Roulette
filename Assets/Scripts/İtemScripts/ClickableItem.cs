using UnityEngine;
using UnityEngine.AI;

public abstract class ClickableItem : MonoBehaviour
{
    public string itemName = "item";
    public  PlayerCharacter player;
    public AIController enemy;
    private void Awake()
    {
        player= GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
        enemy=GameObject.FindWithTag("Enemy").GetComponent<AIController>();
    }

    public abstract void OnClicked(ShooterType type);
}
