using UnityEngine;

[CreateAssetMenu(fileName = "LiquidSO", menuName = "Scriptable Objects/LiquidSO")]
public class LiquidSO : ScriptableObject
{
    [SerializeField] float wobbleSpeed = 5f;
    [SerializeField] float maxWobble = 0.5f;
    [SerializeField] float recoverySpeed = 3f;

    public float GetWobbleSpeed { get => wobbleSpeed; }
    public float GetMaxWobble { get => maxWobble; }
    public float GetRecoverySpeed { get => recoverySpeed; }
}
