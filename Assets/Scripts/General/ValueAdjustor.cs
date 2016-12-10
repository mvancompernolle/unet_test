using UnityEngine;
using System.Collections.Generic;

public class ValueAdjustor
{

    private struct TimedValue
    {
        public float value;
        public Timer timer;

        public TimedValue(float value, float time)
        {
            this.value = value;
            timer = new Timer(time, true);
        }
    }

    private Dictionary<string, float> adjustors = new Dictionary<string, float>();
    private Dictionary<string, TimedValue> timedAdjustors = new Dictionary<string, TimedValue>();
    private float totalValue = 0.0f;

    public void SetAdjustor(string key, float val)
    {
        float oldVal;
        if (adjustors.TryGetValue(key, out oldVal))
            totalValue -= oldVal;

        adjustors[key] = val;
        totalValue += val;
    }

    public void RemoveAdjustor(string key)
    {
        float oldVal;
        if (adjustors.TryGetValue(key, out oldVal))
        {
            totalValue -= oldVal;
            adjustors.Remove(key);
        }
    }

    public void SetTimedAdjustor(string key, float val, float time)
    {
        TimedValue oldVal;
        if (timedAdjustors.TryGetValue(key, out oldVal))
            totalValue -= oldVal.value;

        timedAdjustors[key] = new TimedValue(val, time);
        totalValue += val;
    }

    public void RemoveTimedAdjustor(string key)
    {
        TimedValue oldVal;
        if (timedAdjustors.TryGetValue(key, out oldVal))
        {
            totalValue -= oldVal.value;
            adjustors.Remove(key);
        }
    }

    public void UpdateTimedValues(float dt)
    {
        List<string> toRemoveList = new List<string>();
        foreach (KeyValuePair<string, TimedValue> timedValue in timedAdjustors)
        {
            if (timedValue.Value.timer.Update(dt))
            {
                toRemoveList.Add(timedValue.Key);
            }
        }
        foreach (string key in toRemoveList)
        {
            RemoveAdjustor(key);
        }
    }

    public float GetValue()
    {
        return totalValue;
    }

    public void Clear()
    {
        adjustors.Clear();
        totalValue = 0.0f;
    }
}
