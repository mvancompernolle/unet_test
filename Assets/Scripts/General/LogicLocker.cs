using System.Collections.Generic;

public class LogicLocker
{

    private HashSet<string> lockers = new HashSet<string>();

    public void SetLocker(string key)
    {
        lockers.Add(key);
    }

    public void RemoveLocker(string key)
    {
        lockers.Remove(key);
    }

    public bool IsLocked()
    {
        return lockers.Count != 0;
    }

    public bool Contains(string key)
    {
        return lockers.Contains(key);
    }

    public void Clear()
    {
        lockers.Clear();
    }

}
