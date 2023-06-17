
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractTest : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    public override void Interact()
    {
        Debug.Log("Interact!");
    }
}
