using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    [SerializeField]
    Portal linkedPortal;
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
        SetInputCamera();
        RotateCamera();
        TranslateCamera();
        //SetNearClipPlane();
    }

    void SetInputCamera()
    {
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

    void SetNearClipPlane()
    {
        Vector3 planeNormal = linkedPortal.transform.forward;
        Vector3 planePos = linkedPortal.transform.position;
        float distance = -Vector3.Dot(planeNormal, planePos);
        Vector4 clipPlaneWorldSpace = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, distance);
        Matrix4x4 cameraToWorld = cam.worldToCameraMatrix.inverse;
        Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(cameraToWorld) * clipPlaneWorldSpace;
        Matrix4x4 newProjectionMatrix = Camera.main.CalculateObliqueMatrix(clipPlaneCameraSpace);
        cam.projectionMatrix = newProjectionMatrix;
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
        bool moveToLinkedPortal = Vector3.Dot(transform.forward, other.transform.position - transform.position) > 0;
        if (moveToLinkedPortal)
        {
            Vector3 offset = transform.InverseTransformPoint(other.transform.position);
            other.transform.position = linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
            other.transform.rotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation * Quaternion.Euler(0, 180, 0);
        }
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
        while (copies.ContainsKey(other) && copies[other] != null)
        {
            foreach (Transform t in transforms.Keys)
            {
                bool lookingAtPortal = Vector3.Dot(transform.forward, t.forward) < 0;
                transforms[t].enabled = lookingAtPortal;
                copyCameras[transforms[t]].enabled = !lookingAtPortal;
            }
            Vector3 offset = transform.InverseTransformPoint(other.transform.position);
            copies[other].transform.position = linkedPortal.transform.TransformPoint(new Vector3(-offset.x, offset.y, -offset.z));
            copies[other].transform.rotation = linkedPortal.transform.rotation * Quaternion.Inverse(transform.rotation) * other.transform.rotation * Quaternion.Euler(0, 180, 0);
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
