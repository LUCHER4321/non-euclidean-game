using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    [Header("Basic Properties")]
    [SerializeField]
    public Camera cam;
    [SerializeField]
    bool teleport = true;
    [Header("Portal Relationships")]
    public Portal linkedPortal;
    [SerializeField]
    Portal auxiliaryPortal;
    private Dictionary<Light, Light> clonedLights;
    private Dictionary<Light, DecalProjector> negativeDecals;
    private RenderTexture rt;
    private Dictionary<Collider, GameObject> copies;

    bool DoesLightReachPortal(Light sourceLight)
    {
        if (sourceLight.type == LightType.Directional) return true;
        Collider portalCollider = GetComponent<Collider>();
        if (portalCollider == null) return true;
        Vector3 lightPos = sourceLight.transform.position;
        if (Vector3.Dot(lightPos - transform.position, -transform.forward) >= 0) return false;
        Vector3 closestPoint = portalCollider.ClosestPoint(lightPos);
        float sqrDistance = (closestPoint - lightPos).sqrMagnitude;
        float sqrRange = sourceLight.range * sourceLight.range;
        if (sqrDistance > sqrRange) return false;
        if (sourceLight.type == LightType.Point) return true;
        if (sourceLight.type == LightType.Spot)
        {
            float halfAngle = sourceLight.spotAngle * 0.5f;
            Vector3 forward = sourceLight.transform.forward;
            Vector3 dirToClosest = (closestPoint - lightPos).normalized;
            if (Vector3.Angle(forward, dirToClosest) <= halfAngle) return true;
            Vector3 center = portalCollider.bounds.center;
            Vector3 dirToCenter = (center - lightPos).normalized;
            if (Vector3.Angle(forward, dirToCenter) <= halfAngle) return true;
            Vector3 extents = portalCollider.bounds.extents;
            Vector3[] corners = new Vector3[8]
            {
                center + new Vector3(extents.x, extents.y, extents.z),
                center + new Vector3(extents.x, extents.y, -extents.z),
                center + new Vector3(extents.x, -extents.y, extents.z),
                center + new Vector3(extents.x, -extents.y, -extents.z),
                center + new Vector3(-extents.x, extents.y, extents.z),
                center + new Vector3(-extents.x, extents.y, -extents.z),
                center + new Vector3(-extents.x, -extents.y, extents.z),
                center + new Vector3(-extents.x, -extents.y, -extents.z)
            };
            foreach (Vector3 corner in corners)
            {
                Vector3 dirToCorner = (corner - lightPos).normalized;
                if (Vector3.Angle(forward, dirToCorner) <= halfAngle) return true;
            }
            return false;
        }
        return true;
    }

    void Awake()
    {
        copies = new Dictionary<Collider, GameObject>();
        clonedLights = new Dictionary<Light, Light>();
        negativeDecals = new Dictionary<Light, DecalProjector>();
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
        InitializeLights();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (linkedPortal == null || cam == null || Camera.main == null) return;
        cam.aspect = Camera.main.aspect;
        RotateCamera();
        TranslateCamera();
        SetCameraNear();
        SyncLights();
    }

    void InitializeLights()
    {
        if (linkedPortal == null || !teleport) return;

        foreach (Light sourceLight in PortalST.Instance.GetIncomingLights)
        {
            if (sourceLight != null && !clonedLights.ContainsKey(sourceLight))
            {
                GameObject lightCloneObj = new GameObject(sourceLight.name + " (Portal Clone)");
                lightCloneObj.transform.SetParent(linkedPortal.transform);
                Light clonedLight = lightCloneObj.AddComponent<Light>();
                clonedLight.type = sourceLight.type;
                clonedLight.color = sourceLight.color;
                clonedLight.intensity = sourceLight.intensity;
                clonedLight.range = sourceLight.range;
                clonedLight.spotAngle = sourceLight.spotAngle;
                clonedLight.innerSpotAngle = sourceLight.innerSpotAngle;
                clonedLight.shadows = sourceLight.shadows;
                clonedLight.cullingMask = sourceLight.cullingMask;
                sourceLight.renderingLayerMask = sourceLight.cullingMask;
                clonedLight.renderingLayerMask = sourceLight.renderingLayerMask;
                GameObject negativeDecalObj = new GameObject(sourceLight.name + " (Portal Negative Decal)");
                negativeDecalObj.transform.SetParent(clonedLight.transform);
                DecalProjector negativeDecal = negativeDecalObj.AddComponent<DecalProjector>();
                negativeDecal.material = PortalST.Instance.GetNegativeDecalMaterial;
                clonedLights.Add(sourceLight, clonedLight);
                negativeDecals.Add(sourceLight, negativeDecal);
            }
        }
    }

    void SyncLights()
    {
        if (!teleport || linkedPortal == null) return;
        foreach (KeyValuePair<Light, Light> kvp in clonedLights)
        {
            Light sourceLight = kvp.Key;
            Light clonedLight = kvp.Value;
            DecalProjector negativeDecal = negativeDecals[sourceLight];
            if (sourceLight == null || !sourceLight.gameObject.activeInHierarchy || !DoesLightReachPortal(sourceLight))
            {
                clonedLight.enabled = false;
                negativeDecal.enabled = false;
                continue;
            }
            clonedLight.enabled = true;
            Vector3 localPos = transform.InverseTransformPoint(sourceLight.transform.position);
            Vector3 outOrigin = linkedPortal.transform.TransformPoint(new Vector3(-localPos.x, localPos.y, -localPos.z));
            clonedLight.transform.position = outOrigin;
            Vector3 localDir = transform.InverseTransformDirection(sourceLight.transform.forward);
            Vector3 outDirection = linkedPortal.transform.TransformDirection(new Vector3(-localDir.x, localDir.y, -localDir.z));
            clonedLight.transform.forward = outDirection;
            clonedLight.intensity = sourceLight.intensity;
            clonedLight.color = sourceLight.color;
            float dist = Vector3.Distance(sourceLight.transform.position, transform.position);
            clonedLight.shadowNearPlane = dist;
            negativeDecal.transform.position = clonedLight.transform.position;
            negativeDecal.transform.rotation = clonedLight.transform.rotation;
            float spotSize = sourceLight.type == LightType.Spot ? Mathf.Tan(sourceLight.spotAngle * 0.5f * Mathf.Deg2Rad) * dist * 2f : sourceLight.range;
            negativeDecal.size = new Vector3(spotSize, spotSize, dist);
        }
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
        if (linkedPortal == null || linkedPortal.cam == null || !teleport) return;
        Quaternion direction = Quaternion.Inverse(linkedPortal.transform.rotation) * Camera.main.transform.rotation;
        cam.transform.localEulerAngles = direction.eulerAngles + 180 * Vector3.up;
        auxiliaryPortal.cam.transform.rotation = cam.transform.rotation;
    }

    void TranslateCamera()
    {
        if (linkedPortal == null || cam == null || !teleport) return;
        Vector3 offset = linkedPortal.transform.InverseTransformPoint(Camera.main.transform.position);
        cam.transform.localPosition = new Vector3(-offset.x, offset.y, -offset.z);
        auxiliaryPortal.cam.transform.position = cam.transform.position;
    }

    void SetCameraNear()
    {
        if (linkedPortal == null || cam == null || !teleport) return;
        Vector3 portalPosition = transform.position;
        Vector3 portalNormal = transform.forward;
        Matrix4x4 worldToCamera = cam.worldToCameraMatrix;
        Vector3 viewSpacePos = worldToCamera.MultiplyPoint(portalPosition);
        Vector3 viewSpaceNormal = worldToCamera.MultiplyVector(portalNormal).normalized;
        float d = -Vector3.Dot(viewSpaceNormal, viewSpacePos);
        Vector4 clipPlane = new Vector4(viewSpaceNormal.x, viewSpaceNormal.y, viewSpaceNormal.z, d);
        cam.ResetProjectionMatrix();
        cam.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
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
        Gizmos.color = Color.blue;
        if (linkedPortal.clonedLights == null) return;
        foreach (Light light in linkedPortal.clonedLights.Values)
        {
            if (light != null)
            {
                Gizmos.DrawLine(transform.position, light.transform.position);
            }
        }
    }
}
