using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShellVisualSpawner : MonoBehaviour
{
    [Header("Prefablar")]
    public GameObject liveShellPrefab;   // DOLU mermi asset
    public GameObject blankShellPrefab;  // BOÞ mermi asset

    [Header("Spawn Ayarlarý")]
    public Transform spawnParent;        // Mermilerin altýna gireceði parent (masa vs.)
    public float spacing = 0.3f;         // Mermiler arasý mesafe
    public Vector3 startOffset = Vector3.zero; // Parent'tan ne kadar offsetle baþlasýn

    private System.Random rng = new System.Random();

    /// <summary>
    /// liveCount ve blankCount'a göre mermileri diz.
    /// </summary>
    public void SpawnShells(int liveCount, int blankCount)
    {
        if (spawnParent == null)
            spawnParent = this.transform;

        // Önce eski mermileri temizle
        for (int i = spawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnParent.GetChild(i).gameObject);
        }

        // 1) Tip listesini oluþtur (dolu/boþ)
        List<ShellType> shells = new List<ShellType>();

        for (int i = 0; i < liveCount; i++)
            shells.Add(ShellType.Live);

        for (int i = 0; i < blankCount; i++)
            shells.Add(ShellType.Blank);

        // 2) Ýstersen karýþtýr (magazine sýrasý ile ayný olsun istiyorsan
        // GunController içinden listeyi alýp buraya direkt geçirebilirsin)
        shells = shells.OrderBy(x => rng.Next()).ToList();

        // 3) Listeye göre prefab spawnla
        for (int i = 0; i < shells.Count; i++)
        {
            GameObject prefabToSpawn = null;

            if (shells[i] == ShellType.Live)
                prefabToSpawn = liveShellPrefab;
            else
                prefabToSpawn = blankShellPrefab;

            if (prefabToSpawn == null)
            {
                Debug.LogWarning("Shell prefab eksik! Live/Blank prefab atamayý unutma.");
                continue;
            }

            // spawn pozisyonunu hesapla (yan yana diziyoruz)
            Vector3 worldPos = spawnParent.position
                               + spawnParent.right * (i * spacing)
                               + startOffset;
         // Yatay konumlandýrma

            GameObject shellObj = Instantiate(prefabToSpawn, worldPos, spawnParent.rotation, spawnParent);
        }
        spawnParent.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
