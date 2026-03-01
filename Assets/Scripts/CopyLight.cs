using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CopyLight : MonoBehaviour
{
    [SerializeField]
    Light originalLight;
    [SerializeField]
    Light lightPrefab;
    private Dictionary<Portal, Light> lights;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lights = new Dictionary<Portal, Light>();
        StartCoroutine(CreateCopies());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator CreateCopies()
    {
        Portal[] portals = FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Portal portal in portals)
        {
            if (portal.linkedPortal == null) continue;
            Light copy = Instantiate(lightPrefab);
            copy.range = originalLight.range;
            copy.spotAngle = originalLight.spotAngle;
            lights[portal] = copy;
            StartCoroutine(MoveCopy(portal));
            yield return null;
        }
    }

    IEnumerator MoveCopy(Portal portal)
    {
        while (lights.ContainsKey(portal))
        {
            Light light = lights[portal];
            float dist = Vector3.Distance(light.transform.position, transform.position);
            float cos = Vector3.Dot(light.transform.forward, transform.position - light.transform.position) / dist;
            light.enabled = cos > Mathf.Cos(light.spotAngle * Mathf.Deg2Rad) && dist < light.range;
            if (!light.enabled) continue;
            Vector3 offset = transform.InverseTransformPoint(light.transform.position);
            light.transform.position = portal.linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
            light.transform.rotation = portal.transform.rotation * Quaternion.Inverse(portal.linkedPortal.transform.rotation) * light.transform.rotation * Quaternion.Euler(0, 180, 0);
            yield return null;
        }
    }
}
