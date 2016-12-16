using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public abstract class AbilityBase : NetworkBehaviour
{
    // public 
    public Timer coolDownTimer = new Timer(0.0f);
    public int abilityPointCost = 1;
    public LogicLocker abilityLocker = new LogicLocker();
    public bool disabled
    {
        get
        {
            return abilityLocker.IsLocked();
        }
    }
    // protected
    protected ShipMasterComp masterComp;

    protected ShipInputs actions
    {
        get
        {
            return masterComp.inputCont.shipInputs;
        }
    }

    public virtual void Awake()
    {
        masterComp = GetComponent<ShipMasterComp>();
    }

    public virtual void Update()
    {
        if (coolDownTimer.isActive)
        {
            coolDownTimer.Update(Time.deltaTime);
        }
    }

    protected virtual bool AttemptToUseAbility()
    {
        if (!disabled && !OnCooldown() && masterComp.abilityCont.AttemptToUseAbilityPoints(abilityPointCost))
        {
            coolDownTimer.Activate();
            return true;
        }
        return false;
    }

    public void RefundAbility()
    {
        masterComp.abilityCont.AddAbilityPoints(abilityPointCost, false);
    }

    protected bool HasEnoughAbilityPoints()
    {
        return masterComp.abilityCont.GetNumAbilityPoints() >= abilityPointCost;
    }

    public abstract void Cancel();

    protected bool OnCooldown()
    {
        return coolDownTimer.isActive;
    }
}
