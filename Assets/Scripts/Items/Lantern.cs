using UnityEngine;

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

    public override bool CanUse() {
        return energy > 0;
    }

    public override void Action(bool pressing)
    {
        if(!pressing) turnedOn = !turnedOn;
        lanternLight.enabled = turnedOn;
    }

    public override void Throw(bool pressing){}

    public override void Reload(){}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(energy <= 0)
        {
            turnedOn = false;
            lanternLight.enabled = false;
        }
        if(turnedOn) energy -= Time.deltaTime * maxEnergy / energyTime;
        battery.SetFloat("_Battery", energy / maxEnergy);
    }
}
