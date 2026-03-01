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
    string matrixName = "_PortalAlignmentMatrix";
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
    public string GetMatrixName { get => matrixName; }

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

    void Start()
    {
        StartCoroutine(SyncWallShaders());
    }

    private IEnumerator SyncWallShaders()
    {
        Portal[] allPortals = FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        GameObject[] allWalls = GameObject.FindGameObjectsWithTag("Wall");
        HashSet<Portal> processedPortals = new HashSet<Portal>();
        foreach (Portal rootPortal in allPortals)
        {
            if (processedPortals.Contains(rootPortal) || rootPortal.linkedPortal == null) continue;
            Portal linkedPortal = rootPortal.linkedPortal;
            processedPortals.Add(rootPortal);
            processedPortals.Add(linkedPortal);
            Matrix4x4 spaceMatrix = rootPortal.transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix;
            foreach (GameObject wall in allWalls)
            {
                Renderer wallRend = wall.GetComponent<Renderer>();
                if (wallRend == null) continue;
                float rootDist = Vector3.Distance(wall.transform.position, rootPortal.transform.position);
                float linkedDist = Vector3.Distance(wall.transform.position, linkedPortal.transform.position);
                if (linkedDist < rootDist)
                {
                    wallRend.material.SetMatrix(matrixName, spaceMatrix);
                }
                else if (rootDist < linkedDist)
                {
                    wallRend.material.SetMatrix(matrixName, Matrix4x4.identity);
                }
                yield return null;
            }
        }
    }
}
