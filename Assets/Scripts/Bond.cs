using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bond : MonoBehaviour
{
    //my vars
    [SerializeField]
    protected Atom m_primary;
    [SerializeField]
    protected Atom m_child;
    [SerializeField]
    protected int m_bondorder;
    protected Vector3 bondDirection;
    [SerializeField]
    protected float bondLength;

    //access vars
    /// <summary>
    /// The parent atom.
    /// </summary>
    public Atom primary 
    {
        get { return m_primary; }
        set { m_primary = primary; }
    }
    public Atom child
    {
        get { return m_child; }
        set { m_child = child; }
    }
    public int bondorder {
        get { return m_bondorder; }
    }

    /* PUBLIC ACCESS METHODS */
    public bool create(Atom prim, Atom kid, float length = 0.4f, int order = 1) {
        //set primary
        m_primary = prim;
        m_child = kid;

        //set the bond order, defaults to 1; and the bondlength;
        m_bondorder = order;
        bondLength = length;
        
        //disable collision betweens them
        Physics.IgnoreCollision(m_primary.GetComponent<Collider>(), m_child.GetComponent<Collider>());

        //update valencces.
        m_primary.valenceElectrons += bondorder;
        m_child.valenceElectrons += bondorder;

        //set parent
        m_child.transform.parent = this.transform;

        if (m_primary.bonds.Count == 0) {
            
        }

        //the direction from parent to child.
        Vector3 primToChild = m_child.transform.localPosition.normalized;
        bondDirection = primToChild;
        
        
        //set the child so in its own transform, the parent is always directly above it, 
        //hence (0,1,0) is its primary axis. i.e. rotate (0,1,0) to (-primttochild)
        //this also happens every frame so that the rotation is functionalyl frozen,
        //except about the y axis;
        m_child.transform.localRotation = Quaternion.FromToRotation(Vector3.up, -primToChild);

        //Rigidbody rb = m_child.GetComponent<Rigidbody>();
        //Destroy(rb);

        //if it's not the first bond, we have to put it in the correct position
        if(m_primary.bonds != null) {
            bondDirection = m_primary.bondDirections[m_primary.bonds.Count];
        }

        // move it to the correct position;
        m_child.transform.localPosition = bondDirection * bondLength;// * inverseScalar;
        
        //add the created bond to its atoms lists.
        m_primary.bonds.Add(this);
        m_child.bonds[0] = this;

        Rigidbody rb = m_child.GetComponent<Rigidbody>();
        Destroy(rb);

        updateSticks();

        return true;
    }

    public Atom other(Atom me) {
        if (me == m_primary) {return m_child;}
        if (me == m_child) {return m_primary;}
        else { return null; }
    }

    /*public bool updateScale(float scalar) {
        gameObject.transform.localScale = scalar*(new Vector3(1,1,1));
        updateSticks();
        return true;
    }
*/

    /* PRIVATE ACCESS METHODS */
    private void Update() {
        //set the child so the parent is always above it in local space.
        //Functionally freezes rotation except about the y-axis.
        Vector3 primToChild = m_child.transform.localPosition.normalized;
        float currentRot = m_child.transform.eulerAngles.y;
        Quaternion relativeRot = Quaternion.FromToRotation(Vector3.up, -primToChild);
        //Vector3 EulerRot = relativeRot.eulerAngles;
        //EulerRot.y = currentRot;
        //relativeRot.eulerAngles = Eu

        

        m_child.transform.localRotation = relativeRot;
        //m_child.transform.localEulerAngles = new Vector3(m_child.transform.localEulerAngles.x,m_child.transform.localEulerAngles.z);

        m_child.transform.localPosition = bondDirection * bondLength;// * inverseScalar;


        //update grabpoints
        //just force releases any children, only lets you grab top dogs.
        Grabbable kid_grabbed = m_child.GetComponent<Grabbable>();
        if(kid_grabbed.isGrabbed) {
            OVRGrabber kidnapper = kid_grabbed.grabbedBy;
            kidnapper.ForceRelease(kid_grabbed);
        }
    }

    //util methods
    [ContextMenu("Update Sticks")]
    private void updateSticks() {
        //kill the children
        for (int i = 0; i < gameObject.transform.childCount; i++ ) { 
            if(gameObject.transform.GetChild(i).gameObject.name == "Cylinder") { 
                Destroy(gameObject.transform.GetChild(i).gameObject); 
            }
        }

        for (int i = 0; i < m_bondorder; i++) {
            GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            stick.transform.parent = gameObject.transform;
            stick.transform.localScale = (new Vector3(0.01f, bondLength/2f, 0.01f));

            stick.transform.localPosition = Quaternion.FromToRotation(Vector3.up, bondDirection)*(new Vector3(0.0f, bondLength/2f, (i*0.05f - (0.05f * (m_bondorder-1) ) / 2) ));
            stick.transform.rotation = Quaternion.FromToRotation(Vector3.up, bondDirection);

            Material[] newMats = stick.GetComponent<MeshRenderer>().materials;
            newMats[0].color = new Color(0.5f,0.5f,0.5f,1.0f);
            stick.GetComponent<MeshRenderer>().materials = newMats;
            
            Physics.IgnoreCollision(m_primary.GetComponent<Collider>(), stick.GetComponent<Collider>());
            Physics.IgnoreCollision(m_child.GetComponent<Collider>(), stick.GetComponent<Collider>());
        }
    }

    [ContextMenu("Kill")]
    public void contextKill(){
        breakBond();
    }

    ///<summary>
    ///Destroys the bond and removes it from the registry. Electron Overrides change how many electrons are added back to each bond.
    ///They are multiplied by the bond order.
    ///</summary>
    private void breakBond(int pElectronOverride = 1, int cElectronOverride = 1) {
        //remove us from their lists
        m_child.bonds.Remove(this);
        m_primary.bonds.Remove(this);
        BondManager bManager = GameObject.FindGameObjectWithTag("bondManager").GetComponent<BondManager>();
        
        //if its inside the parent, move it away a lil'
        if (m_child.transform.localPosition.magnitude < m_primary.regularScale - m_child.regularScale) {
            m_child.transform.localPosition = m_child.transform.localPosition.normalized * 1.01f * (m_primary.regularScale - m_child.regularScale);
        }

        m_child.transform.SetParent(null);

        m_primary.valenceElectrons += pElectronOverride*m_bondorder;
        m_child.valenceElectrons += cElectronOverride*m_bondorder;

        //enable collisions
        Physics.IgnoreCollision(m_child.GetComponent<Collider>(), m_primary.GetComponent<Collider>(), false);

        //commit murder
        if(bManager.deleteBond(m_primary, m_child)) {
            Destroy(this.gameObject);
        }
    }
}