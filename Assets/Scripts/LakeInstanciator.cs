using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LakeInstanciator : MonoBehaviour
{
    public Material material;
    public int nbRows = 0;
    public int nbLines = 0;
    public float m_scale = 1.0f;


    private int oldRows;
    private int oldLines;
    private Vector3 localPos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (nbRows != oldRows || nbLines != oldLines)
        {
            localPos = transform.position;
            //Delete all existing
            DeleteAllChildren();
            //Instanciate new planes
            InstanciateGrids(nbRows, nbLines);
            oldRows = nbRows;
            oldLines = nbLines;
        }
    }


    void DeleteAllChildren()
    {
        foreach (Transform child in this.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }


    void InstanciateGrids(int nbRows, int nbLines)
    {
        float offset = m_scale * 10.0f;
        for (int r = 0; r < nbRows; r++)
        {
            for (int l = 0; l < nbLines; l++)
            {
                GameObject grid = GameObject.CreatePrimitive(PrimitiveType.Plane);
                grid.transform.SetParent(this.transform);
                grid.transform.localScale = new Vector3(m_scale, 1.0f, m_scale);
                grid.transform.position = new Vector3(l * offset + localPos.x, localPos.y, r * offset + localPos.z);
                Renderer gridRenderer = grid.GetComponent<Renderer>();
                gridRenderer.material = material;
                gridRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                MeshCollider collider = grid.GetComponent<MeshCollider>();
                collider.enabled = false;
            }
        }
    }
}
