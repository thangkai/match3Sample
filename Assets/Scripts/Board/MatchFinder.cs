using System.Collections.Generic;
using UnityEngine;

public class MatchFinder
{
    private IBoard m_board;
    private int m_matchMin;

    public MatchFinder(IBoard board, int matchMin)
    {
        m_board = board;
        m_matchMin = matchMin;
    }

    public List<Cell> GetHorizontalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }

    public List<Cell> GetVerticalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();
        list.Add(cell);

        Cell newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourUp;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        newcell = cell;
        while (true)
        {
            Cell neib = newcell.NeighbourBottom;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                newcell = neib;
            }
            else break;
        }

        return list;
    }

    public Board.eMatchDirection GetMatchDirection(List<Cell> matches)
    {
        if (matches == null || matches.Count < m_matchMin) return Board.eMatchDirection.NONE;

        bool allVertical = true;
        bool allHorizontal = true;
        int firstX = matches[0].BoardX;
        int firstY = matches[0].BoardY;

        for (int i = 1; i < matches.Count; i++)
        {
            if (matches[i].BoardX != firstX)
            {
                allVertical = false;
            }
            if (matches[i].BoardY != firstY)
            {
                allHorizontal = false;
            }
        }

        if (allVertical) return Board.eMatchDirection.VERTICAL;
        if (allHorizontal) return Board.eMatchDirection.HORIZONTAL;
        if (matches.Count > 5) return Board.eMatchDirection.ALL;

        return Board.eMatchDirection.NONE;
    }

    public List<Cell> FindFirstMatch()
    {
        List<Cell> list = new List<Cell>();

        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);

                var listhor = GetHorizontalMatches(cell);
                if (listhor.Count >= m_matchMin)
                {
                    list = listhor;
                    break;
                }

                var listvert = GetVerticalMatches(cell);
                if (listvert.Count >= m_matchMin)
                {
                    list = listvert;
                    break;
                }
            }
        }

        return list;
    }

    public List<Cell> GetPotentialMatches()
    {
        List<Cell> result = new List<Cell>();
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);

                if (cell.NeighbourRight != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourRight, cell.NeighbourRight.NeighbourRight);
                    if (result.Count > 0) break;
                }

                if (cell.NeighbourUp != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourUp, cell.NeighbourUp.NeighbourUp);
                    if (result.Count > 0) break;
                }

                if (cell.NeighbourBottom != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourBottom, cell.NeighbourBottom.NeighbourBottom);
                    if (result.Count > 0) break;
                }

                if (cell.NeighbourLeft != null)
                {
                    result = GetPotentialMatch(cell, cell.NeighbourLeft, cell.NeighbourLeft.NeighbourLeft);
                    if (result.Count > 0) break;
                }

                Cell neib = cell.NeighbourRight;
                if (neib != null && neib.NeighbourRight != null && neib.NeighbourRight.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellVertical(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourRight);
                        result.Add(second);
                        break;
                    }
                }

                neib = cell.NeighbourUp;
                if (neib != null && neib.NeighbourUp != null && neib.NeighbourUp.IsSameType(cell))
                {
                    Cell second = LookForTheSecondCellHorizontal(neib, cell);
                    if (second != null)
                    {
                        result.Add(cell);
                        result.Add(neib.NeighbourUp);
                        result.Add(second);
                        break;
                    }
                }
            }
            if (result.Count > 0) break;
        }

        return result;
    }

    private List<Cell> GetPotentialMatch(Cell cell, Cell neighbour, Cell target)
    {
        List<Cell> result = new List<Cell>();

        if (neighbour != null && neighbour.IsSameType(cell))
        {
            Cell third = LookForTheThirdCell(target, neighbour);
            if (third != null)
            {
                result.Add(cell);
                result.Add(neighbour);
                result.Add(third);
            }
        }

        return result;
    }

    private Cell LookForTheSecondCellHorizontal(Cell target, Cell main)
    {
        if (target == null || target.IsSameType(main)) return null;

        Cell second = target.NeighbourRight;
        if (second != null && second.IsSameType(main)) return second;

        second = target.NeighbourLeft;
        if (second != null && second.IsSameType(main)) return second;

        return null;
    }

    private Cell LookForTheSecondCellVertical(Cell target, Cell main)
    {
        if (target == null || target.IsSameType(main)) return null;

        Cell second = target.NeighbourUp;
        if (second != null && second.IsSameType(main)) return second;

        second = target.NeighbourBottom;
        if (second != null && second.IsSameType(main)) return second;

        return null;
    }

    private Cell LookForTheThirdCell(Cell target, Cell main)
    {
        if (target == null || target.IsSameType(main)) return null;

        Cell third = CheckThirdCell(target.NeighbourUp, main);
        if (third != null) return third;

        third = CheckThirdCell(target.NeighbourRight, main);
        if (third != null) return third;

        third = CheckThirdCell(target.NeighbourBottom, main);
        if (third != null) return third;

        third = CheckThirdCell(target.NeighbourLeft, main);
        if (third != null) return third;

        return null;
    }

    private Cell CheckThirdCell(Cell target, Cell main)
    {
        if (target != null && target != main && target.IsSameType(main)) return target;
        return null;
    }
}
