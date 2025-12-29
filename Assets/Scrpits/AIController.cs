using UnityEngine;

/// <summary>
/// AIController = Computer player brain.
/// - Does NOT manage the game rules.
/// - Only decides: which items to use + shoot target (Self/Opponent).
/// - GameManager applies everything: ammo queue, reload, damage, turn switching, handcuffs, etc.
/// </summary>
public class AIController : MonoBehaviour
{
    // =========================
    // Types
    // =========================
    public enum TurnOwner { Player, AI }
    public enum ShootTarget { Self, Opponent }

    public enum AIPersonality { Aggressive, Balanced, Coward }

    public enum ItemType
    {
        BeerEject,
        CigaretteHeal,
        HandSawDamageBoost,
        HandcuffsSkip,
        MagnifyingGlassPeek,
        Shield,
        ShuffleAmmo,
        StealGun
    }

    // =========================
    // Inspector
    // =========================
    [Header("Wiring")]
    [Tooltip("Assign a component that implements IGameManager.")]
    public MonoBehaviour gameManagerBehaviour;

    [Tooltip("Which turn owner this AI controls.")]
    public TurnOwner myTurnOwner = TurnOwner.AI;

    [Header("AI Settings")]
    public AIPersonality personality = AIPersonality.Balanced;

    [Range(0f, 0.25f)]
    [Tooltip("Chance to make a suboptimal decision (0 = perfect).")]
    public float humanErrorChance = 0.10f;

    [Tooltip("Delay before each AI action to feel like thinking.")]
    public float thinkDelaySeconds = 0.25f;

    [Header("Safety")]
    [Tooltip("Max number of item uses AI will attempt in a single turn. Set <=0 for unlimited.")]
    public int maxItemUsesPerTurn = 8; // [Changed] <=0 => unlimited

    [Tooltip("Stops runaway if GM keeps giving turn due to self-blank chains. Set <=0 for unlimited.")]
    public int maxChainedActs = 60; // [Changed] <=0 => unlimited

    [Header("Heuristics (Added)")]
    [Tooltip("If remaining shells are low and odds look bad for the planned shot, prefer Beer/Shuffle to speed up.")]
    public int lowAmmoRoundsThreshold = 2;

    [Range(0f, 1f)]
    [Tooltip("If planning to shoot Opponent and pLive is below this, it's considered 'bad odds'.")]
    public float badOddsOppShootPLive = 0.35f;

    [Range(0f, 1f)]
    [Tooltip("If planning to shoot Self and pLive is above this, it's considered 'bad odds'.")]
    public float badOddsSelfShootPLive = 0.65f;

    [Tooltip("Allows the AI to sometimes stop item usage early (human-like). If false, it won't stop early when an item is valuable.")]
    public bool allowHumanLikeEarlyStop = true;

    // =========================
    // Internal
    // =========================
    private IGameManager gm;
    private bool acting;
    private int chainedActs;

    // =========================
    // Unity
    // =========================
    private void Awake()
    {
        gm = gameManagerBehaviour as IGameManager;
        if (gm == null)
            Debug.LogError("[AIController] gameManagerBehaviour must implement IGameManager.");
    }

    private void Update()
    {
        if (gm == null) return;
        if (gm.IsGameOver()) return;

        if (!acting && gm.IsMyTurn(myTurnOwner))
        {
            acting = true;
            chainedActs = 0;

            if (thinkDelaySeconds <= 0f) Act();
            else Invoke(nameof(Act), thinkDelaySeconds);
        }
    }

