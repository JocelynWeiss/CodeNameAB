using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if (UNITY_EDITOR)
using UnityEditor;
#endif

// To add a new linear item, right click on a UI component and do UI/Linear Progress Bar.

// Fill the fill image with the fillColour according to the mask image.
// Set the current pourcentage between minimum and maximum with current value.

// Radial setup:
// 1- Create an image for background with this script to it
// 2- Create a child image for the fill picture
// 3- Create a child image of the above to be the mask
// 4- Link to this script the fill and the mask child and set a default colour.

[ExecuteInEditMode()]
public class JowProgressBar : MonoBehaviour
{
#if (UNITY_EDITOR)
    [MenuItem("GameObject/UI/Linear Progress Bar")]
    public static void AddLinearProgressBar()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/Linear Progress Bar"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
    }


    [MenuItem("GameObject/UI/Radial Progress Bar")]
    public static void AddRadialProgressBar()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("UI/Radial Progress Bar"));
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
    }
#endif

    public int m_minimum;
    public int m_maximum = 100;
    public float m_cur;
    public Image m_fill;
    public Color m_fillColour;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetFillAmount();

#if (UNITY_EDITOR)
        // If the mouse is in the bottom third of the screen, set current value according to mouse X position.
        float mousePosY = Input.mousePosition.y;
        if (mousePosY < Screen.height * 0.33f)
        {
            //m_cur = (int)Input.mousePosition.x;
            //m_minimum = 0;
            //m_maximum = Screen.width;
            if (Input.mousePosition.x > 0)
            {
                m_cur = Input.mousePosition.x / (float)Screen.width * 100.0f;
            }
        }
#endif
    }


    // Sets the mask and the fill parameters according to current value, min and max.
    void SetFillAmount()
    {
        float curOffset = m_cur - m_minimum;
        float maxOffset = m_maximum - m_minimum;
        float fillAmount = 0.0f;
        if (maxOffset > 0.0f)
        {
            fillAmount = curOffset / maxOffset;
        }
        m_fill.fillAmount = fillAmount;
        m_fill.color = m_fillColour;
    }
}
