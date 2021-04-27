using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Label : MonoBehaviour
{
    //joint to atom

    //value
    public int num = 1;

    private void Update() {
        gameObject.GetComponentInChildren<TextMesh>().text = num.ToString();
    }
    public bool isCorrect() {
        
        
        return false;
    }
}
