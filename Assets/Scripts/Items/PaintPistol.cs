using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PaintPistol : Item
{
    [SerializeField] int maxPaintAmount = 100;
    [SerializeField] float range = 50f;
    [SerializeField] float epsilon = 0.01f;
    [SerializeField] PaintColor paintColor = new PaintColor(Color.red, 100);
    [SerializeField] MeshRenderer hopper;
    private Material hopperMaterial;
    private int hopperPropertyID, hopperColorPropertyID;
    public static Vector3 HopperScale { get; private set; }
    public static int maxPAmount { get; private set; }

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

        private PaintColor(params PaintColor[] paints)
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

        private PaintColor(PaintColor paintColor, int factor)
        {
            color = paintColor.color;
            quantity = paintColor.quantity * factor;
        }

        public static PaintColor operator +(PaintColor a, PaintColor b)
        {
            return new PaintColor(a, b);
        }

        public static PaintColor operator *(PaintColor paint, int factor)
        {
            return new PaintColor(paint, factor);
        }

        public static PaintColor operator *(int factor, PaintColor paint)
        {
            return new PaintColor(paint, factor);
        }
    }

    public override bool CanUse()
    {
        return paintColor.quantity > 0;
    }

    public override void Action(bool pressing)
    {
        if (pressing) return;
        RaycastHit hit;
        if (!Portal.Raycast(new Ray(owner.cam.transform.position, owner.cam.transform.forward), out hit, range)) return;
        DecalProjector projector = ItemST.Instance.PaintStain(paintColor.color, hit.point + epsilon * hit.normal, Quaternion.LookRotation(-hit.normal));
        if (projector == null) return;
        paintColor += new PaintColor(paintColor.color, -1);
        projector.transform.parent = hit.transform;
    }

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
            pb.UpdatePaint();
            b = false;
        }
        paintColor += new PaintColor(pb.paintColor, q - paintColor.quantity);
        UpdateMaterial();
        return b;
    }

    void Awake()
    {
        if (maxPaintAmount > maxPAmount && hopper != null)
        {
            maxPAmount = maxPaintAmount;
            HopperScale = hopper.transform.lossyScale;
        }
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
