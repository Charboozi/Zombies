using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetWalking(bool walking) => animator.SetBool("isWalking", walking);
    public void SetAttacking(bool attacking) => animator.SetBool("isAttacking", attacking);
    public void PlayDeath() => animator.SetBool("isDead", true);
    public void TriggerAttack() => animator.SetTrigger("Attack");

    public void SetWalkIndex(int index) => animator.SetInteger("walkIndex", index);
}
