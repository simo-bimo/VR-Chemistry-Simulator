using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondManager : MonoBehaviour
{   
    [System.Serializable]
    protected struct RegistryEntry {
        public Atom parent;
        public Atom child;

        public RegistryEntry(Atom p, Atom c) {
            parent = p;
            child = c;
            //Bond b = new Bond();
            //b.create(h,c);
            //bond = null;
        }
        public RegistryEntry(Atom p, Atom c, Bond b) {
            parent = p;
            child = c;
        }
        public RegistryEntry(Bond b) {
            parent = b.primary;
            child = b.child;
        }
    }

    [SerializeField]
    protected List<RegistryEntry> bondRegistry = new List<RegistryEntry>();

    protected int m_renderMode;
    public int renderMode {
        get { return m_renderMode; }
    } //space filling is 0, ball and stick is 1

    [ContextMenu("Update RenderMode")]
    public void setRenderMode(int newRender = 0) {
        m_renderMode = newRender;
        GameObject[] atoms = GameObject.FindGameObjectsWithTag("Atom");
        for(int i = 0; i <atoms.Length; i++) { atoms[i].GetComponent<Atom>().updateRender(m_renderMode); }
    }

    public bool requestBond(Atom me, Atom other) {
        if(bondRegistry.Contains(new RegistryEntry(other, me))) {return false;}
        if(bondRegistry.Contains(new RegistryEntry(me, other))) {return false;}

        //is this a valid bond?
        if(me.holes == 0) {return false;}
        if(other.holes == 0) {return false;}

        createBond(me, other);
        return true;
    }

    private bool createBond(Atom prim, Atom other) {
        
        GameObject bondObj = new GameObject(string.Format("{0} and {1} bond.", prim.Z, other.Z));
        Bond bondComp = bondObj.AddComponent<Bond>();
        bondComp.create(prim, other, prim.regularScale/2);

        bondObj.transform.parent = prim.transform;
        bondObj.transform.localPosition = new Vector3(0,0,0);

        bondRegistry.Add(new RegistryEntry(prim, other));
        return true;
    }

    public bool deleteBond(Atom prim, Atom child) {
        return  bondRegistry.Remove(new RegistryEntry(prim, child));
    }
}
