using UnityEngine;

/*
Setup (Class 1 scene):
1) Create a GameObject, e.g. "XR Rig Reference".
2) Add this script.
3) Assign your local Meta rig transforms:
   - head -> CenterEyeAnchor
   - leftHand -> LeftHandAnchor
   - rightHand -> RightHandAnchor
4) Assign this component to PlayerSpawner.localRigReferences.
*/
public sealed class LocalRigReferences : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    public Transform Head => head;
    public Transform LeftHand => leftHand;
    public Transform RightHand => rightHand;
}
