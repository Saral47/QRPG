using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomGrid : MonoBehaviour
{
    private List<Vector2> gridNodes = new List<Vector2>();

    //For future implementation of custon width and height spacing between tiles
    private float xdist = 0;
    private float ydist = 0;

    public GameObject gridBaseObject;

    private void Start()
    {
        computeGrid();
    }
    public void computeGrid()
    {
        int nX = (int)Manager.instance.numTilesX / 2 * 2;
        int nY = (int)Manager.instance.numTilesY / 2 * 2;
        int tileSize = Manager.instance.tileSize;
        gridBaseObject.transform.localScale = new Vector3(tileSize, 1, tileSize);

        gridNodes = new List<Vector2>();
        for (int i = -nX / 2; i < nX / 2; i = i + 1)
        {
            for (int j = -nY / 2; j < nY / 2; j = j + 1)
            {
                float x = i * tileSize + (i - 1) * xdist + tileSize / 2.0f;
                float y = j * tileSize + (j - 1) * ydist + tileSize / 2.0f;
                GameObject go = Instantiate(gridBaseObject, new Vector3(x, 0f, y), Quaternion.identity);
                go.transform.SetParent(transform);
                gridNodes.Add(new Vector2(x, y));
            }
        }
    }

}
