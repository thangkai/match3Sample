namespace AudioLib
{
    /// <summary>
    /// Định nghĩa tất cả audio ID trong game.
    /// Thêm entry mới vào đây khi có audio mới.
    /// </summary>
    public enum BgmID
    {
        None = 0,
        MainMenu,
        Gameplay,
        Victory,
        GameOver,
    }

    public enum SfxID
    {
        None = 0,

        // UI
        ButtonClick,
        ButtonHover,
        PopupOpen,
        PopupClose,

        // Gameplay - Casual/Puzzle
        PiecePlace,
        PieceMatch,
        ComboSmall,
        ComboLarge,
        LevelComplete,
        LevelFail,
        StarEarn,
        CoinCollect,

        // Misc
        CountdownTick,
        CountdownGo,
    }
}
