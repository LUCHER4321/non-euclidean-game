using UnityEngine;

[CreateAssetMenu(fileName = "State", menuName = "Scriptable Objects/State Machines/State")]
public class State : ScriptableObject
{
    [SerializeField]
    string stateName;
    public string GetStateName { get => stateName; }
}
