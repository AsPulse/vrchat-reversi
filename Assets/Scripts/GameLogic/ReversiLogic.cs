
using System;
using System.Linq;
using tutinoco;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;


public class ReversiLogic : SimpleNetworkUdonBehaviour
{
    
    [SerializeField]
    private GameObject DiskPrefab;

    [SerializeField]
    private GameObject PlaceableDiskPrefab;

    private ReversiCoordinateManager Coord;
    private GameObject[] DiskInstances;
    private GameObject[] PlaceableDiskInstances;


    void Start()
    {
        SimpleNetworkInit();
        Coord = GetComponent<ReversiCoordinateManager>();
        Coord.SetUp(DiskPrefab);

        DiskInstances = new GameObject[Coord.WIDTH * Coord.HEIGHT];
        for(int i = 0; i < DiskInstances.Length; i++)
        {
            DiskInstances[i] = null;
        }

        PlaceableDiskInstances = new GameObject[Coord.WIDTH * Coord.HEIGHT];
        for(int i = 0; i < PlaceableDiskInstances.Length; i++)
        {
            PlaceableDiskInstances[i] = null;
        }

        SyncBoard();
    }

    public override void ReceiveEvent(string name, string value)
    {
        if(name == "StateSync")
        {
            Coord.Decode(value);
            SyncBoard();
            return;
        }
    }

    public void SendBoardStateSync()
    {
        ClearJoinSync();
        SendEvent("StateSync", Coord.Encode());
    }

    void SyncBoard()
    {
        DateTime start = DateTime.Now;

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
                    DiskInstances[Coord.CoordinateToIndex(x, y)] = generated;
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

        for (int y = 0; y < Coord.HEIGHT; y++)
        {
            for (int x = 0; x < Coord.WIDTH; x++)
            {
                int index = Coord.CoordinateToIndex(x, y);
                GameObject target = PlaceableDiskInstances[index];
                ReversiCellStatus status = Coord.GetCell(x, y);

                bool result = false;
                if (status == ReversiCellStatus.NONE)
                {
                    result = Coord.IsFlippable(Coord.GetToBeFlipped(index));
                }
                if (!result)
                {
                    if (target != null)
                    {
                        Destroy(target);
                        target = null;

                    }
                    continue;
                }
                if(target != null && target.GetComponent<PlaceableReversiDisk>().Color != Coord.Turn)
                {
                    Destroy(target);
                    target = null;
                }
                GameObject generated = Instantiate(PlaceableDiskPrefab);
                generated.GetComponent<PlaceableReversiDisk>().Setup(this, index, Coord.GetCellVector(x, y), Coord.Turn == ReversiCellStatus.BLACK);
                PlaceableDiskInstances[Coord.CoordinateToIndex(x, y)] = generated;
            }
        }
        DateTime end = DateTime.Now;

        Debug.LogFormat("State synced ({0}ms)", (end - start).TotalMilliseconds);
    }

    public void Flip(int index)
    {

    }
}

public enum ReversiCellStatus : byte
{
    BLACK = 1,
    WHITE = 2,
    NONE = 0,
}

