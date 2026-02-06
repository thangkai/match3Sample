using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class Item
{
    public Cell Cell { get; private set; }

    public Transform View { get; private set; }

    public string PrefabPath { get; set; }

    public virtual void SetView(Transform view)
    {
        View = view;
    }

    public virtual void SetCell(Cell cell)
    {
        Cell = cell;
    }

    internal void AnimationMoveToPosition()
    {
        if (View == null) return;

        View.DOMove(Cell.transform.position, 0.2f);
    }

    public void SetViewPosition(Vector3 pos)
    {
        if (View)
        {
            View.position = pos;
        }
    }

    public void SetViewRoot(Transform root)
    {
        if (View)
        {
            View.SetParent(root);
        }
    }

    public void SetSortingLayerHigher()
    {
        if (View == null) return;

        SpriteRenderer sp = View.GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.sortingOrder = 1;
        }
    }


    public void SetSortingLayerLower()
    {
        if (View == null) return;

        SpriteRenderer sp = View.GetComponent<SpriteRenderer>();
        if (sp)
        {
            sp.sortingOrder = 0;
        }

    }

    internal void ShowAppearAnimation()
    {
        if (View == null) return;

        Vector3 scale = Vector3.one; // Assume default scale is one
        View.localScale = Vector3.one * 0.1f;
        View.DOScale(scale, 0.1f);
    }

    internal virtual bool IsSameType(Item other)
    {
        return false;
    }

    internal virtual void ExplodeView()
    {
        if (View)
        {
            View.DOScale(0.1f, 0.1f).OnComplete(
                () =>
                {
                    ItemFactory.Instance.ReturnItem(this);
                }
                );
        }
    }

    internal void AnimateForHint()
    {
        if (View)
        {
            View.DOPunchScale(Vector3.one * 0.1f, 0.1f).SetLoops(-1);
        }
    }

    internal void StopAnimateForHint()
    {
        if (View)
        {
            View.DOKill();
            View.localScale = Vector3.one;
        }
    }

    internal void Clear()
    {
        Cell = null;
        ItemFactory.Instance.ReturnItem(this);
    }

    public virtual void OnGetFromPool()
    {
        if (View)
        {
            View.gameObject.SetActive(true);
            View.localScale = Vector3.one;
            View.DOKill();
            
            SpriteRenderer sp = View.GetComponent<SpriteRenderer>();
            if (sp) sp.sortingOrder = 0;
        }
    }

    public virtual void OnReturnToPool()
    {
        if (View)
        {
            View.DOKill();
            View.gameObject.SetActive(false);
        }
        Cell = null;
    }
}
