    
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;


public class ReversiLogic : UdonSharpBehaviour
{
    
    [SerializeField]
    private GameObject DiskPrefab;

    private ReversiCoordinateManager Coord;
    private GameObject[] DiskInstances;


    void Start()
    {
        Coord = GetComponent<ReversiCoordinateManager>();
        Coord.SetUp(DiskPrefab);
        DiskInstances = new GameObject[Coord.WIDTH * Coord.HEIGHT];
        for(int i = 0; i < DiskInstances.Length; i++)
        {
            DiskInstances[i] = null;
        }
        SyncBoard();
    }

    void SyncBoard()
    {

        for (int y = 0; y < Coord.HEIGHT; y++)
        {
            for (int x = 0; x < Coord.WIDTH; x++)
            {
                GameObject target = DiskInstances[Coord.CoordinateToIndex(x, y)];
                ReversiCellStatus status = Coord.GetCell(x, y);
                if (target == null)
                {
                    if (status == ReversiCellStatus.NONE) continue;
                    GameObject generated = Instantiate(DiskPrefab);
                    generated.GetComponent<ReversiDisk>().Setup(gameObject, Coord.GetCellVector(x, y), status == ReversiCellStatus.BLACK);
                } else {   
                    if (status == ReversiCellStatus.NONE)
                    {
                        Debug.LogWarning("Removed the disk once generated maybe due to sync.");
                        Destroy(target);
                        target = null;
                        continue;
                    }
                    target.GetComponent<ReversiDisk>().Color = status == ReversiCellStatus.BLACK;
                }
            }
        }
    }
}

public enum ReversiCellStatus : byte
{
    WHITE = 0,
    BLACK = 1,
    NONE = 2,
}

