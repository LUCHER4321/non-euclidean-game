using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [SerializeField]
    float moveSpeed = 5f;
    [SerializeField]
    float runSpeed = 10f;
    [SerializeField]
    float jumpHeight = 0.5f;
    [SerializeField]
    float height = 1f;
    [SerializeField]
    Vector2 limit = new Vector2(-90f, 90f);
    [SerializeField]
    float visionLength = 1000f;
    [SerializeField]
    float visionAngle = 60f;
    public float GetMoveSpeed { get => moveSpeed; }
    public float GetRunSpeed { get => runSpeed; }
    public float GetJumpHeight { get => jumpHeight; }
    public float GetHeight { get => height; }
    public Vector2 GetLimit { get => limit; }
    public float GetVisionLength { get => visionLength; }
    public float GetVisionAngle { get => visionAngle; }
}
