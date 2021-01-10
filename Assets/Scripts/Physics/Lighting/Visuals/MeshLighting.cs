using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class MeshLighting : MonoBehaviour
{
    public float meshSize = 1 / 3;
    public Vector2Int Size = new Vector2Int(3, 3);
    public Vector2Int Size_Tiles;
    public Material material;
    private LightBehavior lightManager;
    private Vector2Int offset;
    public int divide = 4;
    private void Start()
    {
        lightManager = GetComponent<LightBehavior>();

        Size_Tiles = (Vector2Int)lightManager.LitTilemap.size;
        offset = (Vector2Int)lightManager.LitTilemap.origin / 3;
    }
    public void UpdateVisuals()
    {
        var sw = new Stopwatch();
        if (Input.GetKey(KeyCode.Mouse0))
        {
            sw.Start();
            int width = (Size_Tiles.x) * (int)Size.x;
            int height = (Size_Tiles.y) * (int)Size.y;
            int startX = 0;
            int startY = 0;
            int endX = width / divide;
            int endY = height / divide;
            for (; startX < width;)
            {
                for (; startY < height;)
                {
                    if (endY > height)
                    {
                        break;
                    }
                    CreateMesh(lightManager.lightLevel, new Vector2Int(0, 0), startX, endX, startY, endY);
                    startY += height / divide;
                    endY += height / divide;
                }
                startY = 0;
                endY = height / divide;
                startX += width / divide;
                endX += width / divide;
            }
            sw.Stop();
            print($"Overall creating all meshes took {sw.ElapsedMilliseconds} ms.");
        }
    }
    private void CreateMesh(Light[,] self, Vector2Int setposition, int startX, int endX, int startY, int endY)
    {
        var sw = new Stopwatch();
        sw.Start();
        //int width = (Size_Tiles.x) * (int)Size.x;
        int width = (Size_Tiles.x) * (int)Size.x;
        width /= 4;
        Light[,] newCanvas = self;
        Mesh mesh = new Mesh();
        int trianglesCount = 6;
        int verticesCount = 4;
        Vector3[] vertices = new Vector3[verticesCount * ((int)Size.x * (int)Size.y) * ((endX * endY) / 4)];
        Vector2[] uv = new Vector2[verticesCount * ((int)Size.x * (int)Size.y) * ((endX * endY) / 4)];
        Color[] colors = new Color[verticesCount * ((int)Size.x * (int)Size.y) * ((endX * endY) / 4)];
        int[] triangles = new int[trianglesCount * ((int)Size.x * (int)Size.y) * ((endX * endY) / 4)];
        sw.Stop();
        print($"Creating needed arrays and variables took {sw.ElapsedMilliseconds} ms total");
        sw.Reset();
        sw.Start();
        #region vertices tris and uv FirstMesh
        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                int index = y * width + x;
                vertices[index * verticesCount + 0] = new Vector3(meshSize * (x), meshSize * (y)); // 0 0  // 1 * (0 + 0) = 0
                vertices[index * verticesCount + 1] = new Vector3(meshSize * (x), meshSize * ((y + 1))); // 0 1
                vertices[index * verticesCount + 2] = new Vector3(meshSize * (x + 1), meshSize * ((y + 1))); // 1 1
                vertices[index * verticesCount + 3] = new Vector3(meshSize * (x + 1), meshSize * (y)); // 1 0
                Vector2Int position = new Vector2Int(setposition.x + (x / (int)Size.x), setposition.y + (y / (int)Size.y));
                if (!(position.x > 0 && position.x < newCanvas.GetLength(0) - 1 && position.y > 0 && position.y < newCanvas.GetLength(1) - 1))
                {
                    uv[index * verticesCount + 0] = new Vector2(0, 0);
                    uv[index * verticesCount + 1] = new Vector2(0, 1);
                    uv[index * verticesCount + 2] = new Vector2(1, 1);
                    uv[index * verticesCount + 3] = new Vector2(1, 0);
                    triangles[index * trianglesCount + 0] = index * 4 + 0;
                    triangles[index * trianglesCount + 1] = index * 4 + 1;
                    triangles[index * trianglesCount + 2] = index * 4 + 2;

                    triangles[index * trianglesCount + 3] = index * 4 + 0;
                    triangles[index * trianglesCount + 4] = index * 4 + 2;
                    triangles[index * trianglesCount + 5] = index * 4 + 3;
                    colors[index * verticesCount + 0] = Color.black;
                    colors[index * verticesCount + 1] = Color.black;
                    colors[index * verticesCount + 2] = Color.black;
                    colors[index * verticesCount + 3] = Color.black;
                    continue;
                }
                uv[index * verticesCount + 0] = new Vector2(0, 0);
                uv[index * verticesCount + 1] = new Vector2(0, 1);
                uv[index * verticesCount + 2] = new Vector2(1, 1);
                uv[index * verticesCount + 3] = new Vector2(1, 0);

                triangles[index * trianglesCount + 0] = index * 4 + 0;
                triangles[index * trianglesCount + 1] = index * 4 + 1;
                triangles[index * trianglesCount + 2] = index * 4 + 2;

                triangles[index * trianglesCount + 3] = index * 4 + 0;
                triangles[index * trianglesCount + 4] = index * 4 + 2;
                triangles[index * trianglesCount + 5] = index * 4 + 3;
                if (x % (int)Size.x == 1 && y % (int)Size.y == 1) // Center
                { // Ignore all
                    colors[index * verticesCount + 0] = newCanvas[position.x, position.y].color;
                    colors[index * verticesCount + 1] = newCanvas[position.x, position.y].color;
                    colors[index * verticesCount + 2] = newCanvas[position.x, position.y].color;
                    colors[index * verticesCount + 3] = newCanvas[position.x, position.y].color;
                }
                else if (x % (int)Size.x == 0 && y % (int)Size.y == 0) // Bottom Left (2)
                {
                    colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y - 1].color, 0.5f);
                    colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 2] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                    if (newCanvas[position.x, position.y - 1].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                        colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);

                    }
                    if (newCanvas[position.x - 1, position.y].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                        // colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    }
                }
                else if (x % (int)Size.x == 0 && y % (int)Size.y == 1) // Middle Left (2,3)
                {
                    colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 2] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 3] = newCanvas[position.x, position.y].color; // Ignore
                }
                else if (x % (int)Size.x == 2 && y % (int)Size.y == 1) // Middle Right (1,0)
                {
                    colors[index * verticesCount + 0] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 1] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);
                }
                else  if (x % (int)Size.x == 1 && y % (int)Size.y == 0) // Bottom Middle (1,2)
                {
                    colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                    colors[index * verticesCount + 1] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 2] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                }
                else if (x % (int)Size.x == 1 && y % (int)Size.y == 2) // Top Middle (0,3)
                {
                    colors[index * verticesCount + 0] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 3] = newCanvas[position.x, position.y].color; // Ignore
                }
                else if (x % (int)Size.x == 0 && y % (int)Size.y == 2) // Top Left (3)
                {
                    colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 3] = newCanvas[position.x, position.y].color; // Ignore
                    if (newCanvas[position.x, position.y + 1].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                        colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);


                    }
                    if (newCanvas[position.x - 1, position.y].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x - 1, position.y].color, 0.5f);
                    }
                }
                else if (x % (int)Size.x == 2 && y % (int)Size.y == 0) // Bottom Right (1)
                {
                    colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                    colors[index * verticesCount + 1] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);
                    colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y - 1].color, 0.5f);
                    if (newCanvas[position.x, position.y - 1].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 0] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);
                        colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y - 1].color, 0.5f);

                    }
                    if (newCanvas[position.x + 1, position.y].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);

                    }
                }
                else if (x % (int)Size.x == 2 && y % (int)Size.y == 2) // Top Right (0)
                {
                    colors[index * verticesCount + 0] = newCanvas[position.x, position.y].color; // Ignore
                    colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y + 1].color, 0.5f);
                    colors[index * verticesCount + 3] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);
                    if (newCanvas[position.x, position.y + 1].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 1] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);
                        colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x, position.y + 1].color, 0.5f);

                    }
                    if (newCanvas[position.x + 1, position.y].color != newCanvas[position.x, position.y].color)
                    {
                        colors[index * verticesCount + 2] = Color.Lerp(newCanvas[position.x, position.y].color, newCanvas[position.x + 1, position.y].color, 0.5f);
                    }
                }
                colors[index * verticesCount + 0].a = newCanvas[position.x, position.y].Power / lightManager.maximumLevel;
                colors[index * verticesCount + 1].a = newCanvas[position.x, position.y].Power / lightManager.maximumLevel;
                colors[index * verticesCount + 2].a = newCanvas[position.x, position.y].Power / lightManager.maximumLevel;
                colors[index * verticesCount + 3].a = newCanvas[position.x, position.y].Power / lightManager.maximumLevel;
                
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.colors = colors;
        // mesh.bounds = new Bounds(Vector2.zero, Vector2.one * 1000f);
        #endregion

        GameObject gameObject = new GameObject($"Mesh {startX} {endX};{startY} {endY}", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.transform.position = new Vector2(offset.x * 3, offset.y * 3);
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, 1f);
        sw.Stop();
        print($"Creating single mesh took {sw.ElapsedMilliseconds} ms.");
    }
}
