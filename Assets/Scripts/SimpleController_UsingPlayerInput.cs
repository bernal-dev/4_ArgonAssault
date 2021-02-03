using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

// Use a separate PlayerInput component for setting up input.
public class SimpleController_UsingPlayerInput : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float xSpeed = 10f;
    [SerializeField] private float xRange = 9f;
    [SerializeField] private float ySpeed = 10f;
    [SerializeField] private float yRange = 5f;

    [Header("Screen Position Based")]
    [SerializeField] private float positionPitchFactor = -5f;
    [SerializeField] private float positionYawFactor = 5f;

    [Header("Control Throw Based")]
    [SerializeField] private float controlPitchFactor = -20f;
    [SerializeField] private float controlRollFactor = -35;


    private bool isControlEnabled = true;
    private float rotateSpeed;
    private float burstSpeed;
    public GameObject projectile;

    private bool m_Charging;
    private Vector2 m_Rotation;
    private Vector2 m_Look;
    private Vector2 m_Move;

    public void OnMove(InputAction.CallbackContext context)
    {
        m_Move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        m_Look = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is SlowTapInteraction)
                {
                    StartCoroutine(BurstFire((int)(context.duration * burstSpeed)));
                }
                else
                {
                    Fire();
                }
                m_Charging = false;
                break;

            case InputActionPhase.Started:
                if (context.interaction is SlowTapInteraction)
                    m_Charging = true;
                break;

            case InputActionPhase.Canceled:
                m_Charging = false;
                break;
        }
    }

    public void OnPlayerDeath() // Called by string reference
    {
        isControlEnabled = false;
        print("froze controls");
    }

    public void OnGUI()
    {
        if (m_Charging)
            GUI.Label(new Rect(100, 100, 200, 100), "Charging...");
    }

    public void Update()
    {
        // Update orientation first, then move. Otherwise move orientation will lag
        if (isControlEnabled)
        {
            ProccessTranslation(m_Move);
            ProccessRotation(m_Move);
        }
    }

    private void ProccessRotation(Vector2 direction)
    {

        var pitchDueToPosition = transform.localPosition.y * positionPitchFactor;
        var pitchDueToControlThrow = direction.y * controlPitchFactor;
        var pitch = pitchDueToPosition + pitchDueToControlThrow;

        var yaw = transform.localPosition.x * positionYawFactor;

        var roll = direction.x * controlRollFactor;
        transform.localRotation = Quaternion.Euler(pitch, yaw, roll);
    }

    private void ProccessTranslation(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01)
            return;

        var xSpeedFrameIndependent = xSpeed * Time.deltaTime;
        var ySpeedFrameIndependen = ySpeed * Time.deltaTime;
        var xOffset = direction.x * xSpeedFrameIndependent;
        var yOffset = direction.y * ySpeedFrameIndependen;
        var xFuturePosition = Mathf.Clamp(xOffset + transform.localPosition.x, -xRange, xRange);
        var yFuturePosition = Mathf.Clamp(yOffset + transform.localPosition.y, -yRange, yRange);
        transform.localPosition = new Vector3(xFuturePosition, yFuturePosition, transform.localPosition.z);
    }

    private void Look(Vector2 rotate)
    {
        if (rotate.sqrMagnitude < 0.01)
            return;
        var scaledRotateSpeed = rotateSpeed * Time.deltaTime;
        m_Rotation.y += rotate.x * scaledRotateSpeed;
        m_Rotation.x = Mathf.Clamp(m_Rotation.x - rotate.y * scaledRotateSpeed, -89, 89);
        transform.localEulerAngles = m_Rotation;
    }

    private IEnumerator BurstFire(int burstAmount)
    {
        for (var i = 0; i < burstAmount; ++i)
        {
            Fire();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Fire()
    {
        var transform = this.transform;
        var newProjectile = Instantiate(projectile);
        newProjectile.transform.position = transform.position + transform.forward * 0.6f;
        newProjectile.transform.rotation = transform.rotation;
        const int size = 1;
        newProjectile.transform.localScale *= size;
        newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
        newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
        newProjectile.GetComponent<MeshRenderer>().material.color =
            new Color(Random.value, Random.value, Random.value, 1.0f);
    }
}
