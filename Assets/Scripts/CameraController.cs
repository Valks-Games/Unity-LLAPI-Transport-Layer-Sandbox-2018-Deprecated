using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset;
    public float speed = 1.0f;

    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    void LateUpdate()
    {
        float interpolation = speed * Time.deltaTime;

        Vector3 position = transform.position;
        position.x = Mathf.Lerp(transform.position.x, player.transform.position.x, interpolation);
        position.y = Mathf.Lerp(transform.position.y, player.transform.position.y, interpolation);

        transform.position = position;
    }
}