    // =========================
    // Main turn logic
    // =========================
    private void Act()
    {
        if (gm == null)
        {
            acting = false;
            return;
        }

        if (gm.IsGameOver())
        {
            acting = false;
            return;
        }

        // Turn could have changed while we were waiting.
        if (!gm.IsMyTurn(myTurnOwner))
        {
            acting = false;
            return;
        }

        // Your rule: ammo auto reload when empty.
        gm.ReloadIfEmpty();

        // If handcuffed, GM should skip our turn and clear the status.
        if (gm.IsHandcuffed(myTurnOwner))
        {
            gm.ResolveHandcuffSkip(myTurnOwner);
            acting = false;
            return;
        }

        // Prevent infinite loops if self-blank keeps the turn repeatedly.
        chainedActs++;
        if (maxChainedActs > 0 && chainedActs > maxChainedActs) // [Changed] optional unlimited
        {
            Debug.LogWarning("[AIController] maxChainedActs reached; stopping to prevent runaway.");
            acting = false;
            return;
        }

        // 1) Use items (0..N)
        UseItemsSmartly();

        // 2) Decide shot target
        ShootTarget target = DecideShotTarget(previewOnly: false);

        // 3) Shoot (GM applies: draw shell, damage, saw/shield, turn switch rules, reload, etc.)
        bool ok = gm.Shoot(myTurnOwner, target);
        if (!ok)
        {
            Debug.LogWarning("[AIController] GM rejected Shoot().");
            acting = false;
            return;
        }

        // If GM kept our turn (self blank chain, steal gun effect), act again.
        if (gm.IsMyTurn(myTurnOwner))
        {
            gm.ReloadIfEmpty();

            if (thinkDelaySeconds <= 0f) Act();
            else Invoke(nameof(Act), thinkDelaySeconds);
        }
        else
        {
            acting = false;
        }
    }

    // =========================
    // Item usage
    // =========================
    private void UseItemsSmartly()
    {
        int uses = 0;
        int usesLimit = (maxItemUsesPerTurn <= 0) ? int.MaxValue : maxItemUsesPerTurn; // [Changed] optional unlimited

        while (uses < usesLimit && gm.IsMyTurn(myTurnOwner) && !gm.IsGameOver())
        {
            gm.ReloadIfEmpty();

            // [Changed] get both best item and its score so we can avoid "early stop" when item is truly valuable
            if (!TryChooseBestItemToUse(out ItemType best, out float bestScore))
                break;

            // Human-like: sometimes stop early
            // [Changed] only allow early stop when the best option is low-value
            if (allowHumanLikeEarlyStop && humanErrorChance > 0f && bestScore < 22f && Random.value < humanErrorChance * 0.25f)
                break;

            bool ok = gm.UseItem(myTurnOwner, best);
            if (!ok) break;

            uses++;
        }
    }

