
using UnityEngine;
using TMPro;

public class EnemyCountUI : MonoBehaviour
{
    public TMP_Text enemyCountText;

    void Update()
    {
        EnemyCountTarget[] enemies =
            FindObjectsByType<EnemyCountTarget>(
                FindObjectsSortMode.None
            );

        int remaining = 0;

        foreach (EnemyCountTarget enemy in enemies)
        {
            if (enemy.IsAlive)
                remaining++;
        }

        if (enemyCountText != null)
            enemyCountText.text = "x " + remaining;
    }
}
