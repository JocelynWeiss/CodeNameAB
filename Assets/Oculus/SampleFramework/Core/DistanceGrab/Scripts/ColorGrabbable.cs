/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using UnityEngine;

namespace OculusSampleFramework
{
	// Simple component that changes color based on grab state.
    public class ColorGrabbable : OVRGrabbable
    {
        public static readonly Color COLOR_GRAB = new Color(1.0f, 0.5f, 0.0f, 1.0f);
        public static readonly Color COLOR_HIGHLIGHT = new Color(1.0f, 0.0f, 1.0f, 1.0f);

        private Color m_color = Color.black;
        private MeshRenderer[] m_meshRenderers = null;
        private bool m_highlight;

        public float m_lastGrabbed = 0.0f;
        
        public bool Highlight
        {
            get { return m_highlight; }
            set
            {
                m_highlight = value;
                UpdateColor();
            }
        }

        public void UpdateColor()
        {
            if (isGrabbed) SetColor(COLOR_GRAB);
            else if (Highlight) SetColor(COLOR_HIGHLIGHT);
            else SetColor(m_color);
        }

        override public void GrabBegin(OVRGrabber hand, Collider grabPoint)
        {
            base.GrabBegin(hand, grabPoint);
            UpdateColor();
        }

        override public void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
        {
            base.GrabEnd(linearVelocity, angularVelocity);
            UpdateColor();
            m_lastGrabbed = Time.time;
        }

        void Awake()
        {
            if (m_grabPoints.Length == 0)
            {
                // Get the collider from the grabbable
                Collider collider = this.GetComponent<Collider>();
                if (collider == null)
                {
				    throw new System.ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
                }
    
                // Create a default grab point
                m_grabPoints = new Collider[1] { collider };

                // Grab points are doing double-duty as a way to identify submeshes that should be colored.
                // If unspecified, just color self.
                m_meshRenderers = new MeshRenderer[1];
                m_meshRenderers[0] = this.GetComponent<MeshRenderer>();
            }
            else
            {
                m_meshRenderers = this.GetComponentsInChildren<MeshRenderer>();
            }
            /*
            m_color = new Color(
                Random.Range(0.1f, 0.95f),
                Random.Range(0.1f, 0.95f),
                Random.Range(0.1f, 0.95f),
                1.0f);
            */
            //m_color = Color.green;
            SetColor(m_color);
        }

        private void SetColor(Color color)
        {
            for (int i = 0; i < m_meshRenderers.Length; ++i)
            {
                MeshRenderer meshRenderer = m_meshRenderers[i];
                for (int j = 0; j < meshRenderer.materials.Length; ++j)
                {
                    Material meshMaterial = meshRenderer.materials[j];
                    //meshMaterial.color = color; // //Jow: Does nothing...
                    //meshMaterial.SetColor("_BaseColor", color);
                    meshMaterial.SetColor("_EmissionColor", color);
                }
            }

            /*
            Renderer rd = GetComponentInParent<Renderer>();
            if (rd)
            {
                //rd.material.SetColor("_BaseColor", Color.yellow);
                //rd.material.SetColor("_BaseColor", Color.green);
            }
            */
        }


        // Jow: Free the grabber so it can grab again. Allow will set if this object can be grabbed again
        public void ForceRelease(bool _allowGrab)
        {
            if (grabbedBy != null)
            {
                m_allowOffhandGrab = _allowGrab;
                grabbedBy.ForceRelease(this);
            }
        }


        /*
        // Jow: Just so we can clearly see if the colliders are working
        private void OnTriggerEnter(Collider other)
        {
            Highlight = true;
        }


        // Jow: Just so we can clearly see if the colliders are working
        private void OnTriggerExit(Collider other)
        {
            Highlight = false;
        }
        */
    }
}
