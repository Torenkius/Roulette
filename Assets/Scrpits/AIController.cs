using UnityEngine;
using System.Collections;

public class AIController : Participant
{
    // AI Karar Mantığı
    public enum AIMove
    {
        ShootSelf,
        ShootOpponent
    }

    public IEnumerator DecideMove(int liveCount, int blankCount, System.Action<AIMove> onDecisionMade)
    {
        // Düşünme süresi (Simülasyon)
        yield return new WaitForSeconds(1.5f);

        AIMove decision = AIMove.ShootOpponent; // Varsayılan: Rakibe sık

        int totalShells = liveCount + blankCount;
        
        if (totalShells > 0)
        {
            float liveChance = (float)liveCount / totalShells;

            // Mantık
            // Eğer boş mermi olasılığı yüksekse, kendine sıkıp ekstra tur kazanmayı dener.
            // Eğer dolu mermi olasılığı yüksekse, rakibe sıkar.
            
            if (liveChance > 0.5f)
            {
                // Dolu ihtimali yüksek -> Rakibe sık
                decision = AIMove.ShootOpponent;
            }
            else if (liveChance < 0.5f) // Boş ihtimali daha yüksek
            {
                // Boş ihtimali yüksek -> Kendine sık (Risk al)
                decision = AIMove.ShootSelf;
            }
            else 
            {
                // %50-%50 durumunda rastgele
                decision = Random.Range(0, 2) == 0 ? AIMove.ShootSelf : AIMove.ShootOpponent;
            }
        }
        
        Debug.Log($"AI Decision: {decision} (Live: {liveCount}, Blank: {blankCount})");
        onDecisionMade?.Invoke(decision);
    }
}
