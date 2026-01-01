using UnityEngine;
using UnityEngine.InputSystem; // New Input System Namespace

public class TableTester : MonoBehaviour
{
    public TableController tableController;
    public GameObject testItemPrefab; // Test için küp veya silindir prefabı

    private void Start()
    {
        if (tableController == null) Debug.LogError("TableTester: TableController is MISSING in Inspector!");
        if (testItemPrefab == null) Debug.LogError("TableTester: TestItemPrefab is MISSING in Inspector!");
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (tableController == null || testItemPrefab == null) return; // Hata varsa çalışma

        // 1 Tuşu: Oyuncuya eşya ver (Sigara gibi düşünelim)
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log("Test: Spawning Item for Player");
            tableController.SpawnItemOnTable(testItemPrefab, true);
        }

        // 2 Tuşu: Rakibe eşya ver
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log("Test: Spawning Item for Opponent");
            tableController.SpawnItemOnTable(testItemPrefab, false);
        }

        // R Tuşu: Masayı temizle
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("Test: Clearing Table");
            tableController.ClearTableItems();
        }

        // G Tuşu: Silahı masaya resetle
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Debug.Log("Test: Resetting Gun Position");
            tableController.PlaceGun();
        }
    }
}
