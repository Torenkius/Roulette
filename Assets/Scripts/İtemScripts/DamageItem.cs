using UnityEditor.TextCore.Text;
using UnityEngine;

public class DamageItem : ClickableItem
{
    public int damageAmountX = 2;

    public override void OnClicked(ShooterType t)
    {
        Debug.Log(itemName + " týklandý! Hasar veriyor: " + damageAmountX);
         GameObject gunObject = GameObject.FindGameObjectWithTag("Gun");
        if(t==ShooterType.Player ){
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

        if (gunObject != null)
        {
            GunController gun = gunObject.GetComponent<GunController>();

            if (gun != null)
            {
                gun.damageMultiplierActive=true;
            }
            else
            {
                Debug.LogWarning("Gun scripti bulunamadý!");
            }
        }
        else
        {
            Debug.LogWarning("Gun tag'li obje bulunamadý!");
        }
        Destroy(gameObject,2f);
    }
   
    
       
    

}
