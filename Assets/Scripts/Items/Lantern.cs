using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Lantern : Item
{
    [SerializeField] Light lanternLight;
    [SerializeField] float maxEnergy = 100, energy = 100, energyTime = 3600;
    [SerializeField] bool turnedOn;
    [SerializeField] DecalProjector battery;
    private Material batteryMaterial;
    private int batteryPropertyID;
    private Coroutine drainCoroutine;

    public override bool CanUse()
    {
        return energy > 0;
    }

    public override void Action(bool pressing)
    {
        if (pressing) return;
        turnedOn = !turnedOn;
        if (turnedOn && CanUse())
        {
            lanternLight.enabled = true;
            if (drainCoroutine != null) StopCoroutine(drainCoroutine);
            drainCoroutine = StartCoroutine(DrainEnergyRoutine());
        }
        else TurnOffLantern();
    }

    public override void Throw(bool pressing) { }

    public override bool HandleReload(Item item)
    {
        if (energy >= maxEnergy) return false;
        energy += item.reloadQuantity;
        UpdateMaterial();
        return true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        batteryPropertyID = Shader.PropertyToID("_Battery");
        if (battery != null)
        {
            batteryMaterial = new Material(battery.material);
            battery.material = batteryMaterial;
        }
        UpdateMaterial();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator DrainEnergyRoutine()
    {
        while (turnedOn && energy > 0)
        {
            energy -= Time.deltaTime * maxEnergy / energyTime;
            UpdateMaterial();
            yield return null;
        }
        if (energy <= 0) TurnOffLantern();
    }

    private void TurnOffLantern()
    {
        turnedOn = false;
        lanternLight.enabled = false;
        if (drainCoroutine != null)
        {
            StopCoroutine(drainCoroutine);
            drainCoroutine = null;
        }
    }

    void UpdateMaterial()
    {
        batteryMaterial.SetFloat(batteryPropertyID, Mathf.Clamp(energy / maxEnergy, 0, 1));
    }
}
