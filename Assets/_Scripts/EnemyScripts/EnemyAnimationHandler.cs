using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public int numberOfRandomAnimations = 2;

    public void TriggerAttack() => animator.SetTrigger("Attack");
    public void PlayDeath() => animator.SetBool("isDead", true);
    public void SetCrawling(bool crawl) => animator.SetBool("isCrawling", crawl);


    void Awake()
    {
        SetRandomWalkIndex();
    }

    void SetRandomWalkIndex()
    {
        int index = Random.Range(0, numberOfRandomAnimations);
        animator.SetFloat("randomIndex", index);
    }

    public void SetMoveSpeed(float speed)
    {
        animator.SetFloat("moveSpeed", speed);
    }
}
