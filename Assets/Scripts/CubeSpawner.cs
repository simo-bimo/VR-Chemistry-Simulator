using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour
{
    //controller
    [SerializeField]
    protected OVRInput.Controller m_controller;
    public GameObject cube;
    private Vector3 spawnPos;
    public float spawnDist = 0.4f;
    public double coolDown = 0.1;
    public double timeSince = 2.0;

    // Update is called once per frame
    private void Update()
    {
        OVRInput.Update();

        spawnPos = transform.position + transform.forward * spawnDist;
        //check update
        //float indexDown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, montroller);
        //if(0.35f < indexDown && indexDown < 0.55f &&) { spawnCube(spawnPos); }
        if(OVRInput.Get(OVRInput.Button.One, m_controller) && timeSince >= coolDown) {spawnCube(spawnPos); }

        timeSince += 1.0 * Time.deltaTime;
    }

    void spawnCube(Vector3 pos) {
        Instantiate(cube, pos, Quaternion.identity);
        timeSince = 0;
    }
}
