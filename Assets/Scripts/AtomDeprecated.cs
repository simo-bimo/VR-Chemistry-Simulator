using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class AtomDeprecated : MonoBehaviour
{   
    //a static array of bond lengths for [element 1, element 2], set currently so they're all the same length regardless of 
    private float[,] bondLength = {
/* Z1 \ Z2 = 0         1           2           3           4           5           6           7           8           9  */
/* 0 */  {0.200f,    0.200f,     0.200f,     0.250f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 1 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 2 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 3 */  {0.250f,    0.200f,     0.200f,     0.400f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 4 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 5 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 6 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 7 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 8 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 9 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  }
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
    public FixedJoint[] bonds; //a list of all the bonds the AtomDeprecated has, FixedJoint is a unity component that can attach two gameobjects.
    public Vector3[] bondDirections;
    
    //to make sure that only one AtomDeprecated runs the bond() function, assigning them all a rank randomly, highest rank runs the function
    public int rank; 
    //when an AtomDeprecated spawns.
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
    }

    //when two AtomDeprecateds hit eachother
    void OnCollisionEnter(Collision other) {        
        //do bonding
        AtomDeprecated otherAtomDeprecated;
        if(otherAtomDeprecated = other.gameObject.GetComponent<AtomDeprecated>()) { //only do stuff if it's actually got an AtomDeprecated component.
            if(isHighestRank(otherAtomDeprecated)) { //if we have the higher rank, proceed.
                //bond, as long as there's holes to fill on both ends.
                if(holes > 0 && otherAtomDeprecated.holes > 0) { bond(otherAtomDeprecated); }
            }
        }
    }

    //when we call it.
    void bond(AtomDeprecated otherAtomDeprecated) {
        valenceElectrons +=1; //we've borrowed one of theirs.
        otherAtomDeprecated.valenceElectrons +=1; //they've also borrowed one of ours.

        /*
        Create a bond, loop through the list of bonds, and fill in the first one that's null
        If none of them are null, that means we can't do any bonding, so something in our first check went wrong.
        */

        for(int i = 0; i < bonds.Length; i++) {
            if (!bonds[i]) { //check its null
                Debug.Log($"i = {i}");

                //make the bond
                bonds[i] = gameObject.AddComponent<FixedJoint>(); //create the bond as part of the bonds[] list
                bonds[i].connectedBody = otherAtomDeprecated.gameObject.GetComponent<Rigidbody>(); //connect it to the other AtomDeprecated

                bonds[i].anchor = (bondDirections[i]); //set to localscale temporarily for convenience 
                
                //make sure the AtomDeprecateds don't keep colliding with eachother.
                bonds[i].enableCollision = false;

                //set other mass to zero to avoid collisions
                bonds[i].connectedMassScale = 0.1f;

                //if its the first bond, set the bond to what it is, us to them.
                /*
                if(i==0){ bondDirections[i] = (otherAtomDeprecated.gameObject.transform.position - gameObject.transform.position).normalized; }
                else {
                    //Debug.Log($"Bond Directions [0] = {bondDirections[0].x}, {bondDirections[0].y}, {bondDirections[0].z}");

                    bondDirections[i] = Quaternion.AngleAxis(109.5f, Vector3.Cross(bondDirections[0], Vector3.up ) ) * bondDirections[0];

                    //Debug.Log($"Bond Directions [i] = {bondDirections[i].x}, {bondDirections[i].y}, {bondDirections[i].z}");

                    //bondDirections[i] = Quaternion.AngleAxis(120f * i, bondDirections[0]) * bondDirections[i];

                    //Debug.Log($"Bond Directions [i] = {bondDirections[i].x}, {bondDirections[i].y}, {bondDirections[i].z}");

                    //Debug.Log($"Rotation Angle = {Quaternion.Angle(Quaternion.AngleAxis(120f * i, bondDirections[0]), Quaternion.AngleAxis(109.5f, Vector3.Cross(bondDirections[0], Vector3.up ) ))}");
                    
                }
                */
                //debugging: trying to set rotation to zero to see if that has anything to do with it
                transform.rotation = Quaternion.identity;
                otherAtomDeprecated.transform.rotation = Quaternion.identity;

                //teleport the other gameobject to this the bond direction we've set so they're always at the right distance apart.
                otherAtomDeprecated.transform.position = transform.position + bondDirections[i] * bondLength[Z, otherAtomDeprecated.Z];


                //update other AtomDeprecated:
                for(int l = 0; l < bonds.Length; l++) {
                    if (!otherAtomDeprecated.bonds[l]) { //check its null
                        otherAtomDeprecated.bonds[l] = bonds[i]; //set its bond list to be the same as ours
                        break;
                    }
                }
                break;
            }
        }
    }

    //compare sizes:
    bool isHighestRank(AtomDeprecated otherAtomDeprecated) {
        if(otherAtomDeprecated.Z < Z) {return true; }
        if(otherAtomDeprecated.Z > Z) {return false; }
        if (otherAtomDeprecated.rank < rank) { return true; } // if we're bigger, go for it
        if (otherAtomDeprecated.rank == rank) { 
            //if we get the same rank, reshuffle
            Random.InitState((int)Time.time);
            rank = Random.Range(0, 0x7FFFFFFF);
            if (otherAtomDeprecated.rank < rank) { return true; } //check if we bigger this time
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

        bonds = new FixedJoint[maxbonds];
        bondDirections= new Vector3[maxbonds];


        //hardcode tetrahedral angles
        if(valenceShellSize == 8) {
            bondDirections[0] = new Vector3( 0.64f,  0.64f,  0.64f);
            bondDirections[1] = new Vector3(-0.64f, -0.64f,  0.64f);
            bondDirections[2] = new Vector3( 0.64f, -0.64f, -0.64f);
            bondDirections[3] = new Vector3(-0.64f,  0.64f, -0.64f);
        }

        //set the material colour based on Z
        Material[] newMats = gameObject.GetComponent<MeshRenderer>().materials; //load the material
        newMats[0].color = zColors[Z-1]; // set the colour
        gameObject.GetComponent<MeshRenderer>().materials = newMats; //reset the material

        //update size based on Z, sort of approximate with an explonential, in future should replace this with a table so the sizes are accurate
        //we could also update the sizes based on the size on the electrons, so ions can be different sizes.
        gameObject.transform.localScale = new Vector3( 0.2f * Mathf.Pow(1.05f,2*Z), 0.2f * Mathf.Pow(1.05f,2*Z), 0.2f * Mathf.Pow(1.05f,2*Z));
    }

    //override version that does the same thing, but lets you specify a new Z.
    public void updateZ(int newZ) {
        Z = newZ;
        updateZ();
    }
}