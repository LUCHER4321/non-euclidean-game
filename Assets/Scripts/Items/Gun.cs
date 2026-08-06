using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Gun : Item
{
    [SerializeField] int bullets = 10;
    [SerializeField] int maxBullets = 10;
    [SerializeField] float range = 50f;
    [SerializeField] float damage = 90f;

    public override bool CanUse()
    {
        return bullets > 0;
    }

    public override void Action(bool pressing)
    {
        if (pressing) return;
        RaycastHit hit;
        if (!Portal.Raycast(new Ray(owner.cam.transform.position, owner.cam.transform.forward), out hit, range)) return;
        bullets--;
        DecalProjector projector = ItemST.Instance.BulletMark(hit.point + ItemST.Instance.epsilon * hit.normal, Quaternion.LookRotation(-hit.normal));
        if (projector == null) return;
        projector.transform.parent = hit.transform;
        if (hit.collider.TryGetComponent(out Character character)) character.health -= damage;
    }

    public override void Throw(bool pressing) { }

    public override bool HandleReload(Item item)
    {
        if (bullets >= maxBullets) return false;
        bullets += Mathf.FloorToInt(item.reloadQuantity);
        if (bullets > maxBullets)
        {
            item.reloadQuantity = bullets - maxBullets;
            bullets = maxBullets;
            return false;
        }
        return true;
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
