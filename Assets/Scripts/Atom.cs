using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Atom : MonoBehaviour
{   
    //a static array of bond lengths for [element 1, element 2], set currently so they're all the same length regardless of 
    private float[,] bondLength = {
/* Z1 \ Z2 = 0         1           2           3           4           5           6           7           8           9  */
/* 0 */  {0.200f,    0.200f,     0.200f,     0.400f,     0.400f,     0.400f,     0.400f,     0.500f,     0.600f,     0.600f,  },
/* 1 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.400f,     0.200f,     0.600f,     0.200f,  },
/* 2 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.400f,     0.200f,     0.200f,     0.200f,  },
/* 3 */  {0.400f,    0.200f,     0.200f,     0.400f,     0.200f,     0.200f,     0.400f,     0.200f,     0.200f,     0.200f,  },
/* 4 */  {0.400f,    0.200f,     0.200f,     0.200f,     0.700f,     0.200f,     0.400f,     0.200f,     0.200f,     0.200f,  },
/* 5 */  {0.400f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.400f,     0.200f,     0.200f,     0.200f,  },
/* 6 */  {0.400f,    0.400f,     0.400f,     0.400f,     0.400f,     0.400f,     0.700f,     0.200f,     0.200f,     0.200f,  },
/* 7 */  {0.500f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.700f,     0.200f,     0.200f,  },
/* 8 */  {0.600f,    0.600f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.700f,     0.200f,  },
/* 9 */  {0.600f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  }
    };
    
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
    
    //proton count
    public int Z = 1;

    public int valenceElectrons = 1; //ie. 4 for C, 6 for O.
    
    public int valenceShellSize = 2; //ie 8 for C or O.
    
    public int holes = 1; //unfilled spots for electrons in the valence shell, calculated every frame under update();
    
    //bonds are defined by these three lists. First one is the atom we're bonded to, second is the direction of the bond, third is the trigger for the bond.
    public Atom[] bonds;
    public Vector3[] bondDirections;
    public SphereCollider[] bondTriggers;
    
    //to make sure that only one atom runs the bond() function, assigning them all a rank randomly, highest rank runs the function
    public int rank; 

    //when an atom spawns.
    private void Start() {
        //set blah blah based on Z
        updateZ();

        //set the rank to something random
        Random.InitState((int)Time.time); //seed the randomness with the current time
        rank = Random.Range(0, 0x7FFFFFFF); //smallest to biggest int
    }

    //every frame.
    private void Update() {
        //update hole count
        holes = valenceShellSize - valenceElectrons;

        //loopthrough the bonds and update their position based on our vectors
        for (int i =0; i < bonds.Length; i++) {
            if(bonds[i] && transform.parent != bonds[i].transform) { bonds[i].transform.localPosition = bondDirections[i] * bondLength[Z, bonds[i].Z]; }
        }
        //same deal for bondTriggers and bondDirections
        for(int i = 0; i < bonds.Length; i++) {
            bondTriggers[i].center = bondDirections[i]*gameObject.GetComponent<SphereCollider>().radius;
        }

        //if all our holes are filled (heh) disable all the bondtriggers
        for(int i = 0; i < bonds.Length; i++){ if(bonds[i]) {bondTriggers[i].enabled = false;} }
    }

    //when two atoms hit eachother
    private void OnTriggerEnter(Collider other) {
        //some temp vars
        Atom otherAtom;
        
        if(otherAtom = other.gameObject.GetComponent<Atom>()) { //only do stuff if it's actually got an atom component.
            int closestI = 0;
            Vector3 usToThem = (other.transform.position-transform.position).normalized;

            //loop through the bond directions and find the index of which ever its closest too.
            for (int i =0; i < bondDirections.Length; i++){
                //if its closer than the current closest, it is the new closest index.
                if(Vector3.Dot(usToThem, bondDirections[i]) > Vector3.Dot(usToThem, bondDirections[closestI])) { 
                    closestI = i; 
                }
            }

            if(isHighestRank(otherAtom)) { //if we have the higher rank, proceed.
                //bond, as long as there's holes to fill on both ends.
                if(holes > 0 && otherAtom.holes > 0) { 
                    bond(otherAtom, closestI); 
                }
            }
    }
    }

    //when we call it.
    void bond(Atom otherAtom, int index) {
        valenceElectrons +=1; //we've borrowed one of theirs.
        otherAtom.valenceElectrons +=1; //they've also borrowed one of ours.

        if (!bonds[index]) { //check we aren't overwritting an existing bond
            //set parent
            otherAtom.transform.parent = transform;
            otherAtom.transform.localPosition = bondDirections[index] * bondLength[Z, otherAtom.Z];

            //calculate their closest bond index
            int closestI = 0;
            //Vector3 ThemToUs = (transform.position-otherAtom.transform.position).normalized;
            Vector3 ThemToUs = -otherAtom.transform.localPosition.normalized;
            gizmons.Add(new Vector3[] {otherAtom.transform.position, otherAtom.transform.position+ThemToUs, new Vector3(0.0f,0.0f,1.0f)} );


            //loop through the bond directions and find the index of which ever its closest too.
            for (int i =0; i < otherAtom.bondDirections.Length; i++){
                //if its closer than current closest, it is the new supreme index.
                if(Vector3.Dot(ThemToUs, otherAtom.bondDirections[i]) > Vector3.Dot(ThemToUs, otherAtom.bondDirections[closestI])) { closestI = i; }
            }

            gizmons.Add(new Vector3[] {otherAtom.transform.position, otherAtom.transform.position+otherAtom.bondDirections[closestI], new Vector3(1.0f,0.0f,0.0f)} );

            //rotation from forward to up, as thats what trans.LookAt requires
            Quaternion relativerot = Quaternion.FromToRotation(Vector3.forward, otherAtom.bondDirections[closestI]);
            
            //rotate it so the corresponding bond is inside us:
            otherAtom.transform.rotation = Quaternion.LookRotation(ThemToUs)*relativerot;
            gizmons.Add(new Vector3[] {otherAtom.transform.position, otherAtom.transform.position+otherAtom.bondDirections[closestI], new Vector3(0.0f,1.0f,0.0f)} );

            otherAtom.GetComponent<Rigidbody>().isKinematic = true;

            Physics.IgnoreCollision(otherAtom.GetComponent<Collider>(), gameObject.GetComponent<Collider>());

            //update other atom:
            for(int l = 0; l < otherAtom.bonds.Length; l++) {
                if (!otherAtom.bonds[l]) { //check its null
                    otherAtom.bonds[l] = this; //set its bond list to be the same as ours
                    break;
                }
            }

            bonds[index] = otherAtom;

            //break;
            bondTriggers[index].enabled = false;
            otherAtom.bondTriggers[closestI].enabled = false;
        }
    }

    //compare sizes:
    bool isHighestRank(Atom otherAtom) {
        if(otherAtom.Z < Z) {return true; }
        if(otherAtom.Z > Z) {return false; }
        if (otherAtom.rank < rank) { return true; } // if we're bigger, go for it
        if (otherAtom.rank == rank) { 
            //if we get the same rank, reshuffle
            Random.InitState((int)Time.time);
            rank = Random.Range(0, 0x7FFFFFFF);
            if (otherAtom.rank < rank) { return true; } //check if we bigger this time
        }
        //by default, give up and go home.
        return false;
    }

    //update all the other stuff once Z is updated, just here so that the controller can call it to set Z dynamically.
    [ContextMenu("Update Z")]
    public void updateZ() {

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

        bonds = new Atom[maxbonds];
        bondDirections= new Vector3[valenceShellSize/2];
        bondTriggers = new SphereCollider[maxbonds];


        //hardcode tetrahedral angles
        if(valenceShellSize == 8) {
            bondDirections[0] = new Vector3( 0.64f,  0.64f,  0.64f);
            bondDirections[1] = new Vector3(-0.64f, -0.64f,  0.64f);
            bondDirections[2] = new Vector3( 0.64f, -0.64f, -0.64f);
            bondDirections[3] = new Vector3(-0.64f,  0.64f, -0.64f);
        }

        if(valenceShellSize == 2) {
            bondDirections[0] = Vector3.up;
        }

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
        gameObject.transform.localScale = new Vector3( 0.2f * Mathf.Pow(1.05f,2*Z), 0.2f * Mathf.Pow(1.05f,2*Z), 0.2f * Mathf.Pow(1.05f,2*Z));
        
    }

    //override version that does the same thing, but lets you specify a new Z.
    public void specUpdateZ(int newZ) {
        Z = newZ;
        updateZ();
    }

    public List<Vector3[]> gizmons = new List<Vector3[]>();
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        for(int i = 0; i < gizmons.Capacity; i++) {
            Gizmos.color = new Color(gizmons[i][2].x, gizmons[i][2].y,gizmons[i][2].z, 1.0f );
            Gizmos.DrawLine(gizmons[i][0], gizmons[i][1]);
        }
    }
}