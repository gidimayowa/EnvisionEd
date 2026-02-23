using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

/*
Setup (Menu scene):
1) Add this script to a persistent system object (same place as DeviceModeDetector / BootRouter).
2) Assign xrRigRoot to your Meta/OVR rig root (for example: [BuildingBlock] Camera Rig or OVRCameraRig root).
3) Create a non-XR camera (for example: MobileMainCamera), tag it MainCamera, and assign mobileCamera.
4) Ensure only one camera path is active per mode:
   - Mobile_AR => XR rig off, mobile camera on
   - XR_HMD    => XR rig on, mobile camera off
*/
public sealed class PlatformRigSwitcher : MonoBehaviour
{
    private const string LogTag = "[PlatformRigSwitcher]";

    [Header("Primary Rig References")]
    [SerializeField] private GameObject xrRigRoot;
    [SerializeField] private Camera mobileCamera;

    [Header("Optional Mode-Specific Objects")]
    [SerializeField] private GameObject[] xrOnlyObjects;
    [SerializeField] private GameObject[] mobileOnlyObjects;

    [Header("Optional UI Input Modules")]
    [SerializeField] private BaseInputModule xrUiInputModule;
    [SerializeField] private BaseInputModule mobileUiInputModule;
    [SerializeField] private EventSystem uiEventSystem;

    [Header("Mode-specific UI Routing")]
    [Tooltip("Meta/Oculus PointableCanvasModule component. Disabled in mobile mode.")]
    [SerializeField] private Behaviour pointableCanvasModule;
    [Tooltip("Input System UI module that should stay enabled in mobile mode.")]
    [SerializeField] private InputSystemUIInputModule inputSystemUiInputModule;
    [Tooltip("XR ray interactors / pointer behaviours to disable in mobile mode.")]
    [SerializeField] private Behaviour[] xrRayInteractors;
    [Tooltip("XR-only canvases to disable in mobile mode.")]
    [SerializeField] private Canvas[] xrOnlyCanvases;

    private bool isSubscribed;
    private Coroutine delayedStateLogCoroutine;

    private void OnEnable()
    {
        TrySubscribe();

        var mode = AppState.Instance != null ? AppState.Instance.CurrentDeviceMode : DeviceMode.Mobile_AR;
        Apply(mode);
        StartDelayedStateLog();
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
        if (delayedStateLogCoroutine != null)
        {
            StopCoroutine(delayedStateLogCoroutine);
            delayedStateLogCoroutine = null;
        }

        Unsubscribe();
    }

    public void Apply(DeviceMode mode)
    {
        var useXr = mode == DeviceMode.XR_HMD;
        var resolvedInputSystemUiModule = ResolveInputSystemUiModule();

        if (xrRigRoot != null)
        {
            xrRigRoot.SetActive(useXr);
        }
        else
        {
            Debug.LogWarning($"{LogTag} xrRigRoot is not assigned.");
        }

        SetObjectsActive(xrOnlyObjects, useXr, "xrOnlyObjects");
        SetModuleEnabled(xrUiInputModule, useXr, "xrUiInputModule");

        if (pointableCanvasModule != null)
        {
            pointableCanvasModule.enabled = useXr;
        }

        if (resolvedInputSystemUiModule != null)
        {
            // Required for non-XR/mobile button clicks.
            resolvedInputSystemUiModule.enabled = !useXr;
        }
        else
        {
            Debug.LogWarning($"{LogTag} No InputSystemUIInputModule resolved. Mobile UI clicks/typing may fail.");
        }

        SetBehavioursEnabled(xrRayInteractors, useXr, "xrRayInteractors");
        SetCanvasesEnabled(xrOnlyCanvases, useXr, "xrOnlyCanvases");

        if (mobileCamera != null)
        {
            // Never destroy/remove Camera components at runtime; only toggle enabled state.
            mobileCamera.enabled = !useXr;
            if (mobileCamera.gameObject.activeSelf != !useXr)
            {
                mobileCamera.gameObject.SetActive(!useXr);
            }
        }
        else
        {
            Debug.LogError($"{LogTag} mobileCamera is not assigned.");
        }

        SetObjectsActive(mobileOnlyObjects, !useXr, "mobileOnlyObjects");
        SetModuleEnabled(mobileUiInputModule, !useXr, "mobileUiInputModule");
        EnsureEventSystemReady(useXr);

        var xrRigActive = xrRigRoot != null && xrRigRoot.activeSelf;
        var mobileCameraActive = mobileCamera != null && mobileCamera.gameObject.activeSelf;
        Debug.Log($"{LogTag} Applied mode={mode} xrRigActive={xrRigActive} mobileCameraActive={mobileCameraActive}");
        LogActiveCameraState();
    }

