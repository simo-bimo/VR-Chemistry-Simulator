using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtomSpawner : MonoBehaviour
{
    //controller
    [SerializeField]
    protected OVRInput.Controller m_controller;
    public GameObject atomPrefab;
    public GameObject zDisplay;
    private Vector3 spawnPos;
    public float spawnDist = 0.4f;
    public double coolDown = 0.05;
    public double timeSince = 2.0;
    
    public int spawnZ = 1; //the Z value to give to newly spawned atoms.
    private float netChange = 0f; //cumulative addition to Z based on thumbstick

    // Update is called once per frame
    private void Update()
    {
        OVRInput.Update();

        spawnPos = transform.position + transform.forward * spawnDist;
        //check update
        //float indexDown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        //if(0.35f < indexDown && indexDown < 0.55f &&) { spawnCube(spawnPos); }
        if(OVRInput.GetDown(OVRInput.Button.One, m_controller)) { spawnAtom(); }

        timeSince += Time.deltaTime;

        /*logic to change spawnZ based on thumbstick
        float x = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, m_controller).x; //get the sideways changedness of the thumbstick
        if (Mathf.Abs(x) > 0.5f) {
            netChange += x * Time.deltaTime * 5.0f;
        }

        if(netChange >= 1.0f) {
            spawnZ += Mathf.FloorToInt(netChange);
            netChange = 0.0f;
        }
        */
        if(OVRInput.GetUp(OVRInput.Button.Two, m_controller)) { spawnZ += 1; }

        //max and minimise spawnZ
        if(spawnZ < 1) {spawnZ = 1;}
        if(spawnZ > 10) {spawnZ = 1;}

        zDisplay.GetComponent<TextMesh>().text = spawnZ.ToString(); // set the display to the Z value
    }

    [ContextMenu("spawnAtom")]
    public void spawnAtom() {
        GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);
        newAtom.GetComponent<Atom>().specUpdateZ(spawnZ); // set its Z.
        timeSince = 0; //reset cooldown.
    }
}
