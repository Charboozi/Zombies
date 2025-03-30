using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SlidingDoor : NetworkBehaviour, IInteractableAction
{
    [Header("Door Parts")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    [Header("Slide Offsets")]
    [SerializeField] private Vector3 leftOffset = new Vector3(-2f, 0f, 0f);
    [SerializeField] private Vector3 rightOffset = new Vector3(2f, 0f, 0f);

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 leftClosedPos, rightClosedPos;
    private Vector3 leftOpenPos, rightOpenPos;

    private bool isSliding = false;

    private NetworkVariable<bool> isOpen = new NetworkVariable<bool>(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        leftOpenPos = leftClosedPos + leftOffset;
        rightOpenPos = rightClosedPos + rightOffset;
    }

    public override void OnNetworkSpawn()
    {
        isOpen.OnValueChanged += (_, _) => AnimateDoor(isOpen.Value);
        AnimateDoor(isOpen.Value); // Ensure correct state on spawn
    }

    public void DoAction()
    {
        if (!IsServer || isSliding) return;

        isOpen.Value = !isOpen.Value;
    }

    private void AnimateDoor(bool opening)
    {
        StartCoroutine(SlideDoors(opening));
    }

    private IEnumerator SlideDoors(bool opening)
    {
        isSliding = true;

        Vector3 leftStart = leftDoor.localPosition;
        Vector3 leftTarget = opening ? leftOpenPos : leftClosedPos;

        Vector3 rightStart = rightDoor.localPosition;
        Vector3 rightTarget = opening ? rightOpenPos : rightClosedPos;

        float timer = 0f;

        while (timer < slideDuration)
        {
            float t = slideCurve.Evaluate(timer / slideDuration);
            leftDoor.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
            rightDoor.localPosition = Vector3.Lerp(rightStart, rightTarget, t);

            timer += Time.deltaTime;
            yield return null;
        }

        leftDoor.localPosition = leftTarget;
        rightDoor.localPosition = rightTarget;
        isSliding = false;
    }
}
