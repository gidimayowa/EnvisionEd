using System;
using System.IO;
using UnityEngine;

public sealed class AppState : MonoBehaviour
{
    public static AppState Instance { get; private set; }

    public UserProfile User { get; private set; }
    public RoleState Role { get; private set; }
    public SessionDraft Session { get; private set; }

    // Runtime-only mode flag (NOT persisted to JSON).
    public DeviceMode CurrentDeviceMode { get; private set; } = DeviceMode.Mobile_AR;
    public event Action<DeviceMode> DeviceModeChanged;

    // Single JSON file under Unity's persistent app data folder.
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "app_state.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaults();
        LoadFromDisk();
    }

    public void SetDeviceMode(DeviceMode mode)
    {
        if (CurrentDeviceMode == mode)
        {
            return;
        }

        CurrentDeviceMode = mode;
        Debug.Log($"[AppState] Device mode set to: {mode}");
        DeviceModeChanged?.Invoke(mode);
    }

    public void SetEmail(string email)
    {
        Debug.Log($"SetEmail called with: '{email}'");

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (User == null)
        {
            User = new UserProfile();
        }

        if (string.IsNullOrWhiteSpace(User.userId))
        {
            User.userId = Guid.NewGuid().ToString();
        }

        User.email = email.Trim();
        SaveToDisk();
    }

    public void SetRole(Role role)
    {
        Role.activeRole = role;
        Debug.Log("Role set to: " + role);
        SaveToDisk();
    }

    public void SetSessionAsHost(string classNameOrCode)
    {
        Session.isHost = true;
        Session.classCodeOrName = Clean(classNameOrCode);
        Session.classId = string.Empty;
        Session.sessionId = string.Empty;
        SaveToDisk();
    }

    public void SetSessionAsClient(string classCode)
    {
        Session.isHost = false;
        Session.classCodeOrName = Clean(classCode);
        Session.classId = string.Empty;
        Session.sessionId = string.Empty;
        SaveToDisk();
    }

    public void ResetSession()
    {
        Session = new SessionDraft();
        SaveToDisk();
    }

    public void SignOut()
    {
        User = null;
        Role = new RoleState();
        Session = new SessionDraft();
        SaveToDisk();
    }

    public void SaveToDisk()
    {
        // Persist only plain serializable data (no MonoBehaviour references).
        // CurrentDeviceMode intentionally omitted (runtime-only).
        var data = new AppStateData
        {
            user = User,
            role = Role,
            session = Session
        };

        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFilePath, json);
    }

    public void LoadFromDisk()
    {
        if (!File.Exists(SaveFilePath))
        {
            InitializeDefaults();
            return;
        }

        var json = File.ReadAllText(SaveFilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            InitializeDefaults();
            return;
        }

        var data = JsonUtility.FromJson<AppStateData>(json);
        if (data == null)
        {
            InitializeDefaults();
            return;
        }

        // Restore known app state, with safe fallbacks for missing sections.
        User = data.user;
        Role = data.role ?? new RoleState();
        Session = data.session ?? new SessionDraft();
    }

    public void ClearSavedData()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
        }
    }

    private void InitializeDefaults()
    {
        User = null;
        Role = new RoleState();
        Session = new SessionDraft();
    }

    private static string Clean(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
