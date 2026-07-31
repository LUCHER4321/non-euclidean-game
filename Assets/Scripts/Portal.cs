using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    [Header("Basic Properties")]
    [SerializeField] public Camera cam;
    [SerializeField] bool teleport = true;
    [SerializeField] bool artificialShadows = true;
    [Header("Portal Relationships")]
    public Portal linkedPortal;
    [SerializeField] Portal auxiliaryPortal;
    private Dictionary<Light, Light> clonedLights;
    private Dictionary<Light, DecalProjector> negativeDecals;
    private Dictionary<Light, DecalProjector> negativeDecalsForPortal;
    private RenderTexture rt;
    private Dictionary<Collider, GameObject> copies;
    private HashSet<Collider> newObjects = new HashSet<Collider>();
    private Dictionary<Collider, bool> portalEntrySides = new Dictionary<Collider, bool>();
    public Collider PortalCollider { get; private set; }
    private Vector3[] localCorners = new Vector3[8];

    bool IsInBounds(Vector3 position)
    {
        return PortalCollider != null && PortalCollider.bounds.Contains(position);
    }

    bool IsInBounds(Transform trs)
    {
        return IsInBounds(trs.position);
    }

    bool DoesLightReachPortal(Light sourceLight)
    {
        if (sourceLight.type == LightType.Directional) return true;
        if (PortalCollider == null) return true;
        Vector3 lightPos = sourceLight.transform.position;
        if (Vector3.Dot(lightPos - transform.position, -transform.forward) >= 0) return false;
        Vector3 closestPoint = PortalCollider.ClosestPoint(lightPos);
        Vector3 offsetToClosest = closestPoint - lightPos;
        float sqrDistance = offsetToClosest.sqrMagnitude;
        float sqrRange = sourceLight.range * sourceLight.range;
        if (sqrDistance > sqrRange) return false;
        if (sourceLight.type == LightType.Point) return true;
        if (sourceLight.type == LightType.Spot)
        {
            Vector3 forward = sourceLight.transform.forward;
            int mask = sourceLight.cullingMask;
            float minDot = Mathf.Cos(sourceLight.spotAngle * 0.5f * Mathf.Deg2Rad);
            bool CheckPointReach(Vector3 targetPoint)
            {
                Vector3 offset = targetPoint - lightPos;
                Vector3 dir = offset.normalized;
                if (Vector3.Dot(forward, dir) >= minDot && !Physics.Raycast(lightPos, dir, offset.magnitude, mask)) return true;
                return false;
            }
            if (CheckPointReach(closestPoint)) return true;
            if (CheckPointReach(PortalCollider.bounds.center)) return true;
            foreach (Vector3 localCorner in localCorners)
            {
                Vector3 worldCorner = transform.TransformPoint(localCorner);
                if (CheckPointReach(worldCorner)) return true;
            }
            return false;
        }
        return true;
    }

    bool IsVisibleFrom(Camera camera)
    {
        if (camera == null || PortalCollider == null || camera.farClipPlane < Vector3.Distance(camera.transform.position, PortalCollider.ClosestPoint(camera.transform.position))) return false;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, PortalCollider.bounds);
    }

    bool IsOnPortalSide(Collider other)
    {
        if (other == null) return false;
        return Vector3.Dot(other.transform.position - transform.position, transform.forward) < 0f;
    }

    public Ray RedirectRay(Vector3 hitPoint, Vector3 incomingDirection)
    {
        if (linkedPortal == null || (!teleport && (auxiliaryPortal == null || !auxiliaryPortal.IsInBounds(hitPoint)))) return new Ray(hitPoint, incomingDirection);
        Vector3 localHitPoint = transform.InverseTransformPoint(hitPoint);
        Vector3 outOrigin = linkedPortal.transform.TransformPoint(new Vector3(-localHitPoint.x, localHitPoint.y, -localHitPoint.z));
        Vector3 localDirection = transform.InverseTransformDirection(incomingDirection);
        Vector3 outDirection = linkedPortal.transform.TransformDirection(new Vector3(-localDirection.x, localDirection.y, -localDirection.z));
        return new Ray(outOrigin, outDirection);
    }

    private static Portal ClosestPortal(Vector3 position)
    {
        Portal[] portals = FindObjectsByType<Portal>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (portals.Length == 0) return null;
        Portal closestP = null;
        float minSqrDist = -1f;
        foreach (Portal portal in portals)
        {
            if (!portal.teleport) continue;
            float sqrDist = (portal.transform.position - position).sqrMagnitude;
            if (sqrDist < minSqrDist || minSqrDist < 0f)
            {
                minSqrDist = sqrDist;
                closestP = portal;
            }
        }
        return closestP;
    }

    public static bool IsInPortal(Vector3 position, out Portal closestPortal)
    {
        closestPortal = ClosestPortal(position);
        if (closestPortal == null || closestPortal.auxiliaryPortal == null) return false;
        return closestPortal.auxiliaryPortal.IsInBounds(position);
    }

    public static bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = Physics.DefaultRaycastLayers, int maxBounces = 10)
    {
        Portal startingPortal;
        if (IsInPortal(ray.origin, out startingPortal)) ray = startingPortal.RedirectRay(ray.origin, ray.direction);
        return RaycastRecursive(ray, out hitInfo, maxDistance, layerMask, maxBounces);
    }

    private static bool RaycastRecursive(Ray ray, out RaycastHit hitInfo, float maxDistance, int layerMask, int bouncesLeft)
    {
        if (bouncesLeft < 0)
        {
            hitInfo = new RaycastHit();
            return false;
        }
        bool hit = Physics.Raycast(ray, out hitInfo, maxDistance, layerMask);
        if (hit)
        {
            Portal hitPortal = hitInfo.collider.GetComponent<Portal>();
            if (hitPortal != null)
            {
                float remainingDistance = maxDistance - hitInfo.distance;
                Ray redirectedRay = hitPortal.RedirectRay(hitInfo.point, ray.direction);
                Collider linkedCollider = null;
                if (hitPortal.linkedPortal != null)
                {
                    linkedCollider = hitPortal.linkedPortal.PortalCollider;
                    if (linkedCollider != null) linkedCollider.enabled = false;
                }
                bool result = RaycastRecursive(redirectedRay, out hitInfo, remainingDistance, layerMask, bouncesLeft - 1);
                if (linkedCollider != null) linkedCollider.enabled = true;
                return result;
            }
        }
        return hit;
    }

    public static Transform FirstParent(Transform t)
    {
        if (t == null) return null;
        if (t.parent == null) return t;
        return FirstParent(t.parent);
    }

    void Awake()
    {
        copies = new Dictionary<Collider, GameObject>();
        clonedLights = new Dictionary<Light, Light>();
        negativeDecals = new Dictionary<Light, DecalProjector>();
        negativeDecalsForPortal = new Dictionary<Light, DecalProjector>();
        PortalCollider = GetComponent<Collider>();
        if (auxiliaryPortal != null) auxiliaryPortal.linkedPortal = linkedPortal.auxiliaryPortal;
        CalculateLocalCorners();
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
        bool shouldRender = linkedPortal != null && linkedPortal.cam != null && Camera.main != null && linkedPortal.IsVisibleFrom(Camera.main);
        if (!shouldRender)
        {
            if (cam != null && cam.enabled) cam.enabled = false;
            return;
        }
        if (!cam.enabled) cam.enabled = true;
        cam.aspect = Camera.main.aspect;
        RotateCamera();
        TranslateCamera();
        SetCameraNear();
    }

    void CalculateLocalCorners()
    {
        if (PortalCollider is BoxCollider box)
        {
            Vector3 c = box.center;
            Vector3 e = box.size * 0.5f;
            localCorners[0] = c + new Vector3(e.x, e.y, e.z);
            localCorners[1] = c + new Vector3(e.x, e.y, -e.z);
            localCorners[2] = c + new Vector3(e.x, -e.y, e.z);
            localCorners[3] = c + new Vector3(e.x, -e.y, -e.z);
            localCorners[4] = c + new Vector3(-e.x, e.y, e.z);
            localCorners[5] = c + new Vector3(-e.x, e.y, -e.z);
            localCorners[6] = c + new Vector3(-e.x, -e.y, e.z);
            localCorners[7] = c + new Vector3(-e.x, -e.y, -e.z);
        }
        else if (PortalCollider != null)
        {
            Bounds bounds = PortalCollider.bounds;
            Vector3 ext = bounds.extents;
            Vector3 center = bounds.center;
            localCorners[0] = transform.InverseTransformPoint(center + new Vector3(ext.x, ext.y, ext.z));
            localCorners[1] = transform.InverseTransformPoint(center + new Vector3(ext.x, ext.y, -ext.z));
            localCorners[2] = transform.InverseTransformPoint(center + new Vector3(ext.x, -ext.y, ext.z));
            localCorners[3] = transform.InverseTransformPoint(center + new Vector3(ext.x, -ext.y, -ext.z));
            localCorners[4] = transform.InverseTransformPoint(center + new Vector3(-ext.x, ext.y, ext.z));
            localCorners[5] = transform.InverseTransformPoint(center + new Vector3(-ext.x, ext.y, -ext.z));
            localCorners[6] = transform.InverseTransformPoint(center + new Vector3(-ext.x, -ext.y, ext.z));
            localCorners[7] = transform.InverseTransformPoint(center + new Vector3(-ext.x, -ext.y, -ext.z));
        }
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
                clonedLight.bounceIntensity = sourceLight.bounceIntensity;
                GameObject negativeDecalObj = new GameObject(sourceLight.name + " (Portal Negative Decal)");
                GameObject negativeDecalForPortalObj = new GameObject(sourceLight.name + " (Portal Negative Decal For Portal)");
                negativeDecalObj.transform.SetParent(clonedLight.transform);
                negativeDecalForPortalObj.transform.SetParent(clonedLight.transform);
                DecalProjector negativeDecal = negativeDecalObj.AddComponent<DecalProjector>();
                DecalProjector negativeDecalForPortal = negativeDecalForPortalObj.AddComponent<DecalProjector>();
                negativeDecal.material = new Material(PortalST.Instance.GetLightGraph);
                negativeDecalForPortal.material = negativeDecal.material;
                clonedLights.Add(sourceLight, clonedLight);
                negativeDecals.Add(sourceLight, negativeDecal);
                negativeDecal.enabled = false;
                negativeDecalsForPortal.Add(sourceLight, negativeDecalForPortal);
                negativeDecalForPortal.enabled = false;
            }
        }
        StartCoroutine(SyncLights());
    }

    IEnumerator SyncLights()
    {
        while (true)
        {
            if (Camera.main == null || !IsVisibleFrom(Camera.main))
            {
                foreach (KeyValuePair<Light, Light> kvp in clonedLights)
                {
                    Light sourceLight = kvp.Key;
                    Light clonedLight = kvp.Value;
                    clonedLight.enabled = false;
                    if (artificialShadows)
                    {
                        DecalProjector negativeDecal = negativeDecals[sourceLight];
                        DecalProjector negativeDecalForPortal = negativeDecalsForPortal[sourceLight];
                        negativeDecal.enabled = false;
                        negativeDecalForPortal.enabled = false;
                    }
                }
                yield return null;
                continue;
            }
            foreach (KeyValuePair<Light, Light> kvp in clonedLights)
            {
                Light sourceLight = kvp.Key;
                Light clonedLight = kvp.Value;
                DecalProjector negativeDecal = negativeDecals[sourceLight];
                DecalProjector negativeDecalForPortal = negativeDecalsForPortal[sourceLight];
                if (sourceLight == null || !sourceLight.enabled || !sourceLight.gameObject.activeInHierarchy || (!DoesLightReachPortal(sourceLight) && !auxiliaryPortal.IsInBounds(sourceLight.transform)))
                {
                    clonedLight.enabled = false;
                    if (artificialShadows)
                    {
                        negativeDecal.enabled = false;
                        negativeDecalForPortal.enabled = false;
                    }
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
                if (!artificialShadows) continue;
                if (!Physics.Raycast(clonedLight.transform.position, linkedPortal.transform.position - clonedLight.transform.position, (linkedPortal.transform.position - clonedLight.transform.position).magnitude, sourceLight.cullingMask))
                {
                    negativeDecal.enabled = false;
                    negativeDecalForPortal.enabled = false;
                    continue;
                }
                negativeDecal.enabled = true;
                negativeDecalForPortal.enabled = true;
                float dist = Vector3.Distance(sourceLight.transform.position, transform.position);
                clonedLight.shadowNearPlane = dist;
                negativeDecal.transform.position = clonedLight.transform.position;
                negativeDecal.transform.rotation = clonedLight.transform.rotation;
                Collider linkedPortalCol = linkedPortal.PortalCollider;
                Vector3 closestPoint0 = linkedPortalCol != null ? linkedPortalCol.ClosestPoint(clonedLight.transform.position) : linkedPortal.transform.position;
                Vector3 closestPoint = closestPoint0 - Vector3.Dot(closestPoint0 - linkedPortal.transform.position, linkedPortal.transform.forward) * linkedPortal.transform.forward;
                Vector3 tp = linkedPortal.transform.position - closestPoint;
                Vector3 toPortal = tp - Vector3.Dot(tp, linkedPortal.transform.up) * linkedPortal.transform.up;
                negativeDecalForPortal.transform.position = clonedLight.transform.position - linkedPortal.transform.forward * Vector3.Dot(clonedLight.transform.position - linkedPortal.transform.position, linkedPortal.transform.forward);
                negativeDecalForPortal.transform.forward = toPortal.normalized;
                float distToPortal = Vector3.Distance(clonedLight.transform.position, closestPoint);
                float spotSize = sourceLight.type == LightType.Spot ? Mathf.Tan(sourceLight.spotAngle * 0.5f * Mathf.Deg2Rad) * distToPortal * 2f : sourceLight.range;
                negativeDecal.size = new Vector3(spotSize, spotSize, distToPortal);
                float distZ = (closestPoint - negativeDecalForPortal.transform.position).magnitude + 0.001f;
                negativeDecalForPortal.size = new Vector3(spotSize, spotSize, distZ);
                negativeDecal.pivot = distToPortal / 2f * Vector3.forward;
                negativeDecalForPortal.pivot = distZ / 2f * Vector3.forward;
                negativeDecal.material.SetVector("_Position", linkedPortal.transform.position);
                negativeDecal.material.SetVector("_Normal", -linkedPortal.transform.forward);
                negativeDecal.material.SetVector("_Closest", closestPoint);
            }
            yield return null;
        }
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
        if (newObjects.Contains(other))
        {
            newObjects.Remove(other);
            return;
        }
        if (!teleport || linkedPortal == null || !linkedPortal.teleport || copies.ContainsKey(other) || copies.ContainsValue(other.gameObject) || other.gameObject.name.Contains("Copy")) return;
        if (other.transform.parent != null)
        {
            Collider parentCollider = FirstParent(other.transform).GetComponent<Collider>();
            if (parentCollider != null)
            {
                OnTriggerEnter(parentCollider);
                return;
            }
        }
        portalEntrySides[other] = IsOnPortalSide(other);
        linkedPortal.RegisterArrival(other);
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        Quaternion portalRotationMapping = linkedPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(transform.rotation);
        Vector3 offset = other.transform.position - transform.position;
        Vector3 targetPosition = linkedPortal.transform.position + (portalRotationMapping * offset);
        GameObject copy = Instantiate(other.gameObject, targetPosition, portalRotationMapping * other.transform.rotation);
        copy.name = other.gameObject.name + " Copy";
        copy.GetComponent<Collider>().enabled = false;
        copy.GetComponent<Rigidbody>().useGravity = false;
        foreach (AudioListener listener in copy.GetComponentsInChildren<AudioListener>()) listener.enabled = false;
        foreach (Camera camera in copy.GetComponentsInChildren<Camera>()) camera.enabled = false;
        foreach (Light light in copy.GetComponentsInChildren<Light>()) light.enabled = false;
        copies.Add(other, copy);
        StartCoroutine(MoveCopy(other));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!teleport || !copies.ContainsKey(other) || linkedPortal == null || !linkedPortal.teleport || copies.ContainsValue(other.gameObject)) return;
        if (other.transform.parent != null)
        {
            Collider parentCollider = FirstParent(other.transform).GetComponent<Collider>();
            if (parentCollider != null)
            {
                OnTriggerExit(parentCollider);
                return;
            }
        }
        if (newObjects.Contains(other)) newObjects.Remove(other);
        GameObject copy = copies[other];
        copies.Remove(other);
        Destroy(copy.gameObject);
        Quaternion portalRotationMapping = linkedPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(transform.rotation);
        Vector3 offset = other.transform.position - transform.position;
        Vector3 targetPosition = linkedPortal.transform.position + (portalRotationMapping * offset);
        Quaternion targetRotation = portalRotationMapping * other.transform.rotation;
        bool currentPortalSide = IsOnPortalSide(other);
        bool shouldTeleport = !portalEntrySides.TryGetValue(other, out bool enteredPortalSide) || enteredPortalSide != currentPortalSide;
        portalEntrySides.Remove(other);
        if (shouldTeleport)
        {
            other.transform.position = targetPosition;
            other.transform.rotation = targetRotation;
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = portalRotationMapping * rb.linearVelocity;
                rb.angularVelocity = portalRotationMapping * rb.angularVelocity;
            }
        }
    }

    IEnumerator MoveCopy(Collider other)
    {
        while (copies.ContainsKey(other) && copies[other] != null)
        {
            Quaternion portalRotationMapping = linkedPortal.transform.rotation * Quaternion.Euler(0, 180, 0) * Quaternion.Inverse(transform.rotation);
            Vector3 offset = other.transform.position - transform.position;
            Vector3 targetPosition = linkedPortal.transform.position + (portalRotationMapping * offset);
            Quaternion targetRotation = portalRotationMapping * other.transform.rotation;
            copies[other].transform.position = targetPosition;
            copies[other].transform.rotation = targetRotation;
            KeepLocals(other.transform, copies[other].transform);
            yield return null;
        }
    }

    void KeepLocals(Transform other, Transform copy)
    {
        if (other.childCount == 0 || copy.childCount == 0) return;
        for (int i = 0; i < other.childCount; i++)
        {
            Transform child = other.GetChild(i);
            Transform copyChild = copy.GetChild(i);
            copyChild.localRotation = child.localRotation;
            copyChild.localPosition = child.localPosition;
            copyChild.localScale = child.localScale;
            KeepLocals(child, copyChild);
        }
    }

    void RegisterArrival(Collider other)
    {
        newObjects.Add(other);
    }

    void OnDrawGizmos()
    {
        if (!teleport) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, cam.transform.position);
        if (linkedPortal == null || linkedPortal.clonedLights == null) return;
        Gizmos.color = Color.blue;
        foreach (Light light in linkedPortal.clonedLights.Values) if (light != null && light.enabled) Gizmos.DrawLine(transform.position, light.transform.position);
    }
}
