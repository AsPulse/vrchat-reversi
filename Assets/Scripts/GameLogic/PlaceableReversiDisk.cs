
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlaceableReversiDisk : UdonSharpBehaviour
{
    void Start()
    {
        
    }

    public ReversiCellStatus Color;
    private ReversiLogic ReversiLogic;
    private int Index;


    public void Setup(ReversiLogic logic, int index, Vector3 position, bool color)
    {
        Color = color ? ReversiCellStatus.BLACK : ReversiCellStatus.WHITE;
        ReversiLogic = logic;
        Index = index;
        transform.parent = logic.transform;
        transform.Translate(logic.transform.position + position, Space.World);
        transform.rotation = Quaternion.Euler(color ? 0 : 180f, 0, 0);
    }

    public override void Interact()
    {
        ReversiLogic.Flip(Index);
    }


}
