using UnityEngine;

public class UIGravity : MonoBehaviour
{
    public Vector3 startVelocity = new Vector3(0f, -10f, 0f);
    float rotationSpeed;
    public float gravity = -1000f;

    void Start()
    {
        rotationSpeed = Random.Range(-360f, 360f);
        startVelocity.y = Random.Range(500f, -300f);
        startVelocity.x = Random.Range(-700f, -300f);
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        transform.position += startVelocity * Time.deltaTime;
        //Apply gravity
        startVelocity.y += gravity * Time.deltaTime;

        if(transform.position.y < -1000f){ //Arbitrary large number to ensure it's well off screen
            Destroy(gameObject);
        }
    }
}
