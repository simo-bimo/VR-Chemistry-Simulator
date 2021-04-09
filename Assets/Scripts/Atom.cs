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
/* 0 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 1 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 2 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 3 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 4 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 5 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 6 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 7 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 8 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  },
/* 9 */  {0.200f,    0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,     0.200f,  }
    };
    
    //proton count
    public int Z = 1;

    public int valenceElectrons = 1; //ie. 4 for C, 6 for O.
    
    public int valenceShellSize = 2; //ie 8 for C or O.
    
    public int holes = 1; //unfilled spots for electrons in the valence shell, calculated every frame under update();
    public FixedJoint[] bonds; //a list of all the bonds the atom has, FixedJoint is a unity component that can attach two gameobjects.
    
    //to make sure that only one atom runs the bond() function, assigning them all a rank randomly, highest rank runs the function
    public int rank; 
    //when an atom spawns.
    private void Start() {
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
        bonds = new FixedJoint[valenceShellSize/2];

        //set the rank to something random
        Random.InitState((int)Time.time); //seed the randomness with the current time
        rank = Random.Range(0, 0x7FFFFFFF); //smallest to biggest int
    }

    //every frame.
    private void Update() {
        //update hole count
        holes = valenceShellSize - valenceElectrons;
    }

    //when two atoms hit eachother
    void OnCollisionEnter(Collision other) {        
        //do bonding
        Atom otherAtom;
        if(otherAtom = other.gameObject.GetComponent<Atom>()) { //only do stuff if it's actually got an atom component.
            if(isHighestRank(otherAtom)) { //if we have the higher rank, proceed.
                //bond, as long as there's holes to fill on both ends.
                if(holes > 0 && otherAtom.holes > 0) { bond(otherAtom); }
            }
        }
    }

    //when we call it.
    void bond(Atom otherAtom) {
        valenceElectrons +=1; //we've borrowed one of theirs.
        otherAtom.valenceElectrons +=1; //they've also borrowed one of ours.

        /*
        Create a bond, loop through the list of bonds, and fill in the first one that's null
        If none of them are null, that means we can't do any bonding, so something in our first check went wrong.
        */

        for(int i = 0; i < bonds.Length; i++) {
            if (!bonds[i]) { //check its null
                 //teleport the other gameobject to this anchor point, so they're always at the right distance apart.
                otherAtom.gameObject.transform.position = gameObject.transform.position + (((otherAtom.gameObject.transform.position - gameObject.transform.position).normalized * bondLength[Z, otherAtom.Z]));

                //make the bond
                bonds[i] = gameObject.AddComponent<FixedJoint>(); //create the bond as part of the bonds[] list
                bonds[i].connectedBody = otherAtom.gameObject.GetComponent<Rigidbody>(); //connect it to the other atom

                //make them connected to us by the vector (Us to Them) * bond distance
                //in future this should be replaced by some logic to make the bonds distribut approriately based on how many other electron pairs there are.
                bonds[i].anchor = ((otherAtom.gameObject.transform.position - gameObject.transform.position).normalized * bondLength[Z, otherAtom.Z]); 
                
                //make sure the atoms don't keep colliding with eachother.
                bonds[i].enableCollision = false;

                //update other atom:
                for(int l = 0; l < bonds.Length; l++) {
                    if (!otherAtom.bonds[l]) { //check its null
                        otherAtom.bonds[l] = bonds[i]; //set its bond list to be the same as ours
                    }
            }
            }
        }
    }

    //compare sizes:
    bool isHighestRank(Atom otherAtom) {
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
}
