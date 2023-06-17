
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class ReversiLogic : UdonSharpBehaviour
{
    [SerializeField]
    private GameObject DiskPrefab;

    void Start()
    {
        GameObject test = Instantiate(DiskPrefab);
        test.GetComponent<ReversiDisk>().Setup(gameObject, true);
    }
}
