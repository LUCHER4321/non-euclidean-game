using UnityEngine;

public class PortalST : MonoBehaviour
{
    public static PortalST Instance { get; private set; }
    [SerializeField]
    Shader graph;
    [SerializeField]
    string inputName = "_Portal_Texture";
    [SerializeField]
    LayerMask lMask;

    public Shader GetGraph { get => graph; }
    public string GetInputName { get => inputName; }
    public LayerMask GetLayerMask { get => lMask; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}
