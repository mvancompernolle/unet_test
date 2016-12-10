﻿using UnityEngine;

[System.Serializable]
public class PID
{

    // P I D
    // Ship
    // Angle Controller : 3, 0, 0.033
    // Angular Vel Controller : 0.79, 0, 0

    // Game State
    // Distance Controller : 300, 0, 10
    public string name;

    [SerializeField]
    public float Kp = 1;
    [SerializeField]
    public float Ki = 0;
    [SerializeField]
    public float Kd = 0.1f;

    private float P, I, D;
    private float prevError;
    private bool firstOutput;

    public PID(string n)
    {
        name = n;
        Reset();
    }

    public PID(float p, float i, float d, string name = "")
    {
        Kp = p;
        Ki = i;
        Kd = d;
        this.name = name;
        Reset();
    }

    public PID(PID pid)
    {
        name = pid.name;
        Kp = pid.Kp;
        Ki = pid.Ki;
        Kd = pid.Kd;
        Reset();
    }

    public void Reset()
    {
        firstOutput = true;
        P = I = D = prevError = 0.0f;
    }

    public float GetOutput(float currentError, float deltaTime)
    {
        P = currentError;
        I += P * deltaTime;
        if (firstOutput)
        {
            D = 0.0f;
            firstOutput = false;
        }
        else
        {
            D = (P - prevError) / deltaTime;
        }

        prevError = currentError;
        return P * Kp + I * Ki + D * Kd;
    }
}
