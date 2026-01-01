using UnityEngine;
using System.Collections.Generic;

public class TableController : MonoBehaviour
{
    [Header("Table Layout")]
    public Transform gunRestPoint; // Silahın masada duracağı nokta

    [Header("Slots (Assign 4 for each side)")]
    public List<Transform> playerItemSlots; // Oyuncu tarafındaki eşya yuvaları (4 adet)
    public List<Transform> opponentItemSlots; // Rakip tarafındaki eşya yuvaları (4 adet)

    private const int RequestedSlotCount = 4;
    [Header("Layout Settings")]
    public Vector3 itemScale = new Vector3(0.34f, 0.34f, 0.34f);

    [Header("Active Objects")]
    public GunController activeGun; // Masadaki silahın referansı

    private void Start()
    {
        ValidateSlots();
        ArrangeSlots(); // Slotların yerlerini otomatik ayarla
        ClearTableItems(); // Oyun başlarken masadaki test objelerini temizle

        // Başlangıçta silahı yerine koy
        if (activeGun != null)
        {
            PlaceGun();
        }
    }

    private void ValidateSlots()
    {
        if (playerItemSlots.Count != RequestedSlotCount)
            Debug.LogWarning($"TableController: Player slots count is different than {RequestedSlotCount} (Current: {playerItemSlots.Count})");

        if (opponentItemSlots.Count != RequestedSlotCount)
            Debug.LogWarning($"TableController: Opponent slots count is different than {RequestedSlotCount} (Current: {opponentItemSlots.Count})");
    }

    [ContextMenu("Auto Arrange Slots")]
    public void ArrangeSlots()
    {
        // 4 Slotlu Grid (2x2)
        // Sol Üst (0,0) -> Sağ Alt (1,1) interpolasyonu

        // --- PLAYER ---
        // Player Sol Üst (Index 0): x=0, y=0, z=0 (was 1)
        // Player Sağ Alt (Index 3): x=1, y=0, z=-1 (was 0)
        Vector3 pStart = new Vector3(0f, 0f, 0f);
        Vector3 pEnd = new Vector3(1f, 0f, -1f);

        // --- OPPONENT (AI) ---
        // AI Sol Üst (Index 0): x=0, y=0, z=4.25 (was 5.25)
        // AI Sağ Alt (Index 3): x=1, y=0, z=3.25 (was 4.25)
        Vector3 oStart = new Vector3(0f, 0f, 4.25f);
        Vector3 oEnd = new Vector3(1f, 0f, 3.25f);

        ArrangeGrid(playerItemSlots, pStart, pEnd);
        ArrangeGrid(opponentItemSlots, oStart, oEnd);

        Debug.Log("Slots Arranged with New Coordinates!");
    }

    private void ArrangeGrid(List<Transform> slots, Vector3 startPos, Vector3 endPos)
    {
        // 2x2 Grid olduğu için:
        // Col 0 -> x = start.x
        // Col 1 -> x = end.x
        // Row 0 -> z = start.z
        // Row 1 -> z = end.z

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null) continue;

            int row = i / 2; // 0, 0, 1, 1
            int col = i % 2; // 0, 1, 0, 1

            // Lerp (Interpolasyon) kullanarak pozisyon bulma
            // Col 0 ise t=0, Col 1 ise t=1
            float xPos = Mathf.Lerp(startPos.x, endPos.x, col);

            // Row 0 ise t=0 (StartZ), Row 1 ise t=1 (EndZ)
            float zPos = Mathf.Lerp(startPos.z, endPos.z, row);

            slots[i].localPosition = new Vector3(xPos, startPos.y, zPos);
        }
    }

    [ContextMenu("Clear Slots Now")]
    public void ClearSlotsNow()
    {
        ClearTableItems();
        Debug.Log("Table Cleared via Context Menu");
    }

    /// <summary>
    /// Silahı masadaki yerine (gunRestPoint) yerleştirir.
    /// </summary>
    public void PlaceGun()
    {
        if (activeGun != null && gunRestPoint != null)
        {
            activeGun.transform.position = gunRestPoint.position;
            activeGun.transform.rotation = gunRestPoint.rotation;
        }
    }

    /// <summary>
    /// Belirtilen tarafa (Oyuncu veya Rakip) bir eşya spawn eder.
    /// </summary>
    /// <param name="itemPrefab">Oluşturulacak eşya prefab'ı</param>
    /// <param name="isPlayerSide">True ise oyuncu tarafına, False ise rakip tarafına</param>
    public void SpawnItemOnTable(GameObject itemPrefab, bool isPlayerSide)
    {
        List<Transform> targetSlots = isPlayerSide ? playerItemSlots : opponentItemSlots;

        // Boş bir slot bul
        Transform freeSlot = GetFreeSlot(targetSlots);

        if (freeSlot != null)
        {
            GameObject newItem = Instantiate(itemPrefab, freeSlot.position, freeSlot.rotation);
            newItem.transform.SetParent(freeSlot); // Düzenli durması için parent yap

            // Pozisyonu tam sıfırla ki slotun tam ortasında olsun
            newItem.transform.localPosition = Vector3.zero;
            newItem.transform.localRotation = Quaternion.identity;

            // İstenen Scale (Negatif değer girilirse düzelt)
            newItem.transform.localScale = new Vector3(
                Mathf.Abs(itemScale.x),
                Mathf.Abs(itemScale.y),
                Mathf.Abs(itemScale.z)
            );

            // Eğer hierarchy'den kapalı bir obje kopyalandıysa diye
            newItem.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Masada boş yer kalmadı!");
        }
    }

    private Transform GetFreeSlot(List<Transform> slots)
    {
        foreach (var slot in slots)
        {
            if (slot.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    /// <summary>
    /// Masadaki tüm eşyaları temizler (Silah hariç).
    /// </summary>
    public void ClearTableItems()
    {
        ClearSlots(playerItemSlots);
        ClearSlots(opponentItemSlots);
    }

    private void ClearSlots(List<Transform> slots)
    {
        foreach (var slot in slots)
        {
            // Tersten gitmek güvenlidir (listenin boyutu değişirse diye) ama burada child iterate ediyoruz
            // For loop ile child count'a bakmak daha güvenli olabilir destroy ederken.

            int childCount = slot.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = slot.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Slotları editörde görebilmek için çizim
        if (playerItemSlots != null)
        {
            Gizmos.color = Color.green;
            foreach (var slot in playerItemSlots)
            {
                if (slot != null) Gizmos.DrawWireCube(slot.position, itemScale);
            }
        }

        if (opponentItemSlots != null)
        {
            Gizmos.color = Color.red;
            foreach (var slot in opponentItemSlots)
            {
                if (slot != null) Gizmos.DrawWireCube(slot.position, itemScale);
            }
        }

        if (gunRestPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(gunRestPoint.position, new Vector3(0.5f, 0.1f, 0.2f));
        }
    }
}
