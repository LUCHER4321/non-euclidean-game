using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Character")]
    public Rigidbody rb;
    public Camera cam;
    public bool isRunning = false;
    public CharacterSO characterSO;

    public bool CanSee(GameObject target)
    {
        if (characterSO == null) return false;
        Vector3 direction = target.transform.position - cam.transform.position;
        if (direction.sqrMagnitude > Mathf.Pow(characterSO.GetVisionLength, 2)) return false;
        if (Vector3.Dot(cam.transform.forward, direction.normalized) <= Mathf.Cos(characterSO.GetVisionAngle * Mathf.Deg2Rad)) return false;
        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit, characterSO.GetVisionLength))
        {
            return hit.collider.gameObject == target;
        }
        return false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(Vector2 velocity)
    {
        if (rb == null) return;
        Vector2 input = velocity.normalized * (isRunning ? characterSO.GetRunSpeed : characterSO.GetMoveSpeed);
        rb.linearVelocity = input.x * rb.transform.right + rb.linearVelocity.y * Vector3.up + input.y * rb.transform.forward;
    }

    public void Jump()
    {
        if (rb == null) return;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, characterSO.GetHeight + 0.1f)) return;
        float initialSpeed = Mathf.Sqrt(2 * characterSO.GetJumpHeight * -Physics.gravity.y);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, initialSpeed, rb.linearVelocity.z);
    }

    public void Look(Vector2 delta)
    {
        if (cam == null) return;
        rb.transform.Rotate(Vector3.up, delta.x);
        float currentPitch = cam.transform.localRotation.eulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
        float newPitch = currentPitch - delta.y;
        newPitch = Mathf.Clamp(newPitch, characterSO.GetLimit.x, characterSO.GetLimit.y);
        cam.transform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
    }
}
