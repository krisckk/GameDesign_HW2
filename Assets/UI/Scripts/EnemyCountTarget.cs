
using System;
using UnityEngine;

public class EnemyCountTarget : MonoBehaviour
{
    public static event Action AnyDied;

    public bool IsAlive { get; private set; } = true;

    public void MarkDead()
    {
        if (!IsAlive) return;

        IsAlive = false;
        Debug.Log(gameObject.name + " died!");
        AnyDied?.Invoke();
    }
}
