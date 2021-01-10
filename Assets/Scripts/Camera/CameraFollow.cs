using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Math;
public class CameraFollow : MonoBehaviour
{
    public bool isStatic;
    public bool adaptive;
    private Camera main;
    private GameObject player;
    private RaycastHit2D Ray;
    private RaycastHit2D floor;
    public LayerMask layers;
    float initZ;
    private Vector2 initialPos;
    private new SpriteRenderer renderer;
    private void Start()
    {
        main = Camera.main;
        player = GameObject.Find("player");
        initZ = transform.position.z;
        initialPos = main.transform.position;
        if(player != null)
            renderer = player.GetComponentInChildren<SpriteRenderer>();
    }
    private void FixedUpdate()
    {
        if(!isStatic)
        {
            if (player != null)
            {
                if (adaptive)
                {
                    int Layer = layers;
                    floor = Physics2D.Raycast(player.transform.position, new Vector2(0, -1), Mathf.Infinity, Layer);
                    // Cast a ray to floor so we can calculate a distance between ceiling and floor
                    Ray = Physics2D.Raycast(floor.point + new Vector2(0, 0.1f), new Vector2(0, 1), Mathf.Infinity, Layer);
                    main.orthographicSize = Mathf.Lerp(main.orthographicSize, Ray.distance / 2 + 10, 0.05F);

                }
                transform.position = Vector2.Lerp((Vector2)transform.position, player.transform.position, 0.1f);
                transform.position = new Vector3(transform.position.x, transform.position.y, initZ);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(Ray.point, 0.1f);
        Gizmos.DrawSphere(floor.point, 0.1f);
    }
}
