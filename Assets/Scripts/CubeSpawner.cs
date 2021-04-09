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
    public float spawnDist = 0.25f;
    public double coolDown = 2.0;
    public double timeLeft = 0.0;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        spawnPos = transform.position + transform.forward * spawnDist;
        //check update
        //float indexDown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        //if(0.35f < indexDown && indexDown < 0.55f &&) { spawnCube(spawnPos); }
        if(OVRInput.Get(OVRInput.Button.One, m_controller) && timeLeft <= 0) {spawnCube(spawnPos); }

        timeLeft -= 1.0 / Time.deltaTime;
    }
    [ContextMenu("Spawn Cube")]
    void spawnCube() {
        Instantiate(cube, spawnPos, Quaternion.identity);
        timeLeft = coolDown;
    }
    void spawnCube(Vector3 pos) {
        Instantiate(cube, pos, Quaternion.identity);
        timeLeft = coolDown;
    }
}
