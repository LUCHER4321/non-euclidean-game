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
    [SerializeField]
    GameObject portalPrefab;
    [SerializeField]
    string exclusiveLayerName;

    public Shader GetGraph { get => graph; }
    public string GetInputName { get => inputName; }
    public LayerMask GetLayerMask { get => lMask; }
    public GameObject GetPortalPrefab { get => portalPrefab; }
    public string GetExclusiveLayerName { get => exclusiveLayerName; }

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
