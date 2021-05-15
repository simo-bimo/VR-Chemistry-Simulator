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

    private float oldIndexDown;

    // Update is called once per frame
    private void Update()
    {
        spawnPos = OVRInput.GetLocalControllerPosition(m_controller) + transform.forward * spawnDist;

        if(OVRInput.GetUp(OVRInput.Button.One, m_controller)) { spawnAtom(); }

        float indexdown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        if(indexdown == 1.0f && indexdown > oldIndexDown) {
            BondManager bManager = GameObject.FindGameObjectWithTag("bondManager").GetComponent<BondManager>();
            if(bManager.renderMode==0) {bManager.setRenderMode(1); }
            else {bManager.setRenderMode(0);}
        }
        
        if(OVRInput.GetUp(OVRInput.Button.Two, m_controller)) { spawnZ += 1; }

        //max and minimise spawnZ
        if(spawnZ < 1) {spawnZ = 1;}
        if(spawnZ > 10) {spawnZ = 1;}

        zDisplay.GetComponent<TextMesh>().text = spawnZ.ToString(); // set the display to the Z value

        oldIndexDown = indexdown;
    }

    [ContextMenu("spawnAtom")]
    public void spawnAtom() {
        GameObject newAtom = Instantiate(atomPrefab, spawnPos, Quaternion.identity);
        newAtom.GetComponent<Atom>().updateZ(spawnZ); // set its Z.
        timeSince = 0; //reset cooldown.
    }
}
