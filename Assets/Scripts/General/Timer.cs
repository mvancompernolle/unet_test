using UnityEngine;

[System.Serializable]
public class Timer
{
    public bool hasFired { get; private set; }
    public bool isActive { get; private set; }
    public float timerStart;
    public float timeLeft { get; private set; }
    public float timePassed { get { return timerStart - timeLeft; } }

    public Timer(float time, bool active = false)
    {
        hasFired = false;
        isActive = false;
        timeLeft = timerStart = time;

        if (active)
            Activate();
    }

    /// <summary>
    /// Updates the timer by deltaTime.
    /// </summary>
    /// <param name="modTime"> Additional time to take off the timer. Negative numbers increase the timer. </param>
    public bool Update(float dt)
    {
        if (isActive)
        {
            timeLeft -= dt;
            if (timeLeft <= 0.0f)
            {
                hasFired = true;
                isActive = false;
                timeLeft = 0.0f;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Start the timer
    /// </summary>
    /// <param name="time"> Changes the start time. </param>
    public void Activate(float time)
    {
        timerStart = time;
        Activate();
    }

    public void Activate()
    {
        hasFired = false;
        isActive = true;
        timeLeft = timerStart;
    }

    /// <summary>
    /// Stop the timer.
    /// </summary>
    /// <param name="forceToFire"> Force the timer to fire, instead of just stopping. </param>
    public void Deactivate(bool forceToFire = false)
    {
        isActive = false;
        hasFired = forceToFire;
    }
}