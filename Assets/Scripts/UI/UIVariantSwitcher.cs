using UnityEngine;

/*
Setup (per screen parent object):
1) Add this script to each screen root controlled by BootRouter.
2) Under that screen root, create:
   - XR_Root (world-space canvas)
   - Mobile_Root (screen-space overlay/camera canvas)
3) Assign xrUiRoot and mobileUiRoot.
4) BootRouter keeps enabling/disabling screen roots. This script only switches XR/Mobile variant inside the active screen.
*/
public sealed class UIVariantSwitcher : MonoBehaviour
{
    private const string LogTag = "[UIVariantSwitcher]";

    [SerializeField] private GameObject xrUiRoot;
    [SerializeField] private GameObject mobileUiRoot;

    private bool isSubscribed;
    private DeviceMode lastAppliedMode = (DeviceMode)(-1);

    private void OnEnable()
    {
        TrySubscribe();
        Apply();
    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Apply()
    {
        var appState = AppState.Instance;
        var mode = appState != null ? appState.CurrentDeviceMode : DeviceMode.Mobile_AR;
        var useXr = mode == DeviceMode.XR_HMD;

        var xrChanged = false;
        var mobileChanged = false;

        if (xrUiRoot != null)
        {
            if (xrUiRoot.activeSelf != useXr)
            {
                xrUiRoot.SetActive(useXr);
                xrChanged = true;
            }
        }

        if (mobileUiRoot != null)
        {
            if (mobileUiRoot.activeSelf != !useXr)
            {
                mobileUiRoot.SetActive(!useXr);
                mobileChanged = true;
            }
        }

        if (mode != lastAppliedMode || xrChanged || mobileChanged)
        {
            var xrActive = xrUiRoot != null && xrUiRoot.activeSelf;
            var mobileActive = mobileUiRoot != null && mobileUiRoot.activeSelf;
            Debug.Log($"{LogTag} Applied mode={mode} xrActive={xrActive} mobileActive={mobileActive}");
            lastAppliedMode = mode;
        }
    }

    private void HandleDeviceModeChanged(DeviceMode mode)
    {
        Apply();
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        var appState = AppState.Instance;
        if (appState == null)
        {
            return;
        }

        appState.DeviceModeChanged += HandleDeviceModeChanged;
        isSubscribed = true;
        Apply();
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
        {
            return;
        }

        if (AppState.Instance != null)
        {
            AppState.Instance.DeviceModeChanged -= HandleDeviceModeChanged;
        }

        isSubscribed = false;
    }
}
