using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : OVRGrabbable
{
    override public void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        m_grabbedBy = hand;
        print(string.Format("{0} has begun being grabbed by {1}. The grabPoint is a child of {2}.", this.gameObject, m_grabbedBy.gameObject, grabPoint.gameObject));
        m_grabbedCollider = grabPoint;
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
}
