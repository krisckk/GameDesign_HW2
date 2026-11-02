
using System.Collections;
using UnityEngine;

public class StoneMonsterDamageAnimation : MonoBehaviour
{
    public Animation monsterAnimation;
    public StoneMonsterAI monsterAI;

    private Coroutine hitCoroutine;
    private bool isDead = false;

    void Awake()
    {
        if (monsterAnimation == null)
            monsterAnimation = GetComponentInChildren<Animation>();

        if (monsterAI == null)
            monsterAI = GetComponent<StoneMonsterAI>();
    }

    public void PlayHit()
    {
        if (isDead || monsterAnimation == null)
            return;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        hitCoroutine = StartCoroutine(HitRoutine());
    }

    IEnumerator HitRoutine()
    {
        if (monsterAI != null)
            monsterAI.enabled = false;

        monsterAnimation.CrossFade("Anim_Damage", 0.05f);

        float duration = 0.5f;

        if (monsterAnimation["Anim_Damage"] != null)
            duration = monsterAnimation["Anim_Damage"].length;

        yield return new WaitForSeconds(duration);

        if (!isDead && monsterAI != null)
            monsterAI.enabled = true;

        hitCoroutine = null;
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;

        if (hitCoroutine != null)
            StopCoroutine(hitCoroutine);

        if (monsterAI != null)
            monsterAI.StopAI();

        if (monsterAnimation != null)
        {
            monsterAnimation.Stop();
            monsterAnimation.Play("Anim_Death");
        }
    }
}
