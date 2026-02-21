using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Rigidbody rb;
    public Camera cam;
    private bool touchingFloor = false;
    public Vector2 limit = new Vector2(-90f, 90f);
    public float height = 1f;

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
        rb.linearVelocity = velocity.x * rb.transform.right + rb.linearVelocity.y * Vector3.up + velocity.y * rb.transform.forward;
    }

    public void Jump(float height)
    {
        if (rb == null) return;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (!Physics.Raycast(ray, height + 0.1f)) return;
        float initialSpeed = Mathf.Sqrt(2 * height * -Physics.gravity.y);
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, initialSpeed, rb.linearVelocity.z);
    }

    public void Look(Vector2 delta)
    {
        if (cam == null) return;
        rb.transform.Rotate(Vector3.up, delta.x);
        float currentPitch = cam.transform.localRotation.eulerAngles.x;
        if (currentPitch > 180f) currentPitch -= 360f;
        float newPitch = currentPitch - delta.y;
        newPitch = Mathf.Clamp(newPitch, limit.x, limit.y);
        cam.transform.localRotation = Quaternion.Euler(newPitch, 0f, 0f);
    }

    private void OnCollisionStay(Collision other)
    {
        if (other == null) return;
        ContactPoint[] contactPoints = other.contacts;
        foreach (ContactPoint contact in contactPoints)
        {
            if (contact.normal.y > 0.5f)
            {
                touchingFloor = true;
                return;
            }
        }
        touchingFloor = false;
    }
}
