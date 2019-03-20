using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    //public GameObject gridGameObject;

    private Rigidbody2D rb;
    private Grid grid;
    private Tilemap tilemap;

    private bool movingUp = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool movingDown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //grid = gridGameObject.GetComponent<Grid>();
        //tilemap = grid.GetComponentInChildren<Tilemap>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) movingUp = true;
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) movingUp = false;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) movingLeft = true;
        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow)) movingLeft = false;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) movingRight = true;
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow)) movingRight = false;

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) movingDown = true;
        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow)) movingDown = false;

        //Vector3Int pos = grid.WorldToCell(transform.position);
        /*string name = tilemap.GetTile(pos).name;
        if (name.Equals("Water"))
        {
            speed = 100f;
        }
        else {
            speed = 500f;
        }*/
    }

    void FixedUpdate() {
        if (movingUp)
        {
            rb.AddForce(new Vector2(0, 5));
        }

        if (movingDown) {
            rb.AddForce(new Vector2(0, -5));
        }

        if (movingLeft)
        {
            rb.AddForce(new Vector2(-5, 0));
        }

        if (movingRight)
        {
            rb.AddForce(new Vector2(5, 0));
        }
    }
}
