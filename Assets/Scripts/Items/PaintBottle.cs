using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PaintBottle : GenericItem
{
    public Color paintColor = Color.red;
    [SerializeField] MeshRenderer liquid;
    float maxPaintAmount { get => (float)PaintPistol.maxPAmount * transform.lossyScale.x * transform.lossyScale.z / (PaintPistol.HopperScale.x * PaintPistol.HopperScale.y * PaintPistol.HopperScale.z) * (3 * transform.lossyScale.y - Mathf.Sqrt(transform.lossyScale.y * transform.lossyScale.z) / 2); }
    private int liquidPropertyID, liquidColorPropertyID;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        liquidPropertyID = Shader.PropertyToID("_Paint");
        liquidColorPropertyID = Shader.PropertyToID("_Paint_Color");
        UpdatePaint();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdatePaint()
    {
        if (liquid == null) return;
        liquid.material.SetFloat(liquidPropertyID, Mathf.Clamp(reloadQuantity / maxPaintAmount, 0, 1));
        liquid.material.SetColor(liquidColorPropertyID, paintColor);
    }
}
