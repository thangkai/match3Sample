using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoardProcessor
{
    private IBoard m_board;
    private IItemFactory m_itemFactory;
    private MatchFinder m_matchFinder;
    private Transform m_root;
    /// <summary>
    /// Dependency Injection.
    /// </summary>
    /// <param name="board">Interface đại diện cho board. (lấy cell + biết kích thước board ) </param>
    /// <param name="itemFactory">Factory pattern tạo item: </param>
    /// <param name="matchFinder"> Class tìm match. </param>
    /// <param name="root">Parent chứa toàn bộ item view trong scene. </param>
    public BoardProcessor(IBoard board, IItemFactory itemFactory, MatchFinder matchFinder, Transform root)
    {
        m_board = board;
        m_itemFactory = itemFactory;
        m_matchFinder = matchFinder;
        m_root = root;
    }

    public void Fill()
    {
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);

                List<NormalItem.eNormalType> types = new List<NormalItem.eNormalType>();
                if (cell.NeighbourBottom != null)
                {
                    NormalItem nitem = cell.NeighbourBottom.Item as NormalItem;
                    if (nitem != null) types.Add(nitem.ItemType);
                }

                if (cell.NeighbourLeft != null)
                {
                    NormalItem nitem = cell.NeighbourLeft.Item as NormalItem;
                    if (nitem != null) types.Add(nitem.ItemType);
                }
            
                var type = Utils.GetRandomNormalTypeExcept(types.ToArray());
                //Không tạo 3 item giống nhau liên tiếp.
                NormalItem item = m_itemFactory.GetNormalItem(type);
                item.SetType(type);
                item.SetViewRoot(m_root);
                
                //Gán vào cell
                cell.Assign(item);
                cell.ApplyItemPosition(false);
            }
        }
    }
    /// <summary>
    /// Trộn lại toàn bộ board.
    /// </summary>

    public void Shuffle()
    {
        List<Item> list = new List<Item>();
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);
                list.Add(cell.Item);
                cell.Free();
            }
        }
        //Fisher-Yates
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                int rnd = UnityEngine.Random.Range(0, list.Count);
                m_board.GetCell(x, y).Assign(list[rnd]);
                m_board.GetCell(x, y).ApplyItemMoveToPosition();

                list.RemoveAt(rnd);
            }
        }
    }

    /// <summary>
    /// Sinh item mới vào ô trống.
    /// </summary>
    public void FillGapsWithNewItems()
    {
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);
                if (!cell.IsEmpty) continue;

                var type = Utils.GetRandomNormalType();
                NormalItem item = m_itemFactory.GetNormalItem(type);
                item.SetType(type);
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(true);
            }
        }
    }
    /// <summary>
    /// Nếu có ô trống bên dưới → item phía trên rơi xuống.
    /// </summary>

    public void ShiftDownItems()
    {
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            int shifts = 0;
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                Cell cell = m_board.GetCell(x, y);
                if (cell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                Cell holder = m_board.GetCell(x, y - shifts);

                Item item = cell.Item;
                cell.Free();

                holder.Assign(item);
                item.View.DOMove(holder.transform.position, 0.3f);
            }
        }
    }
    /// <summary>
    ///Gọi explode từng cell.

    ///Có thể dùng khi:

    /// bomb toàn màn

    ///reset level

    ///   skill đặc biệt
    /// 
    /// </summary>
    public void ExplodeAllItems()
    {
        for (int x = 0; x < m_board.BoardSizeX; x++)
        {
            for (int y = 0; y < m_board.BoardSizeY; y++)
            {
                m_board.GetCell(x, y).ExplodeItem();
            }
        }
    }

    /// <summary>
    /// Biến match thành bonus item.
    /// </summary>
    /// <param name="matches">xác định hướng match</param>
    /// <param name="cellToConvert"></param>
    public void ConvertNormalToBonus(List<Cell> matches, Cell cellToConvert)
    {
        Board.eMatchDirection dir = m_matchFinder.GetMatchDirection(matches);

        BonusItem.eBonusType bonusType = BonusItem.eBonusType.NONE;
        switch (dir)
        {
            case Board.eMatchDirection.ALL: bonusType = BonusItem.eBonusType.ALL; break;
            case Board.eMatchDirection.HORIZONTAL: bonusType = BonusItem.eBonusType.HORIZONTAL; break;
            case Board.eMatchDirection.VERTICAL: bonusType = BonusItem.eBonusType.VERTICAL; break;
        }

        if (bonusType != BonusItem.eBonusType.NONE)
        {
            //tạo bonus item
            BonusItem item = m_itemFactory.GetBonusItem(bonusType);
            item.SetType(bonusType);
            item.SetViewRoot(m_root);

            if (cellToConvert == null)
            {
                int rnd = UnityEngine.Random.Range(0, matches.Count);
                cellToConvert = matches[rnd];
            }

            //Replace item
            cellToConvert.Free();
            cellToConvert.Assign(item);
            cellToConvert.ApplyItemPosition(true);
        }
    }
}
