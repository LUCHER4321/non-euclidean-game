using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    [SerializeField]
    Portal auxiliaryPortal;
    [SerializeField]
    Camera cam;
    [SerializeField]
    bool teleport = true;
    private RenderTexture rt;
    private Dictionary<Collider, GameObject> copies;

    void Awake()
    {
        copies = new Dictionary<Collider, GameObject>();
        if (auxiliaryPortal != null)
        {
            auxiliaryPortal.linkedPortal = linkedPortal.auxiliaryPortal;
        }
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
        if (PortalST.Instance.GetGraph == null) return;
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        cam.targetTexture = rt;
        Material newMaterial = new Material(PortalST.Instance.GetGraph);
        newMaterial.SetTexture(PortalST.Instance.GetInputName, rt);
        Renderer rndr = linkedPortal != null ? linkedPortal.GetComponent<Renderer>() : null;
        if (rndr != null) rndr.material = newMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        cam.aspect = Camera.main.aspect;
        RotateCamera();
        TranslateCamera();
        //SetCameraNear();
    }

    public Ray RedirectRay(Vector3 hitPoint, Vector3 incomingDirection)
    {
        if (!teleport || linkedPortal == null) return new Ray(hitPoint, incomingDirection);
        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
        Vector3 outOrigin = linkedPortal.transform.TransformPoint(new Vector3(-localHitPoint.x, localHitPoint.y, -localHitPoint.z));
        Vector3 localDirection = transform.InverseTransformDirection(incomingDirection);
        Vector3 outDirection = linkedPortal.transform.TransformDirection(new Vector3(-localDirection.x, localDirection.y, -localDirection.z));
        return new Ray(outOrigin, outDirection);
    }

    void RotateCamera()
    {
        if (linkedPortal == null || linkedPortal.cam == null) return;
        Quaternion direction = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;
        linkedPortal.cam.transform.localEulerAngles = direction.eulerAngles + 180 * Vector3.up;
    }

    void TranslateCamera()
    {
        if (linkedPortal == null || cam == null || !teleport) return;
        Vector3 offset = transform.InverseTransformPoint(Camera.main.transform.position);
        linkedPortal.cam.transform.localPosition = new Vector3(-offset.x, offset.y, -offset.z);
        linkedPortal.auxiliaryPortal.cam.transform.position = linkedPortal.cam.transform.position;
    }

    void SetCameraNear()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;
        Mesh mesh = meshFilter.sharedMesh;
        if (mesh == null) return;
        List<Vector3> localVertices = new List<Vector3>();
        mesh.GetVertices(localVertices);
        float minDistance = float.MaxValue;
        foreach (Vector3 v in localVertices)
        {
            Vector3 worldVertex = transform.TransformPoint(v);
            float distance = Vector3.Distance(cam.transform.position, worldVertex);
            float linkedDistance = Vector3.Distance(linkedPortal.cam.transform.position, worldVertex);
            float distanceToUse = Mathf.Min(distance, linkedDistance);
            if (distanceToUse < minDistance) minDistance = distanceToUse;
        }
        cam.nearClipPlane = minDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!teleport || linkedPortal == null || !linkedPortal.teleport || copies.ContainsKey(other) || copies.ContainsValue(other.gameObject) || other.gameObject.name.Contains("Copy")) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        Vector3 offset = transform.InverseTransformPoint(other.transform.position);
        GameObject copy = Instantiate(other.gameObject, linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z)), linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation);
        copy.name = other.gameObject.name + " Copy";
        copy.GetComponent<Collider>().enabled = false;
        copy.GetComponent<Rigidbody>().useGravity = false;
        foreach (AudioListener listener in copy.GetComponentsInChildren<AudioListener>())
        {
            listener.enabled = false;
        }
        foreach (Camera camera in copy.GetComponentsInChildren<Camera>())
        {
            camera.enabled = false;
        }
        copies.Add(other, copy);
        StartCoroutine(MoveCopy(other));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!teleport || !copies.ContainsKey(other) || linkedPortal == null || !linkedPortal.teleport || copies.ContainsValue(other.gameObject)) return;
        GameObject copy = copies[other];
        copies.Remove(other);
        Destroy(copy.gameObject);
        Vector3 offset = transform.InverseTransformPoint(other.transform.position);
        Vector3 targetPosition = linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
        Quaternion targetRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation * Quaternion.Euler(0, 180, 0);
        bool portalSide = Vector3.Dot(other.transform.position - transform.position, transform.forward) < 0;
        if (portalSide)
        {
            other.transform.position = targetPosition;
            other.transform.rotation = targetRotation;
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Quaternion relativeRot = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0, 180, 0);
                rb.linearVelocity = relativeRot * rb.linearVelocity;
                rb.angularVelocity = relativeRot * rb.angularVelocity;
            }
        }
    }

    IEnumerator MoveCopy(Collider other)
    {
        while (copies.ContainsKey(other) && copies[other] != null)
        {
            Vector3 offset = transform.InverseTransformPoint(other.transform.position);
            Vector3 targetPosition = linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
            Quaternion targetRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation * Quaternion.Euler(0, 180, 0);
            copies[other].transform.position = targetPosition;
            copies[other].transform.rotation = targetRotation;
            yield return null;
        }
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    void OnDrawGizmos()
    {
        if (!teleport) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, cam.transform.position);
    }
}
