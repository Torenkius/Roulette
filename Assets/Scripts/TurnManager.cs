using UnityEngine;

public enum TurnState
{
    PlayerTurn,
    EnemyTurn
}

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState currentTurn = TurnState.PlayerTurn;

    [HideInInspector]
    public bool blockNextEnemyTurn = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartEnemyTurn()
    {
        // Enemy turn baþlarken çaðrýldýðýný varsayýyoruz
        currentTurn = TurnState.EnemyTurn;

        // Eðer item ile bloklanmýþsa bu turn’ü tamamen atla
        if (blockNextEnemyTurn)
        {
            Debug.Log("Enemy turn item tarafýndan bloklandý!");
            blockNextEnemyTurn = false;
            EndEnemyTurn();
            return;
        }

        // Normal enemy hareketlerini burada yaparsýn:
        // EnemyShot();
        // vs...

        // Bittiðinde:
        EndEnemyTurn();
    }

    public void EndEnemyTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        Debug.Log("Enemy turn bitti, sýra player’da.");
    }
}
