using System;

public interface IGameManager
{
    event Action<GameManager.eStateGame> StateChangedAction;
    GameManager.eStateGame State { get; }
    void SetState(GameManager.eStateGame state);
    void LoadLevel(GameManager.eLevelMode mode);
    void ClearLevel();
}
