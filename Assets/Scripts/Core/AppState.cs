using System;
using UnityEngine;

public sealed class AppState : MonoBehaviour
{
    public static AppState Instance { get; private set; }

    public UserProfile CurrentUserProfile { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CreateOrUpdateUserProfile(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        }

        if (CurrentUserProfile == null)
        {
            CurrentUserProfile = new UserProfile();
        }

        if (string.IsNullOrWhiteSpace(CurrentUserProfile.userId))
        {
            CurrentUserProfile.userId = Guid.NewGuid().ToString();
        }

        CurrentUserProfile.email = email.Trim();
    }
}
