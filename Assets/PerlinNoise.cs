
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public int boxSideLength;
    private Vector2 [,] pseudorandomGradientVectors; // element at i, j is for box-coordinate (i, j)
    public GameObject minicubePrefab;

    // Since box is centered at origin in world coordinates, need to convert between box coordinates (let upper left hand corner be 0, 0; x-axis to the right, y-axis down)
    Vector2 worldToBoxCoordinates(Vector3 worldCoordinates) {
        return new Vector2(worldCoordinates.x + boxSideLength / 2f, -worldCoordinates.y - boxSideLength / 2f);
    }

    Vector3 boxToWorldCoordinates(Vector2 boxCoordinates) {
        return new Vector3(boxCoordinates.x - boxSideLength / 2f, -boxCoordinates.y + boxSideLength / 2f, -0.5f);
    }

    void assignPseudorandomGradientVectors() {
        for (int i = 0; i < boxSideLength; i++) {
            for (int j = 0; j < boxSideLength; j++) {
                pseudorandomGradientVectors[i, j] = Random.insideUnitCircle.normalized;
            }
        }
    }

    float dotProductGradientCoordinate(int gridX, int gridY, float x, float y) {
        // take the dot product of the distance vector and the gradient vector at (gridX, gridY)
        // everything in box coordinates of course.
        Vector2 distance = new Vector2(x - gridX, y - gridY);
        return Vector2.Dot(pseudorandomGradientVectors[gridX, gridY], distance);
    }

    float perlin(float boxX, float boxY) {
        // compute the perlin noise at box coordinates x and y. Returns a number in [-1, 1]
        // sanity check
        if (boxX > boxSideLength || boxX < 0 || boxY > boxSideLength || boxY < 0) {
            throw new System.ArgumentException("perlin coordinates out of bounds");
        }
        // mostly following wikipedia's variable naming

        // start by finding nearest coordinates on grid
        int x0 = (int) boxX;
        int x1 = x0 + 1;
        int y0 = (int) boxY;
        int y1 = y0 + 1;
        
        // step 2 dot products
        float n00 = dotProductGradientCoordinate(x0, y0, boxX, boxY);
        float n01 = dotProductGradientCoordinate(x0, y1, boxX, boxY);
        float n10 = dotProductGradientCoordinate(x1, y0, boxX, boxY);
        float n11 = dotProductGradientCoordinate(x1, y1, boxX, boxY);

        // interpolation weights
        float sx = boxX - (float) x0;
        float sy = boxY - (float) y0;
        
        // do the actual interpolation-- if you do the algebra you'll see that it doesn't actually matter whether we do both x and then one y or two y then one x.
        // algebraically it has the same result.
        float ix0 = Mathf.Lerp(n00, n01, sx);
        float iy0 = Mathf.Lerp(n10, n11, sx);
        float result = Mathf.Lerp(ix0, iy0, sy);

        return result;
    }

    void applyPerlinNoise(float stepSize) {
        // TODO: do this a bit smarter-- something  to do with HLSL probably. shaders etc. this is just a hack to see the result.
        for (float i = 0; i < boxSideLength; i += stepSize) {
            for (float j = 0; j < boxSideLength; j += stepSize) {
                // create a child object, colored by the Perlin noise there.
                GameObject minicube = Instantiate(minicubePrefab);
                Vector2 minicubeBoxCoordinates = new Vector2(i, j);
                minicube.transform.position = boxToWorldCoordinates(minicubeBoxCoordinates) + new Vector3(0, 0, -0.5f);
                //Debug.Log(i + ", " + j + ", boxworld: " + boxToWorldCoordinates(minicubeBoxCoordinates).ToString() + ", " + minicube.transform.position.ToString());
                Color c = new Color(1f, (perlin(i, j) + 1) / 2, (perlin(j, i) + 1) / 2);
                minicube.GetComponent<Renderer>().material.SetColor("_Color", c);

            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        pseudorandomGradientVectors = new Vector2[boxSideLength + 1, boxSideLength + 1];
        transform.localScale = new Vector3(boxSideLength, boxSideLength, 1);
        assignPseudorandomGradientVectors();
        applyPerlinNoise(0.05f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