    private void HandleDeviceModeChanged(DeviceMode mode)
    {
        Apply(mode);
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

        appState.DeviceModeChanged -= HandleDeviceModeChanged;
        appState.DeviceModeChanged += HandleDeviceModeChanged;
        isSubscribed = true;
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

    private void SetObjectsActive(GameObject[] objects, bool active, string fieldName)
    {
        if (objects == null)
        {
            return;
        }

        for (var i = 0; i < objects.Length; i++)
        {
            var obj = objects[i];
            if (obj == null)
            {
                Debug.LogWarning($"{LogTag} Null entry at {fieldName}[{i}].");
                continue;
            }

            obj.SetActive(active);
        }
    }

    private void LogActiveCameraState()
    {
        var main = Camera.main;
        var mainName = main != null ? main.name : "None";
        var mainActive = main != null && main.gameObject.activeInHierarchy;

        var mobileName = mobileCamera != null ? mobileCamera.name : "None";
        var mobileActive = mobileCamera != null && mobileCamera.gameObject.activeInHierarchy;
        var mobileCamEnabled = mobileCamera != null && mobileCamera.enabled;
        var eventSystemName = EventSystem.current != null ? EventSystem.current.name : "None";
        var mobileModuleEnabled = mobileUiInputModule != null && mobileUiInputModule.enabled;
        var xrModuleEnabled = xrUiInputModule != null && xrUiInputModule.enabled;
        var pointableEnabled = pointableCanvasModule != null && pointableCanvasModule.enabled;
        var resolvedInputSystemUiModule = ResolveInputSystemUiModule();
        var inputUiEnabled = resolvedInputSystemUiModule != null && resolvedInputSystemUiModule.enabled;

        Debug.Log($"{LogTag} Camera state -> main={mainName} (activeInHierarchy={mainActive}), mobileAssigned={mobileName} (activeInHierarchy={mobileActive}, enabled={mobileCamEnabled}), eventSystem={eventSystemName}, mobileModuleEnabled={mobileModuleEnabled}, xrModuleEnabled={xrModuleEnabled}, pointableEnabled={pointableEnabled}, inputSystemUiEnabled={inputUiEnabled}");
    }

    private InputSystemUIInputModule ResolveInputSystemUiModule()
    {
        if (inputSystemUiInputModule != null)
        {
            return inputSystemUiInputModule;
        }

        if (mobileUiInputModule is InputSystemUIInputModule mobileAsInputSystemUi)
        {
            return mobileAsInputSystemUi;
        }

        return null;
    }

    private void SetBehavioursEnabled(Behaviour[] behaviours, bool enabledState, string fieldName)
    {
        if (behaviours == null)
        {
            return;
        }

        for (var i = 0; i < behaviours.Length; i++)
        {
            var behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = enabledState;

            if (!behaviour.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"{LogTag} {fieldName}[{i}] is assigned but its GameObject is inactive: {behaviour.gameObject.name}");
            }
        }
    }

    private void SetCanvasesEnabled(Canvas[] canvases, bool enabledState, string fieldName)
    {
        if (canvases == null)
        {
            return;
        }

        for (var i = 0; i < canvases.Length; i++)
        {
            var canvas = canvases[i];
            if (canvas == null)
            {
                continue;
            }

            canvas.enabled = enabledState;

            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = enabledState;
            }

            if (!canvas.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"{LogTag} {fieldName}[{i}] is assigned but its GameObject is inactive: {canvas.gameObject.name}");
            }
        }
    }

    private void SetModuleEnabled(BaseInputModule module, bool enabledState, string fieldName)
    {
        if (module == null)
        {
            return;
        }

        module.enabled = enabledState;

        if (!module.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"{LogTag} {fieldName} is assigned but its GameObject is inactive: {module.gameObject.name}");
        }
    }

    private void EnsureEventSystemReady(bool useXr)
    {
        if (uiEventSystem == null)
        {
            return;
        }

        if (!uiEventSystem.gameObject.activeSelf)
        {
            uiEventSystem.gameObject.SetActive(true);
        }

        if (!uiEventSystem.enabled)
        {
            uiEventSystem.enabled = true;
        }

        if (!useXr)
        {
            // In mobile mode, EventSystem + mobile module must both be enabled for click/type.
            var resolvedInputSystemUiModule = ResolveInputSystemUiModule();
            if (resolvedInputSystemUiModule != null && !resolvedInputSystemUiModule.enabled)
            {
                resolvedInputSystemUiModule.enabled = true;
            }
        }
    }

    private void StartDelayedStateLog()
    {
        if (delayedStateLogCoroutine != null)
        {
            StopCoroutine(delayedStateLogCoroutine);
        }

        delayedStateLogCoroutine = StartCoroutine(LogStateNextFrame());
    }

    private IEnumerator LogStateNextFrame()
    {
        yield return null;
        LogActiveCameraState();
        delayedStateLogCoroutine = null;
    }

}
