
using System;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ReversiCoordinateManager : UdonSharpBehaviour {

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

        string a = "2" +
            "00000000" +
            "00000000" +
            "00000000" +
            "00012000" +
            "00021000" +
            "00000000" +
            "00000000" +
            "00000000";
        string b = RadixConverter.Convert(a, 3, 62);
        string c = RadixConverter.Convert(b, 62, 3);
        Debug.Log(a == c);

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

    public ReversiCellStatus GetCell(int x, int y)
    {
        return Map[CoordinateToIndex(x, y)];
    }

    public void SetCell(int x, int y, ReversiCellStatus cell)
    {
        Map[CoordinateToIndex(x, y)] = cell;
    }


}