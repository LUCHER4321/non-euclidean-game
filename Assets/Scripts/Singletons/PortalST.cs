using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PortalST : MonoBehaviour
{
    public static PortalST Instance { get; private set; }
    [SerializeField]
    Shader graph;
    [SerializeField]
    string inputName = "_Portal_Texture";
    [SerializeField]
    List<Light> incomingLights = new List<Light>();

    public Shader GetGraph { get => graph; }
    public string GetInputName { get => inputName; }
    public List<Light> GetIncomingLights { get => incomingLights; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
    }
}
