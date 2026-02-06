using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;

    private GameManager m_mngr;

    public override void Setup(float value, Action<string> onUpdateText, GameManager mngr)
    {
        base.Setup(value, onUpdateText, mngr);

        m_mngr = mngr;

        m_time = value;

        UpdateText();
    }

    private void Update()
    {
        if (m_conditionCompleted) return;

        if (m_mngr.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        UpdateText();

        if (m_time <= -1f)
        {
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        if (m_time < 0f) return;

        if (m_onUpdateText != null) m_onUpdateText(string.Format("TIME:\n{0:00}", m_time));
    }
}
