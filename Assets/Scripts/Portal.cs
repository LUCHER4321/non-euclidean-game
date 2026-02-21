using UnityEngine;
using System.Collections.Generic;

public class Portal : MonoBehaviour
{
    [SerializeField]
    Camera inputCam = Camera.main;
    [SerializeField]
    Portal linkedPortal;
    [SerializeField]
    Camera cam;
    [SerializeField]
    bool teleport = true;
    private RenderTexture rt;
    private Dictionary<Collider, int> teleportObjects = new Dictionary<Collider, int>();

    void Awake()
    {
        inputCam = Camera.main;
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
        Renderer rndr = linkedPortal.GetComponent<Renderer>();
        if (rndr != null) rndr.material = newMaterial;
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
        Quaternion direction = Quaternion.Inverse(transform.rotation) * inputCam.transform.rotation;
        linkedPortal.cam.transform.localEulerAngles = direction.eulerAngles + 180 * Vector3.up;
    }

    void TranslateCamera()
    {
        if (linkedPortal == null || cam == null) return;
        Vector3 offset = transform.InverseTransformPoint(inputCam.transform.position);
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
        Matrix4x4 newProjectionMatrix = inputCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        cam.projectionMatrix = newProjectionMatrix;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!teleport) return;
        if (linkedPortal == null) return;
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;
        Vector3 fromPortal = transform.InverseTransformPoint(rb.transform.position);
        bool inThis = teleportObjects.ContainsKey(other);
        bool inLinked = linkedPortal.teleportObjects.ContainsKey(other);
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
            teleportObjects[other] = originalLayer;
            linkedPortal.teleportObjects[other] = originalLayer;
        }
        else if (!inThis)
        {
            teleportObjects[other] = other.gameObject.layer;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!teleport) return;
        if (teleportObjects.ContainsKey(other))
        {
            teleportObjects.Remove(other);
        }
    }
}
