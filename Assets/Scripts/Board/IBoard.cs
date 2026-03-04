using System;


public interface IBoard
{
    //inheritor === ke thua
    int BoardSizeX { get; }
    int BoardSizeY { get; }
    Cell GetCell(int x, int y);
    void Swap(Cell cell1, Cell cell2, Action callback);
    void Clear();

    
}
