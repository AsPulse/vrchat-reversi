
using System;
using System.Linq;
using System.Runtime.InteropServices;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ReversiCoordinateManager : UdonSharpBehaviour {
    [NonSerialized]
    public ReversiCellStatus Turn = ReversiCellStatus.BLACK; 
    [NonSerialized]
    public ReversiCellStatus[] Map;
    [NonSerialized]
    public readonly int WIDTH = 8;
    [NonSerialized]
    public readonly int HEIGHT = 8;

    [SerializeField]
    private Vector3 DiskSize;

    [SerializeField]
    private RadixConverter RadixConverter;

    public void SetUp(GameObject diskPrefab)
    {
        Map = new ReversiCellStatus[WIDTH * HEIGHT];
        for ( int i = 0; i < Map.Length; i++ ) {
            Map[i] = ReversiCellStatus.NONE;
        }

        SetCell(3, 3, ReversiCellStatus.BLACK);
        SetCell(4, 3, ReversiCellStatus.WHITE);
        SetCell(3, 4, ReversiCellStatus.WHITE);
        SetCell(4, 4, ReversiCellStatus.BLACK);

    }

    public Vector3 GetCellVector(int x, int y)
    {
        return new Vector3(
            DiskSize.x * (x - (WIDTH / 2) + 0.5f),
            0f,
            DiskSize.z * (y - (HEIGHT / 2) + 0.5f)
        );
    }
    public int CoordinateToIndex(int x, int y)
    {
        return y * WIDTH + x;
    }

    public int MovedIndex(int index, int deltaX, int deltaY)
    {
        int x = index % HEIGHT;
        int y = (index - x) / WIDTH;
        x += deltaX;
        y += deltaY;
        if (x < 0) return -1;
        if (x >= WIDTH) return -1;
        if (y < 0) return -1;
        if ( y >= HEIGHT) return -1;
        return CoordinateToIndex(x, y);
    }

    public ReversiCellStatus GetCell(int x, int y)
    {
        return Map[CoordinateToIndex(x, y)];
    }

    public void SetCell(int x, int y, ReversiCellStatus cell)
    {
        Map[CoordinateToIndex(x, y)] = cell;
    }

    public string Encode()
    {
        Debug.Log("Encoding...");

        DateTime start = DateTime.Now;
        string s = ((byte)Turn).ToString();
        foreach(ReversiCellStatus cell in Map)
        {
            s += ((byte) cell).ToString();
        }
        string result = RadixConverter.Convert(s, 3, 62);
        DateTime end = DateTime.Now;
        Debug.LogFormat("Enconding tooks {0}ms", (end - start).TotalMilliseconds);

        return result;
    }

    public void Decode(string str)
    {
        Debug.LogFormat("Decoding... payload:{0}", str);

        DateTime start = DateTime.Now;
        string payload = RadixConverter.Convert(str, 62, 3);
        Turn = (ReversiCellStatus) Byte.Parse(payload[0].ToString());
        for(int i = 0; i < Map.Length; i++)
        {
            Map[i] = (ReversiCellStatus)Byte.Parse(payload[i + 1].ToString());
        }
        DateTime end = DateTime.Now;

        Debug.LogFormat("Decoding tooks {0}ms", (end - start).TotalMilliseconds);
    }

    public int[][] GetToBeFlipped(int index)
    {
        return new int[][]
        {
            GetToBeFlipped(new int[0], index, 1, 1),
            GetToBeFlipped(new int[0], index, 1, 0),
            GetToBeFlipped(new int[0], index, 1, -1),
            GetToBeFlipped(new int[0], index, 0, 1),
            GetToBeFlipped(new int[0], index, 0, -1),
            GetToBeFlipped(new int[0], index, -1, 1),
            GetToBeFlipped(new int[0], index, -1, 0),
            GetToBeFlipped(new int[0], index, -1, -1),
        };
    }

    public bool IsFlippable(int[][] toBeFlipped)
    {
        for(int i = 0; i < toBeFlipped.Length; i++)
        {
            if (toBeFlipped[i].Length > 0) return true;
        }
        return false;
    }

    private int[] GetToBeFlipped(int[] index, int start, int deltaX, int deltaY)
    {
        int pos = MovedIndex(index.Length == 0 ? start : index[index.Length - 1], deltaX, deltaY);
        if (pos == -1) return new int[0];

        ReversiCellStatus status = Map[pos];
        if (status == ReversiCellStatus.NONE) return new int[0];
        if (status == Turn) return index;

        int[] result = new int[index.Length + 1];
        index.CopyTo(result, 0);
        result[result.Length - 1] = pos;
        return GetToBeFlipped(result, start, deltaX, deltaY);
    }

}