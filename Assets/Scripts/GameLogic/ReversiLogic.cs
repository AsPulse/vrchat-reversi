
using Microsoft.Win32;
using System;
using System.Linq;
using System.Reflection;
using tutinoco;
using UdonSharp;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
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

    private int[][] FlipTasks;

    private float timer = -1;

    void Start()
    {
        SimpleNetworkInit();
        Coord = GetComponent<ReversiCoordinateManager>();
        Coord.SetUp(DiskPrefab);
        FlipTasks = new int[0][];

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

        if(name == "Flip")
        {
            ProcessFlip(GetInt(value));
        }
    }


    public void SendBoardStateSync()
    {
        ClearJoinSync();
        SendEvent("StateSync", Coord.Encode());
    }

    private void AddDisk(int x, int y, ReversiCellStatus status)
    {
        if (status == ReversiCellStatus.NONE) return;
        GameObject generated = Instantiate(DiskPrefab);
        generated.GetComponent<ReversiDisk>().Setup(gameObject, Coord.GetCellVector(x, y), status == ReversiCellStatus.BLACK);
        DiskInstances[Coord.CoordinateToIndex(x, y)] = generated;
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
                    AddDisk(x, y, status);
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
        SendEvent("Flip", index, JoinSync.Logging);
    }

    private void ProcessFlip(int index)
    {
        int[][] flippable = Coord.GetToBeFlipped(index);
        if (!Coord.IsFlippable(flippable)) return;

        for (int i = 0; i < PlaceableDiskInstances.Length; i++)
        {
            Destroy(PlaceableDiskInstances[i]);
            PlaceableDiskInstances[i] = null;
        }

        int max = 0;
        for (int i = 0; i < flippable.Length; i++)
        {
            max = Math.Max(max, flippable[i].Length);
        }

        int[][] newFlipTasks = new int[FlipTasks.Length + max][];
        FlipTasks.CopyTo(newFlipTasks, 0);

        for (int i = FlipTasks.Length; i < newFlipTasks.Length; i++)
        {
            int length = 0;
            for (int k = 0; k < flippable.Length; k++)
            {
                if (flippable[k].Length > (i - FlipTasks.Length)) length++;
            }

            newFlipTasks[i] = new int[length];
            int elements = 0;
            for (int k = 0; k < flippable.Length; k++)
            {
                if (flippable[k].Length <= (i - FlipTasks.Length)) continue;
                newFlipTasks[i][elements++] = flippable[k][i - FlipTasks.Length];
            }
        }

        FlipTasks = newFlipTasks;
        Coord.Map[index] = Coord.Turn;
        int[] xy = Coord.IndexToCoordinate(index);
        AddDisk(xy[0], xy[1], Coord.Turn);
        Coord.Turn = Coord.Turn == ReversiCellStatus.BLACK ? ReversiCellStatus.WHITE : ReversiCellStatus.BLACK;

        if (timer >= 0) return;
        timer = 1;
    }

    public void FlipChain()
    {
        DateTime start = DateTime.Now;
        int[] thisFlips = FlipTasks[0];
        int[][] nextFlips = new int[FlipTasks.Length - 1][];
        for(int i = 0; i < nextFlips.Length; i++)
        {
            nextFlips[i] = FlipTasks[i + 1];
        }
        FlipTasks = nextFlips;


        for(int i = 0; i < thisFlips.Length; i++)
        {
            ReversiCellStatus flipped = Coord.Map[thisFlips[i]] == ReversiCellStatus.BLACK ? ReversiCellStatus.WHITE : ReversiCellStatus.BLACK;
            Coord.Map[thisFlips[i]] = flipped;
            GameObject gameObject = DiskInstances[thisFlips[i]];
            if (gameObject == null) continue;
            gameObject.GetComponent<ReversiDisk>().Color = flipped == ReversiCellStatus.BLACK;
        }

        if(nextFlips.Length > 0)
        {
            timer = 0;
        } else
        {
            if (Networking.LocalPlayer.isMaster) SendCustomEventDelayedSeconds(nameof(SendBoardStateSync), 1.2f);
        }
        DateTime end = DateTime.Now;

        Debug.LogFormat("[{2}ms] FlipChain, Remain-FlipTasks = {1}", FlipTasks.Length, (end-start).TotalMilliseconds);


    }

    private void Update()
    {
        if(timer >= 0)
        {
            timer += Time.deltaTime;
        }

        if(timer > 0.15f)
        {
            timer = -1;
            FlipChain();
        }


    }
}

public enum ReversiCellStatus : byte
{
    BLACK = 1,
    WHITE = 2,
    NONE = 0,
}

