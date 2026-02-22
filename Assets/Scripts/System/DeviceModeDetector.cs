using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/*
Setup:
1) Add this to a scene object in Menu scene (same startup scene as BootRouter/AppState).
2) Keep it active at startup so it sets AppState.CurrentDeviceMode before UI flow proceeds.
3) Optional: place on an object that initializes early. This script uses DefaultExecutionOrder to run before most UI logic.
*/
[DefaultExecutionOrder(-500)]
public sealed class DeviceModeDetector : MonoBehaviour
{
    private const string LogTag = "[DeviceModeDetector]";

    [Header("Detection Retry")]
    [SerializeField, Min(0f)] private float retryDurationSeconds = 3f;
    [SerializeField, Min(0.05f)] private float retryIntervalSeconds = 0.25f;

    private readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>();
    private Coroutine detectCoroutine;

    private void Awake()
    {
        StartDetection();
    }

    private void Start()
    {
        // Safety pass for cases where AppState initializes slightly later in scene startup order.
        StartDetection();
    }

    private void OnDisable()
    {
        if (detectCoroutine != null)
        {
            StopCoroutine(detectCoroutine);
            detectCoroutine = null;
        }
    }

    private void StartDetection()
    {
        if (detectCoroutine != null)
        {
            StopCoroutine(detectCoroutine);
        }

        detectCoroutine = StartCoroutine(DetectModeWithRetries());
    }

    private IEnumerator DetectModeWithRetries()
    {
        var endTime = Time.realtimeSinceStartup + retryDurationSeconds;
        var appliedXr = false;
        var lastLoader = false;
        var lastDisplay = false;

        do
        {
            var appState = AppState.Instance;
            if (appState == null)
            {
                yield return new WaitForSecondsRealtime(retryIntervalSeconds);
                continue;
            }

            var xrLoaderActive = XRGeneralSettings.Instance != null
                                 && XRGeneralSettings.Instance.Manager != null
                                 && XRGeneralSettings.Instance.Manager.activeLoader != null;

            displays.Clear();
            SubsystemManager.GetSubsystems(displays);

            var displayRunning = false;
            for (var i = 0; i < displays.Count; i++)
            {
                if (displays[i] != null && displays[i].running)
                {
                    displayRunning = true;
                    break;
                }
            }

            lastLoader = xrLoaderActive;
            lastDisplay = displayRunning;

            if (xrLoaderActive && displayRunning)
            {
                appState.SetDeviceMode(DeviceMode.XR_HMD);
                Debug.Log($"{LogTag} Detected mode=XR_HMD (activeLoader={xrLoaderActive}, displayRunning={displayRunning}).");
                appliedXr = true;
                break;
            }

            yield return new WaitForSecondsRealtime(retryIntervalSeconds);
        }
        while (Time.realtimeSinceStartup < endTime);

        var finalAppState = AppState.Instance;
        if (finalAppState == null)
        {
            detectCoroutine = null;
            yield break;
        }

        if (!appliedXr)
        {
            // Fallback: some runtimes report activeLoader before displayRunning becomes visible to this check.
            var fallbackMode = lastLoader ? DeviceMode.XR_HMD : DeviceMode.Mobile_AR;
            finalAppState.SetDeviceMode(fallbackMode);
            Debug.Log($"{LogTag} Detected mode={fallbackMode} (activeLoader={lastLoader}, displayRunning={lastDisplay}) after retries.");
        }

        detectCoroutine = null;
    }
}
