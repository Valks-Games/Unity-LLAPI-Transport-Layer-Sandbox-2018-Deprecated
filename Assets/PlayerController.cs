using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public GameObject gridGameObject;
    public GameObject tilemapGameObject;

    public float maxSpeed = 25f;
    public float speed = 500f;

    private Rigidbody2D rb;
    private Grid grid;
    private Tilemap tilemap;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        grid = gridGameObject.GetComponent<Grid>();
        tilemap = tilemapGameObject.GetComponent<Tilemap>();
    }

    void FixedUpdate()
    {
        float moveHorz = Input.GetAxis("Horizontal");
        float moveVert = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorz, moveVert) * Time.deltaTime * speed;

        rb.AddForce(movement, ForceMode2D.Force);
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSpeed * Time.deltaTime);

        Vector3Int pos = grid.WorldToCell(transform.position);

        string name = tilemap.GetTile(pos).name;

        if (name.Equals("Water"))
        {
            speed = 100f;
        }
        else {
            speed = 500f;
        }
    }
}
