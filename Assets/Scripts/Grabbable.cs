using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : OVRGrabbable
{
    public void addGrabpoint(Collider newgrabpoint) {
        int length = m_grabPoints.Length;
        Collider[] tempPoints = new Collider[length];
        tempPoints = m_grabPoints;
        m_grabPoints = new Collider[length+1];
        for (int i = 0; i < length; i++) {
            m_grabPoints[i] = tempPoints[i];
        }
        grabPoints[length+1] = newgrabpoint;
    }
}
