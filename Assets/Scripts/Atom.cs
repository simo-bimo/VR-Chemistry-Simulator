using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Atom : MonoBehaviour
{    
    //table of colours to assign to elements
    private Color[] zColors = {
        new Color((255f /255f), (255f /255f), (255f /255f), (255f/255f)), //H  - white
        new Color((217f /255f), (255f /255f), (255f /255f), (255f/255f)), //He - very light blue
        new Color((204f /255f), (128f /255f), (225f /255f), (255f/255f)), //Li - light purple
        new Color((194f /255f), (255f /255f), (  0f /255f), (255f/255f)), //Be - lime
        new Color((255f /255f), (181f /255f), (181f /255f), (255f/255f)), //B  - pinkish
        new Color((144f /255f), (144f /255f), (144f /255f), (255f/255f)), //C  - grey
        new Color(( 48f /255f), ( 80f /255f), (248f /255f), (255f/255f)), //N  - blue
        new Color((255f /255f), ( 13f /255f), ( 13f /255f), (255f/255f)), //O  - red
        new Color((144f /255f), (224f /255f), ( 80f /255f), (255f/255f)), //F  - light green 
        new Color((179f /255f), (227f /255f), (245f /255f), (255f/255f))  //Ne - light blue
        };
    
    public int Z = 1;
    public int valenceElectrons = 1; //ie. 4 for C, 6 for O.
    public int valenceShellSize = 2; //ie 8 for C or O.

    protected int m_holes = 1; //unfilled spots for electrons in the valence shell, calculated every frame under update();
    public int holes { get { return m_holes; } }

    public int molecularmass = 0; // the whole groups mass
    private float lewisScale = 0.1f;
    public float regularScale;
    
    //bonds are defined by these three lists. First one is the atom we're bonded to, second is the direction of the bond, third is the trigger for the bond.
    public List<Bond> bonds;
    public Vector3[] bondDirections;
    public SphereCollider[] bondTriggers;
    private BondManager bManager;

    private void Start() {
        bManager = GameObject.FindObjectOfType<BondManager>();

        bonds.Add(null);

        updateZ();
        updateDirections();
    }
    private void Update() {
        m_holes = valenceShellSize - valenceElectrons;

        //if all our m_holes are filled (heh) disable all the bondtriggers
        if(m_holes == 0) { for(int i = 0; i < bondTriggers.Length; i++){ bondTriggers[i].enabled = false;} }

        molecularmass = 0;
        Atom[] atomicChildren = gameObject.GetComponentsInChildren<Atom>();
        for (int i = 0; i < atomicChildren.Length; i++) {
            molecularmass += atomicChildren[i].Z;
        }
    }

    //when two atoms hit eachother
    private void OnTriggerEnter(Collider other) {     
        //Ask the bondmanager to make a bond, if it does, disable the bond collider;
        if (other.gameObject.tag == "Atom") {
            gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            bManager.requestBond(this, other.gameObject.GetComponent<Atom>());
            gameObject.GetComponent<Rigidbody>().detectCollisions = true;
        }
    }

    public void updateRender(int renderMode) {
        if (renderMode == 1) {
            updateScales(lewisScale);
        }
        regularScale = 0.2f * Mathf.Pow(1.05f,2*Z);
        if (renderMode == 0) {
            updateScales(regularScale);
        }
    }
    //update all the other stuff once Z is updated, just here so that the controller can call it to set Z dynamically.
    [ContextMenu("Update Z")]
    public void yeet() {
        updateZ();
    }
    public void updateZ(int newZ = 0) {
        //update the Z if necessary.
        if(newZ != 0) { Z = newZ; }

        switch (Z) {
            case int Z when (Z <=2): //Z is 1-2, H or He.
                valenceElectrons = Z;
                valenceShellSize = 2;
                break;
            case int Z when (Z <=10 && Z >2): //Z is 3-10, Li through Ne.
                valenceElectrons = Z-2;
                valenceShellSize = 8;
                break;
        }

        //max number of bonds is number of valence electron pairs.
        int maxbonds = 0;
        if(valenceElectrons < valenceShellSize/2) { // prefers to donate
            maxbonds = valenceElectrons;
        }
        if(valenceElectrons >= valenceShellSize/2) {// prefers to grab
            maxbonds = valenceShellSize-valenceElectrons;
        }

        //remove spherecolliders before resetting
        for (int i = 0; i < bondTriggers.Length; i++) {
            Destroy(bondTriggers[i]);
        }

        bondDirections= new Vector3[valenceShellSize/2];
        bondTriggers = new SphereCollider[maxbonds];

        updateDirections();

        //set bondTriggers
        for (int i = 0; i < bondTriggers.Length; i++) {
            bondTriggers[i] = gameObject.AddComponent<SphereCollider>();
            bondTriggers[i].radius = 0.3f;
            bondTriggers[i].isTrigger = true;
            bondTriggers[i].center = bondDirections[i]*gameObject.GetComponent<SphereCollider>().radius;

            //debugging tool to be able to see it
            //Gizmos.DrawWireSphere(transform.position+bondTriggers[i].center, bondTriggers[i].radius);
        }

        //set the material colour based on Z
        Material[] newMats = gameObject.GetComponent<MeshRenderer>().materials; //load the material
        newMats[0].color = zColors[Z-1]; // set the colour
        gameObject.GetComponent<MeshRenderer>().materials = newMats; //reset the material

        //update size based on Z, sort of approximate with an explonential, in future should replace this with a table so the sizes are accurate
        //we could also update the sizes based on the number of the electrons, so ions can be different sizes.
        regularScale = 0.2f * Mathf.Pow(1.05f,2*Z);
        gameObject.transform.localScale = regularScale*(new Vector3(1,1,1));
        
    }
    public void updateDirections(float bondAngle = 104.5f) {
        bondDirections[0] = Vector3.up;

        if (bondDirections.Length == 4) {
            Quaternion upToRight = Quaternion.AngleAxis(bondAngle, Vector3.forward);
            bondDirections[1] = upToRight * bondDirections[0];
            Quaternion relativeRot = Quaternion.AngleAxis(120f, Vector3.up);
            bondDirections[2] = relativeRot*bondDirections[1];
            bondDirections[3] = relativeRot*bondDirections[2];
        }
    }
    private void updateScales(float scalar) {
        //gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.transform.localScale = scalar*(new Vector3(1,1,1));
        for(int i = 0; i < transform.childCount; i ++) {
            if (!transform.GetChild(i).GetComponent<Atom>())transform.GetChild(i).transform.localScale = (1/scalar)*(new Vector3(1,1,1));
        }
    }

    /* DEBUGGING STUFF */
    public List<Vector3[]> gizmons = new List<Vector3[]>();
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        for(int i = 0; i < gizmons.Capacity; i++) {
            Gizmos.color = new Color(gizmons[i][2].x, gizmons[i][2].y,gizmons[i][2].z, 1.0f );
            Gizmos.DrawLine(gizmons[i][0], gizmons[i][1]);
        }
    }
}