using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


// Make sure this is attached to the gameobject with an active camera


public class GlLineSystem : MonoBehaviour
{
    public Material m_mat;
    public Camera m_cam;
    Vector3[] m_lines;
    Color[] m_colors;

    // Start is called before the first frame update
    void Start()
    {
        //Camera.onPostRender += OnPostRenderCallback;
        RenderPipelineManager.endCameraRendering += EndCameraRenderingCB;
    }


    public void InitLineNumber(int nb)
    {
        m_lines = new Vector3[nb * 2];
        m_colors = new Color[nb];
    }

    // Update is called once per frame
    void Update()
    {
    }


    void OnPostRender()
    {
        DrawLines();
    }


    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        DrawLines();
    }


    // Unity calls the methods in this delegate's invocation list before rendering any camera
    void OnPostRenderCallback(Camera cam)
    {
        Debug.Log("Camera callback: Camera name is " + cam.name);

        // Unity calls this for every active Camera.
        if (cam == Camera.main)
        {
            // Put your custom code here
            DrawLines();
        }
    }


    private void EndCameraRenderingCB(ScriptableRenderContext context, Camera camera)
    {
        //Debug.Log("Camera callback: Camera name is " + camera.name);
        OnPostRender();
    }


    void DrawLines()
    {
        GL.PushMatrix();
        m_mat.SetPass(0);
        GL.MultMatrix(Matrix4x4.identity);

        GL.Begin(GL.LINES);
        /*
        GL.Color(Color.red);
        //GL.Vertex(startVertex);
        GL.Vertex3(-1.0f, 0.0f, 0.0f);
        GL.Color(Color.white);
        GL.Vertex(new Vector3(10.0f, 50.0f, 100.0f));

        GL.Color(Color.green);
        GL.Vertex3(1.0f, 0.0f, 0.0f);
        GL.Color(Color.white);
        GL.Vertex(new Vector3(10.0f, 50.0f, 100.0f));
        */

        /*
        float radius = 3.0f;
        int lineCount = 100;
        for (int i = 0; i < lineCount; ++i)
        {
            float a = i / (float)lineCount;
            float angle = a * Mathf.PI * 2;
            // Vertex colors change from red to green
            GL.Color(new Color(a, 1 - a, 0, 0.8F));
            // One vertex at transform position
            GL.Vertex3(0, 0, 0);
            // Another vertex at edge of circle
            GL.Vertex3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
        }
        */

        for (int i=0; i < m_colors.Length; i++)
        {
            GL.Color(m_colors[i]);
            GL.Vertex(m_lines[i * 2]);
            GL.Color(Color.white);
            GL.Vertex(m_lines[i * 2 + 1]);
        }

        GL.End();
        GL.PopMatrix();
    }


    private void OnDestroy()
    {
        //Camera.onPostRender -= OnPostRenderCallback;
        RenderPipelineManager.endCameraRendering -= EndCameraRenderingCB;
    }


    public void SetLine(int idx, Vector3 start, Vector3 end, Color col)
    {
        if (m_lines == null)
            return;

        if (idx >= m_colors.Length)
            return;

        m_lines[idx * 2 + 0] = start;
        m_lines[idx * 2 + 1] = end;
        m_colors[idx] = col;
    }
}
