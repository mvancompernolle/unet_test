using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ShipAbilityCont : NetworkBehaviour {

    // ability point variables
    public bool collisionPointsLocked { get; set; }
    private HashSet<string> abilityLockers = new HashSet<string>();
    [SerializeField]
    public int abilityPoints = 0;
    [SerializeField]
    private int MAX_ABILITY_POINTS = 5;
    [SerializeField]
    private int MAX_ABILITY_POINT_MULTIPLIER = 3;
    [SerializeField]
    private int HITS_TO_INCREASE_MULTIPLIER = 2;
    [SerializeField]
    private int hitsLeftToIncreaseMultiplier;
    private int abilityPointMultiplier = 1;
    private int playerNum;
    Dictionary<GameObject, Timer> playerCollisionTracker = new Dictionary<GameObject, Timer>();

    // components
    private ShipMasterComp masterComp;
    AbilityBase[] abilities = null;

    // Use this for initialization
    void Start()
    {
        // cached different ship abilities
        masterComp = GetComponent<ShipMasterComp>();
        abilities = GetComponents<AbilityBase>();

        hitsLeftToIncreaseMultiplier = HITS_TO_INCREASE_MULTIPLIER;
        playerNum = masterComp.playerNum;
        collisionPointsLocked = false;
    }

    void Update()
    {
        if (playerCollisionTracker.Count > 0)
        {
            List<GameObject> removeList = new List<GameObject>();
            foreach (KeyValuePair<GameObject, Timer> entry in playerCollisionTracker)
            {
                entry.Value.Update(Time.deltaTime);
                if (!entry.Value.isActive) removeList.Add(entry.Key);
            }
            foreach (GameObject player in removeList)
            {
                playerCollisionTracker.Remove(player);
            }
        }
    }

    public void SuccessfullyHitPlayer(GameObject player)
    {
        if (playerCollisionTracker.ContainsKey(player) || collisionPointsLocked) return;
        playerCollisionTracker[player] = new Timer(0.5f, false);
        UpgradeMultiplier(1);
        AddAbilityPoints(1);
    }

    public int GetNumAbilityPoints()
    {
        return abilityPoints;
    }

    public void AddAbilityPoints(int points = 1, bool withMultipler = true)
    {
        if (abilityPoints != MAX_ABILITY_POINTS)
        {
            abilityPoints += withMultipler ? (points * abilityPointMultiplier) : points;
            abilityPoints = Mathf.Min(MAX_ABILITY_POINTS, abilityPoints);
        }
    }

    public void SetAbilityPoints(int points)
    {
        abilityPoints = points;
    }

    public void UpgradeMultiplier(int points = 1)
    {
        if (abilityPointMultiplier == MAX_ABILITY_POINT_MULTIPLIER) return;
        --hitsLeftToIncreaseMultiplier;
        if (hitsLeftToIncreaseMultiplier == 0)
        {
            abilityPointMultiplier++;
            hitsLeftToIncreaseMultiplier = HITS_TO_INCREASE_MULTIPLIER;
        }
    }

    public bool AttemptToUseAbilityPoints(int num)
    {
        if (AbilitiesDisabled() || num > abilityPoints)
        {
            return false;
        }
        abilityPoints -= num;
        return true;
    }

    public bool AbilitiesDisabled()
    {
        return abilityLockers.Count > 0;
    }

    public void CancelAbilities()
    {
        foreach (AbilityBase ability in abilities)
        {
            ability.Cancel();
        }
    }

    public void CancelAbilitiesExcept(AbilityBase exception)
    {
        foreach (AbilityBase ability in abilities)
        {
            if (ability != exception) ability.Cancel();
        }
    }

    public void SetAbilityLocker(string key)
    {
        abilityLockers.Add(key);
    }

    public void RemoveAbilityLocker(string key)
    {
        if (abilityLockers.Contains(key))
            abilityLockers.Remove(key);
    }
}
