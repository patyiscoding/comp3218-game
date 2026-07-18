using UnityEngine;

public class FrogActions : MonoBehaviour
{
    public Rigidbody frog;
    public float jumpForce = 5f; // upward
    public float forwardForce = 2f;
    public Vector3 jumpDirection = Vector3.forward;

    public void Jump()
    {
        frog.drag = 1f;
        frog.angularDrag = 2f;
        frog.velocity = new Vector3(frog.velocity.x, 0f, frog.velocity.z);

        Vector3 force = jumpDirection.normalized * forwardForce;
        force.y = jumpForce;

        frog.AddForce(force, ForceMode.Impulse);
    }
}