    private bool TryChooseBestItemToUse(out ItemType best, out float bestScore)
    {
        TurnOwner opp = OpponentOf(myTurnOwner);

        int myHP = gm.GetHealth(myTurnOwner);
        int oppHP = gm.GetHealth(opp);
        int maxHP = gm.GetMaxHealth();

        (int live, int blank) = gm.GetAmmoCounts();
        int total = live + blank;
        float pLive = LiveProb(live, blank);

        bool nextKnown = gm.IsNextRoundKnown(myTurnOwner);
        bool nextIsLive = nextKnown ? gm.GetNextRoundIsLive(myTurnOwner) : false;
        float effectivePLive = nextKnown ? (nextIsLive ? 1f : 0f) : pLive;

        bool myShield = gm.IsShieldActive(myTurnOwner);
        bool mySaw = gm.IsSawActive(myTurnOwner);
        bool oppHandcuffed = gm.IsHandcuffed(opp);
        bool myStealGun = gm.IsStealGunActive(myTurnOwner);

        // Preview shot WITHOUT randomness so item choice stays stable.
        ShootTarget plannedShot = DecideShotTarget(previewOnly: true);

        bestScore = 0f;
        best = default;
        bool found = false;

        bool HasAnyItem(TurnOwner owner)
        {
            // cheap check: if any count > 0
            foreach (ItemType it in System.Enum.GetValues(typeof(ItemType)))
            {
                if (gm.GetItemCount(owner, it) > 0) return true;
            }
            return false;
        }

        void Consider(ItemType item, float score)
        {
            if (score <= bestScore) return;
            if (gm.GetItemCount(myTurnOwner, item) <= 0) return;
            if (!gm.CanUseItem(myTurnOwner, item)) return;

            bestScore = score;
            best = item;
            found = true;
        }

        // =========================
        // A) Defense
        // =========================
        if (myHP < maxHP)
        {
            float score = (myHP <= 1) ? 100f : (myHP == 2 ? 45f : 12f);
            score += (maxHP - myHP) * 5f;
            Consider(ItemType.CigaretteHeal, score);
        }

        if (!myShield && myHP <= 2 && effectivePLive >= 0.55f)
        {
            float score = (myHP <= 1) ? 70f : 40f;
            Consider(ItemType.Shield, score);
        }

        // =========================
        // B) Info (Peek)
        // =========================
        if (!nextKnown)
        {
            bool uncertaintyHigh = pLive >= 0.30f && pLive <= 0.70f;
            bool stakesHigh = (myHP <= 2) || (oppHP <= 2);

            if (uncertaintyHigh && stakesHigh)
            {
                float bias =
                    (personality == AIPersonality.Coward) ? 1.2f :
                    (personality == AIPersonality.Aggressive) ? 0.8f : 1.0f;

                Consider(ItemType.MagnifyingGlassPeek, 35f * bias);
            }
        }

        // =========================
        // C) Tempo (Handcuffs / StealGun)
        // =========================
        if (!oppHandcuffed)
        {
            bool tempoMatters = (myHP <= 2) || (oppHP <= 2) || (personality == AIPersonality.Aggressive);
            if (tempoMatters && effectivePLive >= 0.35f)
            {
                float score = (oppHP <= 2) ? 45f : 30f;
                if (myHP <= 2) score += 10f;
                Consider(ItemType.HandcuffsSkip, score);
            }
        }

        // [Changed] Broaden StealGun usage: not only "critical/finisher", also tempo when planning to shoot Opponent.
        if (!myStealGun)
        {
            bool wantShootOpp = (plannedShot == ShootTarget.Opponent);
            if (wantShootOpp)
            {
                float score = 18f;

                // tempo/value: if we can chain another Act, it's useful
                if (HasAnyItem(myTurnOwner)) score += 6f;
                if (personality == AIPersonality.Aggressive) score += 4f;

                // if odds suggest a damaging shot, keep initiative
                if (effectivePLive >= 0.55f) score += 8f;

                // finisher / clutch
                if (oppHP <= 2) score += 12f;
                else if (oppHP == 3) score += 6f;

                if (myHP <= 2) score += 6f;

                // if we *know* next is live and we're about to shoot opponent, keep turn is very strong
                if (nextKnown && nextIsLive) score += 10f;

                Consider(ItemType.StealGun, score);
            }
        }

        // =========================
        // D) Damage (Saw)
        // =========================
        if (!mySaw)
        {
            bool liveLikely = nextKnown ? nextIsLive : (pLive >= 0.50f);
            bool wantShootOpp = (plannedShot == ShootTarget.Opponent);
            bool goingForKill = (oppHP <= 2);

            if (goingForKill && liveLikely && wantShootOpp)
                Consider(ItemType.HandSawDamageBoost, 55f);
        }

        // =========================
        // E) Beer / Shuffle
        // =========================
        if (nextKnown)
        {
            bool badForPlan =
                (plannedShot == ShootTarget.Opponent && !nextIsLive) ||
                (plannedShot == ShootTarget.Self && nextIsLive);

            if (badForPlan)
                Consider(ItemType.BeerEject, 60f);
        }

        if (personality == AIPersonality.Coward && myHP <= 2 && pLive >= 0.60f)
        {
            Consider(ItemType.BeerEject, 40f);
        }

        bool extreme = (pLive <= 0.15f) || (pLive >= 0.85f);
        if (extreme)
        {
            Consider(ItemType.ShuffleAmmo, 18f);
        }

        // [Added] Low ammo + bad odds => speed up cycle with Beer/Shuffle
        if (!nextKnown && total > 0 && total <= lowAmmoRoundsThreshold)
        {
            bool badOddsForPlan =
                (plannedShot == ShootTarget.Opponent && pLive < badOddsOppShootPLive) ||
                (plannedShot == ShootTarget.Self && pLive > badOddsSelfShootPLive);

            if (badOddsForPlan)
            {
                float urgency = 20f + (lowAmmoRoundsThreshold - total) * 8f;

                // Prefer Beer to remove the next shell when it's likely to waste time / be bad for plan
                Consider(ItemType.BeerEject, 42f + urgency);

                // If no Beer or can't use, Shuffle as a fallback "shake it up"
                Consider(ItemType.ShuffleAmmo, 26f + urgency * 0.5f);
            }
        }

        return found;
    }

