using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GunController : MonoBehaviour
{
    private Queue<ShellType> magazine = new Queue<ShellType>();
    
    [Header("Gun Settings")]
    public Transform firePoint;   // Ateş efekti veya mermi çıkış noktası
    // public AudioSource fireSound; // İleride eklenebilir

    // Şarjörü doldurur ve karıştırır
    public void LoadMagazine(int liveCount, int blankCount)
    {
        magazine.Clear();
        List<ShellType> shells = new List<ShellType>();

        for (int i = 0; i < liveCount; i++) shells.Add(ShellType.Live);
        for (int i = 0; i < blankCount; i++) shells.Add(ShellType.Blank);

        // Shuffle (Karıştırma)
        System.Random rng = new System.Random();
        shells = shells.OrderBy(a => rng.Next()).ToList();

        foreach (var shell in shells)
        {
            magazine.Enqueue(shell);
        }

        Debug.Log($"Magazine Loaded: {liveCount} Live, {blankCount} Blank");
    }

    // Ateş etme fonksiyonu - Mermi tipini döndürür
    public ShellType Fire()
    {
        if (magazine.Count == 0)
        {
            Debug.LogError("Magazine is empty!");
            return ShellType.Blank; // Hata durumunda boş dönelim
        }

        ShellType currentShell = magazine.Dequeue();
        
        // Burada animasyon veya ses tetiklenebilir
        Debug.Log($"Fired: {currentShell}");
        
        return currentShell;
    }

    public int GetRemainingShells()
    {
        return magazine.Count;
    }
}
