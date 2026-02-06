using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    public event Action ConditionCompleteEvent = delegate { };

    protected Action<string> m_onUpdateText;

    protected bool m_conditionCompleted = false;

    public virtual void Setup(float value, Action<string> onUpdateText)
    {
        m_onUpdateText = onUpdateText;
    }

    public virtual void Setup(float value, Action<string> onUpdateText, GameManager mngr)
    {
        m_onUpdateText = onUpdateText;
    }

    public virtual void Setup(float value, Action<string> onUpdateText, BoardController board)
    {
        m_onUpdateText = onUpdateText;
    }

    protected virtual void UpdateText() { }

    protected void OnConditionComplete()
    {
        m_conditionCompleted = true;

        ConditionCompleteEvent();
    }

    protected virtual void OnDestroy()
    {

    }
}
