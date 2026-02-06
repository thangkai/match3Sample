using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMoves : LevelCondition
{
    private int m_moves;

    private BoardController m_board;

    public override void Setup(float value, Action<string> onUpdateText, BoardController board)
    {
        base.Setup(value, onUpdateText);

        m_moves = (int)value;

        m_board = board;

        m_board.OnMoveEvent += OnMove;

        UpdateText();
    }

    private void OnMove()
    {
        if (m_conditionCompleted) return;

        m_moves--;

        UpdateText();

        if(m_moves <= 0)
        {
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        if(m_onUpdateText != null) m_onUpdateText(string.Format("MOVES:\n{0}", m_moves));
    }

    protected override void OnDestroy()
    {
        if (m_board != null) m_board.OnMoveEvent -= OnMove;

        base.OnDestroy();
    }
}
