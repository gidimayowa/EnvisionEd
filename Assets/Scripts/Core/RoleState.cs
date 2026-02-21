using System;

public enum Role
{
    None,
    Student,
    Teacher
}

[Serializable]
public class RoleState
{
    public Role activeRole = Role.None;
}
