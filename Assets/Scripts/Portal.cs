using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    public Portal linkedPortal;
    [SerializeField]
    Camera cam;
    [SerializeField]
    bool teleport = true;
    private RenderTexture rt;
    private Dictionary<Collider, GameObject> copies;

    void Awake()
    {
        copies = new Dictionary<Collider, GameObject>();
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
        if (rndr != null && linkedPortal.teleport) rndr.material = newMaterial;
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
        copies.Add(other, copy);
        StartCoroutine(MoveCopy(other));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!teleport || !copies.ContainsKey(other) || linkedPortal == null || !linkedPortal.teleport || copies.ContainsValue(other.gameObject)) return;
        GameObject copy = copies[other];
        copies.Remove(other);
        Destroy(copy.gameObject);
        foreach (Camera c in other.GetComponentsInChildren<Camera>())
        {
            c.enabled = true;
        }
    }

    IEnumerator MoveCopy(Collider other)
    {
        Camera[] cameras = other.GetComponentsInChildren<Camera>();
        Camera[] camerasC = copies[other].GetComponentsInChildren<Camera>();
        Dictionary<Camera, Camera> copyCameras = new Dictionary<Camera, Camera>();
        Dictionary<Transform, Camera> transforms = new Dictionary<Transform, Camera>();
        int count = 0;
        foreach (Camera c in cameras)
        {
            transforms.Add(c.transform, c);
            copyCameras.Add(c, camerasC[count]);
            count++;
        }
        int portalSideOld = Math.Sign(Vector3.Dot(other.transform.position - transform.position, transform.forward));
        while (copies.ContainsKey(other) && copies[other] != null)
        {
            foreach (Transform t in transforms.Keys)
            {
                Vector3 dirToPortal = transform.position - t.position;
                bool inFrontOfCamera = Vector3.Dot(t.forward, dirToPortal) > 0;
                bool onFrontSideOfPortal = Vector3.Dot(transform.forward, -dirToPortal) > 0;
                bool lookingAtPortal = inFrontOfCamera && onFrontSideOfPortal;
                transforms[t].enabled = lookingAtPortal;
                copyCameras[transforms[t]].enabled = !lookingAtPortal;
            }
            Vector3 offset = transform.InverseTransformPoint(other.transform.position);
            Vector3 targetPosition = linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
            Quaternion targetRotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation * Quaternion.Euler(0, 180, 0);
            copies[other].transform.position = targetPosition;
            copies[other].transform.rotation = targetRotation;
            int portalSide = Math.Sign(Vector3.Dot(other.transform.position - transform.position, transform.forward));
            if (portalSide != portalSideOld)
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
                portalSideOld = portalSide;
            }

            yield return null;
        }
    }
    void SetObliqueNearClipPlane(Camera playerCam, Camera portalCam, Transform destinationPortal)
    {
        Plane portalPlane = new Plane(destinationPortal.forward, destinationPortal.position);
        Vector4 clipPlaneWorldSpace = new Vector4(portalPlane.normal.x, portalPlane.normal.y, portalPlane.normal.z, portalPlane.distance);
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCam.worldToCameraMatrix)) * clipPlaneWorldSpace;
        portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
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
