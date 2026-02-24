using System;
using System.Collections.Generic;
using UnityEngine;

public interface IBoard
{
    int BoardSizeX { get; }
    int BoardSizeY { get; }
    Cell GetCell(int x, int y);
    void Swap(Cell cell1, Cell cell2, Action callback);
    void Clear();
}
