using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [Header("Moving Stats")]
    [SerializeField, Min(0f)]
    float moveSpeed = 5f;
    [SerializeField, Min(0f)]
    float runSpeed = 10f;
    [SerializeField, Min(0f)]
    float jumpHeight = 0.5f;
    [SerializeField, Min(0f)]
    float height = 1f;
    [SerializeField]
    Vector2 limit = new Vector2(-90f, 90f);
    [Header("Vision Stats")]
    [SerializeField, Min(0f)]
    float visionLength = 1000f;
    [SerializeField, Min(0f)]
    float visionAngle = 60f;
    [SerializeField]
    CharacterSO[] preys;
    public float GetMoveSpeed { get => moveSpeed; }
    public float GetRunSpeed { get => runSpeed; }
    public float GetJumpHeight { get => jumpHeight; }
    public float GetHeight { get => height; }
    public Vector2 GetLimit { get => limit; }
    public float GetVisionLength { get => visionLength; }
    public float GetVisionAngle { get => visionAngle; }
    public CharacterSO[] GetPreys { get => preys; }
}
