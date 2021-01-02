using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public int Lenght;
    public GameObject rope;
    public GameObject ropeEnd;
    private void Start()
    {
        Rigidbody2D currentRb = GetComponent<Rigidbody2D>();
        Transform currentParent = transform;
        for (int i = 0; i < Lenght+1; i++)
        {
            // yield return new WaitForSeconds(0.25f);
            if(i == Lenght)
            {
                GameObject end = Instantiate(ropeEnd, currentParent.position, Quaternion.identity);
                HingeJoint2D hingeJoint = end.GetComponent<HingeJoint2D>();
                hingeJoint.connectedBody = currentRb;
            }
            else
            {
                GameObject rope = Instantiate(this.rope, currentParent.position, Quaternion.identity);
                rope.name = $"Rope {i}";
                HingeJoint2D hingeJoint = rope.GetComponent<HingeJoint2D>();
                hingeJoint.connectedBody = currentRb;
                currentParent = rope.transform.Find("End");
                currentRb = rope.GetComponent<Rigidbody2D>();
            }
        }
    }
}
