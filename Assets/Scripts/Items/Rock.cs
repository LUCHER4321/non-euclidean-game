using UnityEngine;

public class Rock : Item
{
    [SerializeField] float meleeRange = 0.6f;
    [SerializeField] float meleeDamage = 20f;
    [SerializeField] float throwingDamageFactor = 2.5f;

    public override bool CanUse()
    {
        return true;
    }

    public override void Action(bool pressing)
    {
        if (pressing) return;
        RaycastHit hit;
        if (!Portal.Raycast(new Ray(owner.cam.transform.position, owner.cam.transform.forward), out hit, meleeRange)) return;
        if (hit.collider.TryGetComponent(out Character character)) character.health -= meleeDamage;
    }

    public override void Throw(bool pressing)
    {
        if (pressing) return;
        Character c = owner;
        Drop();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = c.rb.linearVelocity + c.cam.transform.forward * c.characterSO.GetThrowingMomentum / rb.mass;
    }

    public override bool HandleReload(Item item)
    {
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 hitVelocity = rb.linearVelocity - other.attachedRigidbody.linearVelocity;
        if (rb.linearVelocity.sqrMagnitude > 0 && Vector3.Dot(hitVelocity, transform.position - other.transform.position) < 0 && other.TryGetComponent(out Character character)) character.health -= hitVelocity.magnitude * throwingDamageFactor;
    }
}
