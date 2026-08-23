using UnityEngine;

public class Item
{
    public int Value;
    public int Row;
    public int Column;
    public GameObject GO;
    public bool WasJustDuplicated;

    public Item(int value, int row, int column, GameObject go = null)
    {
        Value = value;
        Row = row;
        Column = column;
        GO = go;
        WasJustDuplicated = false;
    }
}