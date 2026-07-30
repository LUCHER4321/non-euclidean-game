using UnityEngine;

public class PaintPistol : Item
{
    [SerializeField] int maxPaintAmount = 100;
    [SerializeField] PaintColor paintColor = new PaintColor(Color.red, 100);
    [SerializeField] MeshRenderer hopper;
    private Material hopperMaterial;
    private int hopperPropertyID, hopperColorPropertyID;

    [System.Serializable]
    public struct PaintColor
    {
        public Color color;
        public int quantity;

        public PaintColor(Color color, int quantity)
        {
            this.color = color;
            this.quantity = quantity;
        }

        public PaintColor(params PaintColor[] paints)
        {
            if (paints.Length == 0)
            {
                color = Color.white;
                quantity = 0;
                return;
            }
            int totalQuantity = 0;
            Vector3 cmy = Vector3.zero;
            float totalAlpha = 0f;
            foreach (PaintColor paint in paints)
            {
                totalQuantity += paint.quantity;
                Vector3 ncmy = Vector3.one - new Vector3(paint.color.r, paint.color.g, paint.color.b);
                cmy += ncmy * paint.quantity;
                totalAlpha += paint.color.a * paint.quantity;
            }
            if (totalQuantity == 0)
            {
                color = Color.white;
                quantity = 0;
                return;
            }
            cmy /= totalQuantity;
            totalAlpha /= totalQuantity;
            Vector3 rgb = Vector3.one - cmy;
            color = new Color(rgb.x, rgb.y, rgb.z, totalAlpha);
            quantity = totalQuantity;
        }
    }

    public override bool CanUse()
    {
        return paintColor.quantity > 0;
    }

    public override void Action(bool pressing) { }

    public override void Throw(bool pressing) { }

    public override bool HandleReload(Item item)
    {
        if (paintColor.quantity >= maxPaintAmount || !(item is PaintBottle)) return false;
        PaintBottle pb = (PaintBottle)item;
        bool b = true;
        int q = paintColor.quantity + Mathf.FloorToInt(item.reloadQuantity);
        if (q > maxPaintAmount)
        {
            item.reloadQuantity = q - maxPaintAmount;
            q = maxPaintAmount;
            b = false;
        }
        paintColor = new PaintColor(paintColor, new PaintColor(pb.paintColor, q - paintColor.quantity));
        UpdateMaterial();
        return b;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hopperPropertyID = Shader.PropertyToID("_Paint");
        hopperColorPropertyID = Shader.PropertyToID("_Paint_Color");
        if (hopper != null) hopperMaterial = hopper.material;
        UpdateMaterial();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateMaterial()
    {
        hopperMaterial.SetFloat(hopperPropertyID, Mathf.Clamp((float)paintColor.quantity / (float)maxPaintAmount, 0, 1));
        hopperMaterial.SetColor(hopperColorPropertyID, paintColor.color);
    }
}