    // =========================
    // Shot decision
    // =========================
    private ShootTarget DecideShotTarget(bool previewOnly)
    {
        TurnOwner opp = OpponentOf(myTurnOwner);

        (int live, int blank) = gm.GetAmmoCounts();
        float pLive = LiveProb(live, blank);

        bool nextKnown = gm.IsNextRoundKnown(myTurnOwner);
        bool nextIsLive = nextKnown ? gm.GetNextRoundIsLive(myTurnOwner) : false;

        // Random error (disabled for preview)
        if (!previewOnly && humanErrorChance > 0f && Random.value < humanErrorChance)
            return (Random.value > 0.5f) ? ShootTarget.Opponent : ShootTarget.Self;

        // Peek: play perfectly
        if (nextKnown)
            return nextIsLive ? ShootTarget.Opponent : ShootTarget.Self;

        // Base thresholds by personality
        float threshold =
            (personality == AIPersonality.Aggressive) ? 0.30f :
            (personality == AIPersonality.Coward) ? 0.65f :
            0.50f;

        int myHP = gm.GetHealth(myTurnOwner);
        int oppHP = gm.GetHealth(opp);
        int maxHP = gm.GetMaxHealth();

        // More aggressive when low HP (desperation)
        threshold -= DesperationAdjust(myHP, maxHP);

        // More aggressive when opponent low HP (finisher)
        threshold -= FinisherAdjust(oppHP);

        // Tempo advantage (if opponent is handcuffed)
        if (gm.IsHandcuffed(opp))
            threshold -= 0.05f;

        threshold = Mathf.Clamp(threshold, 0.05f, 0.95f);
        return (pLive >= threshold) ? ShootTarget.Opponent : ShootTarget.Self;
    }

    private float DesperationAdjust(int myHP, int maxHP)
    {
        if (myHP <= 1) return 0.18f;
        if (myHP == 2) return 0.10f;
        if (myHP == 3 && maxHP >= 5) return 0.04f;
        return 0f;
    }

    private float FinisherAdjust(int oppHP)
    {
        if (oppHP <= 1) return 0.20f;
        if (oppHP == 2) return 0.10f;
        return 0f;
    }

    private static TurnOwner OpponentOf(TurnOwner t) => (t == TurnOwner.AI) ? TurnOwner.Player : TurnOwner.AI;

    private static float LiveProb(int live, int blank)
    {
        int total = live + blank;
        return total <= 0 ? 0f : (float)live / total;
    }

    // =========================
    // GameManager Contract (AI expects these)
    // =========================
    public interface IGameManager
    {
        // Turn / game state
        bool IsMyTurn(TurnOwner turnOwner);
        bool IsGameOver();

        // Health
        int GetHealth(TurnOwner turnOwner);
        int GetMaxHealth();

        // Ammo counts (AI uses only probabilities)
        (int live, int blank) GetAmmoCounts();
        void ReloadIfEmpty();

        // Peek knowledge (per owner)
        bool IsNextRoundKnown(TurnOwner turnOwner);
        bool GetNextRoundIsLive(TurnOwner turnOwner);

        // Inventory
        int GetItemCount(TurnOwner turnOwner, ItemType item);

        // Status effects
        bool IsShieldActive(TurnOwner turnOwner);
        bool IsSawActive(TurnOwner turnOwner);
        bool IsHandcuffed(TurnOwner turnOwner);
        bool IsStealGunActive(TurnOwner turnOwner);

        // Item usage
        bool CanUseItem(TurnOwner turnOwner, ItemType item);
        bool UseItem(TurnOwner turnOwner, ItemType item);

        // Handcuffs resolution
        void ResolveHandcuffSkip(TurnOwner turnOwner);

        // Shooting (GM applies full rules)
        bool Shoot(TurnOwner turnOwner, ShootTarget target);
    }
}
