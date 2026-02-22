using Fusion;
using UnityEngine;

/*
Setup:
1) Add this script to your PlayerAvatar network prefab root (same object as NetworkObject).
2) Assign headTarget / leftHandTarget / rightHandTarget to child proxy transforms on the prefab.
3) At runtime, for the local player only, call BindLocalRig(head, leftHand, rightHand)
   from your spawner/binder after the avatar is spawned.
4) Do NOT put your Meta XR camera rig inside this prefab.
*/
public sealed class NetworkRigProxy : NetworkBehaviour
{
    private const string LogTag = "[NetworkRigProxy]";

    [Header("Avatar Proxy Targets (networked visual transforms)")]
    [SerializeField] private Transform headTarget;
    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Local Meta XR Rig Transforms (local authority only)")]
    [SerializeField] private Transform localHead;
    [SerializeField] private Transform localLeftHand;
    [SerializeField] private Transform localRightHand;

    [Networked] private Vector3 HeadPos { get; set; }
    [Networked] private Quaternion HeadRot { get; set; }

    [Networked] private Vector3 LeftPos { get; set; }
    [Networked] private Quaternion LeftRot { get; set; }

    [Networked] private Vector3 RightPos { get; set; }
    [Networked] private Quaternion RightRot { get; set; }

    public bool IsPushingLocalPose { get; private set; }

    private bool hasLoggedPushStart;

    public void BindLocalRig(Transform head, Transform leftHand, Transform rightHand)
    {
        localHead = head;
        localLeftHand = leftHand;
        localRightHand = rightHand;

        Debug.Log($"{LogTag} Local rig bound. head={(head != null)} left={(leftHand != null)} right={(rightHand != null)}");
        hasLoggedPushStart = false;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null)
        {
            IsPushingLocalPose = false;
            return;
        }

        if (Object.HasInputAuthority && HasLocalRigBound())
        {
            PushLocalRigToNetworkedState();
            IsPushingLocalPose = true;

            if (!hasLoggedPushStart)
            {
                Debug.Log($"{LogTag} Local pose push started (InputAuthority=True).");
                hasLoggedPushStart = true;
            }
        }
        else
        {
            IsPushingLocalPose = false;
        }

        ApplyNetworkedStateToTargets();
    }

    private bool HasLocalRigBound()
    {
        return localHead != null || localLeftHand != null || localRightHand != null;
    }

    private void PushLocalRigToNetworkedState()
    {
        if (localHead != null)
        {
            HeadPos = localHead.position;
            HeadRot = localHead.rotation;
        }

        if (localLeftHand != null)
        {
            LeftPos = localLeftHand.position;
            LeftRot = localLeftHand.rotation;
        }

        if (localRightHand != null)
        {
            RightPos = localRightHand.position;
            RightRot = localRightHand.rotation;
        }
    }

    private void ApplyNetworkedStateToTargets()
    {
        if (headTarget != null)
        {
            headTarget.SetPositionAndRotation(HeadPos, HeadRot);
        }

        if (leftHandTarget != null)
        {
            leftHandTarget.SetPositionAndRotation(LeftPos, LeftRot);
        }

        if (rightHandTarget != null)
        {
            rightHandTarget.SetPositionAndRotation(RightPos, RightRot);
        }
    }
}
