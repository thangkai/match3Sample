using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

public class Utils
{
    private static readonly NormalItem.eNormalType[] m_normalTypes = (NormalItem.eNormalType[])Enum.GetValues(typeof(NormalItem.eNormalType));

    public static NormalItem.eNormalType GetRandomNormalType()
    {
        return m_normalTypes[URandom.Range(0, m_normalTypes.Length)];
    }

    public static NormalItem.eNormalType GetRandomNormalTypeExcept(NormalItem.eNormalType[] types)
    {
        List<NormalItem.eNormalType> list = new List<NormalItem.eNormalType>();

        for (int i = 0; i < m_normalTypes.Length; i++)
        {
            var type = m_normalTypes[i];
            bool isExcluded = false;
            for (int j = 0; j < types.Length; j++)
            {
                if (type == types[j])
                {
                    isExcluded = true;
                    break;
                }
            }

            if (!isExcluded)
            {
                list.Add(type);
            }
        }

        if (list.Count == 0) return GetRandomNormalType(); // Fallback

        return list[URandom.Range(0, list.Count)];
    }
}
