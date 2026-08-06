using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ItemST : MonoBehaviour
{
    public static ItemST Instance { get; private set; }
    [SerializeField] Material paintDecal;
    [SerializeField] GameObject paintDecalPrefab;
    [SerializeField] Material bulletDecal;
    [SerializeField] GameObject bulletDecalPrefab;
    public float epsilon = 0.01f;

    public DecalProjector PaintStain(Color color, Vector3 position, Quaternion rotation)
    {
        GameObject spawnedProjector = Instantiate(paintDecalPrefab, position, rotation);
        DecalProjector spawnedDecalProjector = spawnedProjector.GetComponent<DecalProjector>();
        if (spawnedDecalProjector == null) return null;
        Material stainMaterial = new Material(paintDecal);
        stainMaterial.SetColor("_Color", color);
        spawnedDecalProjector.material = stainMaterial;
        return spawnedDecalProjector;
    }

    public DecalProjector BulletMark(Vector3 position, Quaternion rotation)
    {
        GameObject spawnedProjector = Instantiate(bulletDecalPrefab, position, rotation);
        DecalProjector spawnedDecalProjector = spawnedProjector.GetComponent<DecalProjector>();
        if (spawnedDecalProjector == null) return null;
        Material stainMaterial = new Material(bulletDecal);
        spawnedDecalProjector.material = stainMaterial;
        return spawnedDecalProjector;
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
