
using UnityEngine;

public class EnemyCountTarget : MonoBehaviour
{
    public bool IsAlive { get; private set; } = true;

    public void MarkDead()
    {
        if (!IsAlive) return;

        IsAlive = false;
        Debug.Log(gameObject.name + " died!");
    }
}
