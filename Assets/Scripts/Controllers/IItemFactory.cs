public interface IItemFactory
{
    NormalItem GetNormalItem(NormalItem.eNormalType type);
    BonusItem GetBonusItem(BonusItem.eBonusType type);
    void ReturnItem(Item item);
}
