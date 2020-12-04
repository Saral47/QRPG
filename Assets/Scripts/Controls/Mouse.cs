using UnityEngine;
using System;
using System.Collections.Generic;

//This script handles the placement of Obstacles.
public class Mouse : MonoBehaviour
{
    public Camera cam;
    public GameObject highlightObject,highlight,land,monster,gold,exit,player;
    
    public GameObject sphere;
    private Ray highlightRay;
    private bool inBounds;



    void Start()
    {
        float tileSize = Manager.instance.tileSize;
        highlight.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        land.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        monster.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        gold.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        exit.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        player.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
    }

    void Update()
    {
       
        // SetHighlightObject(Manager)
     
        SetHighlightObject(Manager.instance.highlightState);
        Plane plane = new Plane(Vector3.up, 0);
        float distance;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            float x = SnapToGrid(worldPosition.x);
            float y = SnapToGrid(worldPosition.z);
            highlightObject.transform.position = new Vector3(x, worldPosition.y + 0.5f + Manager.instance.tileSize / 2, y);
            if (worldPosition.x > -Manager.instance.worldSizeX && worldPosition.x < Manager.instance.worldSizeX && worldPosition.z > -Manager.instance.worldSizeY && worldPosition.z < Manager.instance.worldSizeY)
            {
                if (Manager.instance.deleteState)
                {
                    //highlightObject = highlight;
                    highlightObject.GetComponent<Renderer>().material.color = Color.red;
                   
                }
                else
                    highlightObject.GetComponent<Renderer>().material.color = Color.green;
                inBounds = true;
            }   
            
            else
            {
                //highlightObject = highlight;
                highlightObject.GetComponent<Renderer>().material.color = Color.black;
                inBounds = false;
            }

            if (Input.GetMouseButtonDown(0) && Manager.instance.deleteState == false && CanSpawn(new Vector3(x,0.5f+Manager.instance.tileSize/2,y), Manager.instance.tileSize/2) && inBounds )
            {
                Manager.instance.AddTile((int)x,(int)y);
            }
            
            if(Input.GetMouseButtonDown(0) && inBounds && Manager.instance.deleteState == true)
            {
                DeleteTile(new Vector3(x, 0.5f + Manager.instance.tileSize / 2, y), Manager.instance.tileSize / 2);
                Debug.Log("DELETING: "+x+" "+y);
                DeleteTile(new Vector3(x, 0.5f + Manager.instance.tileSize / 2, y), Manager.instance.tileSize / 2);
            }
        }  
       
    }

    public void SetHighlightObject(int state)
    {
       
        highlightObject.SetActive(false);
        if (state == 1)
            highlightObject = land;
        else if (state == 2)
            highlightObject = player;
        else if (state == 3)
            highlightObject = gold;
        else if (state == 4)
            highlightObject = exit;
        else if (state == 5)
            highlightObject = monster;
        else
            highlightObject = highlight;

        highlightObject.SetActive(true);
    }

    public float SnapToGrid(float val)
    {
        int d = (int)val / Manager.instance.tileSize;
        return d * Manager.instance.tileSize + Mathf.Sign(val)* Manager.instance.tileSize / 2.0f;
    }

    private bool CanSpawn(Vector3 pos, float radius)
    {
        radius = radius * 0.95f;
        Collider[] colliders = Physics.OverlapSphere(pos, radius);
      
        //GameObject got = Instantiate(sphere, pos, Quaternion.identity);
        //got.transform.localScale = new Vector3(radius * 2, radius * 2, radius * 2);

        foreach (Collider collider in colliders)
        {
            GameObject go = collider.gameObject;

            if ( go.transform.CompareTag("Land"))
            {
                Debug.Log("CANT SPAWN");
                return false;
            }
        }

        return true;
    }

    private void DeleteTile (Vector3 pos, float radius)
    {
        radius = radius * 0.95f;
        Collider[] colliders = Physics.OverlapSphere(pos, radius);

        foreach (Collider collider in colliders)
        {
            GameObject go = collider.gameObject;

            if (go.tag == "Tile")
                go.GetComponent<Tile>().DeleteSelf();
        }

    }
}
      