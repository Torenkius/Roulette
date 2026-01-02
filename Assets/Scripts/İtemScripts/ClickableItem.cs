using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public abstract class ClickableItem : MonoBehaviour
{
    public string itemName = "item";
    public  PlayerCharacter player;
    public AIController enemy;
    public Animator animator;
    private void Awake()
    {
        player= GameObject.FindWithTag("Player").GetComponent<PlayerCharacter>();
        enemy=GameObject.FindWithTag("Enemy").GetComponent<AIController>();
    }

    public abstract void OnClicked(ShooterType type);
    protected virtual void OnMouseEnter()
    {
        if (animator != null) animator.SetBool("isHover", true);
    }

    protected virtual void OnMouseExit()
    {
        if (animator != null) animator.SetBool("isHover", false);
    }
}
