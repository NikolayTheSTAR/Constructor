using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TheSTAR.Utility;
using TheSTAR.Utility.Pointer;

public class ChooseItemCell : MonoBehaviour
{
    [SerializeField] private Image _cellImg;
    [SerializeField] private Image _iconImg;
    [SerializeField] private Pointer _pointer;
    private int _index;
    private Action<int> _onClickAction;
    private BlockType _blockType;
    public BlockType BlockType => _blockType;

    public void Init(int index, Action<int> onClickAction)
    {
        _index = index;
        _onClickAction = onClickAction;
        _pointer.InitPointer((e) => DoAction());
    }

    public void SetCellSprite(Sprite sprite)
    {
        _cellImg.sprite = sprite;
    }

    public void SetItem(Sprite sprite, BlockType blockType)
    {
        _blockType = blockType;
        _iconImg.sprite = sprite;
    }

    public void DoAction()
    {
        _onClickAction(_index);
    }
}