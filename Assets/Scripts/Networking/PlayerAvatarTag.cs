using UnityEngine;
using Fusion;

/*
Setup (Class 1 scene):
1) Create PlayerAvatar prefab: Capsule + NetworkObject + NetworkTransform.
2) Add this script to the same prefab for easier debug naming.
3) Register PlayerAvatar prefab in Fusion NetworkProjectConfig > Prefabs.
*/
public sealed class PlayerAvatarTag : NetworkBehaviour
{
    private const string LogTag = "[PlayerAvatarTag]";

    public override void Spawned()
    {
        var authority = Object != null && Object.HasInputAuthority;
        var runnerPlayer = Runner != null ? Runner.LocalPlayer.PlayerId.ToString() : "n/a";
        Debug.Log($"{LogTag} Spawned avatar '{name}'. InputAuthority={authority} LocalPlayer={runnerPlayer}");
    }
}
