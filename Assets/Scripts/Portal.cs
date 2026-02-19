using UnityEngine;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    [SerializeField]
    Portal linkedPortal;
    [SerializeField]
    Camera cam;
    [SerializeField]
    bool teleport = true;
    [SerializeField]
    Shader sGraph;
    private static Shader sStaticGraph;
    static string inputName = "_Portal_Texture";
    private RenderTexture rt;
    private Dictionary<Collider, int> teleportingObjects = new Dictionary<Collider, int>();

    void Awake()
    {
        if (sGraph != null) sStaticGraph = sGraph;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (linkedPortal == null)
        {
            Portal[] portals = FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (Portal portal in portals)
            {
                if (portal.linkedPortal == this)
                {
                    linkedPortal = portal;
                    break;
                }
            }
        }
        if (sStaticGraph == null) return;
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        cam.targetTexture = rt;
        Material newMaterial = new Material(sStaticGraph);
        newMaterial.SetTexture(inputName, rt);
        Renderer rndr = linkedPortal.GetComponent<Renderer>();
        if (rndr != null) rndr.material = newMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        cam.aspect = Camera.main.aspect;
        RotateCamera();
        TranslateCamera();
    }

    void RotateCamera()
    {
        if (linkedPortal == null || linkedPortal.cam == null) return;
        Quaternion direction = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;
        linkedPortal.cam.transform.localEulerAngles = direction.eulerAngles + 180 * Vector3.up;
    }

    void TranslateCamera()
    {
        if (linkedPortal == null || cam == null) return;
        Vector3 offset = transform.InverseTransformPoint(Camera.main.transform.position);
        linkedPortal.cam.transform.localPosition = new Vector3(-offset.x, offset.y, -offset.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!teleport) return;
        if (linkedPortal == null) return;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        Vector3 fromPortal = transform.InverseTransformPoint(rb.transform.position);
        bool inThis = teleportingObjects.ContainsKey(other);
        bool inLinked = linkedPortal.teleportingObjects.ContainsKey(other);
        Character character = other.GetComponent<Character>();
        bool isCharacter = character != null;
        if (!inThis && !inLinked && fromPortal.z <= 0.02f)
        {
            int originalLayer = other.gameObject.layer;
            Vector3 localPos = transform.InverseTransformPoint(rb.position);
            Quaternion localRot = Quaternion.Inverse(transform.rotation) * rb.rotation;
            Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);
            Vector3 localAngVel = transform.InverseTransformDirection(rb.angularVelocity);
            Vector3 newPos = linkedPortal.transform.TransformPoint(new Vector3(-localPos.x, localPos.y, -localPos.z));
            Quaternion newRot = linkedPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * localRot;
            Vector3 newVel = linkedPortal.transform.TransformDirection(new Vector3(-localVel.x, localVel.y, -localVel.z));
            Vector3 newAngVel = linkedPortal.transform.TransformDirection(new Vector3(-localAngVel.x, localAngVel.y, -localAngVel.z));
            rb.position = newPos;
            rb.rotation = isCharacter ? Quaternion.Euler(newRot.eulerAngles.y * Vector3.up) : newRot;
            rb.linearVelocity = newVel;
            rb.angularVelocity = newAngVel;
            teleportingObjects[other] = originalLayer;
            linkedPortal.teleportingObjects[other] = originalLayer;
        }
        else if (!inThis)
        {
            teleportingObjects[other] = other.gameObject.layer;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!teleport) return;
        if (teleportingObjects.ContainsKey(other))
        {
            teleportingObjects.Remove(other);
        }
    }
}
