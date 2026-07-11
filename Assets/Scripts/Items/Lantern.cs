using UnityEngine;
using System.Collections;

public class Lantern : Item
{
    [SerializeField]
    Light lanternLight;
    [SerializeField]
    float maxEnergy = 100, energy = 100, energyTime = 3600;
    [SerializeField]
    bool turnedOn;
    [SerializeField]
    Material battery;
    private int batteryPropertyID;
    private Coroutine drainCoroutine;

    public override bool CanUse() {
        return energy > 0;
    }

    public override void Action(bool pressing)
    {if (!pressing) 
        {
            turnedOn = !turnedOn;
            if (turnedOn && CanUse())
            {
                lanternLight.enabled = true;
                if (drainCoroutine != null) StopCoroutine(drainCoroutine);
                drainCoroutine = StartCoroutine(DrainEnergyRoutine());
            }
            else TurnOffLantern();
        }
    }

    public override void Throw(bool pressing){}

    public override void Reload(){}

    void Awake()
    {
        batteryPropertyID = Shader.PropertyToID("_Battery");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            battery.SetFloat(batteryPropertyID, energy / maxEnergy);
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
}
