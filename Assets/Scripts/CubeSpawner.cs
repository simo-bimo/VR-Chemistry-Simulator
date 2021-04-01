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
    public float spawnDist = 1.0f;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        OVRInput.Update();

        spawnPos = transform.position + transform.forward * spawnDist;
        //check update
        float indexDown = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        if(0.35f < indexDown && indexDown < 0.55f) { spawnCube(spawnPos); }
    }

    void spawnCube(Vector3 pos) {
        Instantiate(cube, pos, Quaternion.identity);
    }
}
