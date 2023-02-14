using System;
using TheSTAR.Controllers;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TheSTAR.GUI.ChooseItem
{
    public class ChooseItemPanel : MonoBehaviour
    {
        [SerializeField] private Sprite standardBoxSprite;
        [SerializeField] private Sprite selectedBoxSprite;
        [SerializeField] private Sprite clearIcon;
        [SerializeField] private ChooseItemCell[] cells = new ChooseItemCell[10];
        public int Length => cells.Length;

        [Inject] private BlockController _blockController;
        private Action<BlockType> _onPanelItemClick;

        private int _currentCellIndex = -1;

        public void Init(Action<BlockType> onPanelItemClick)
        {
            _onPanelItemClick = onPanelItemClick;
        
            for (var i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                if (cell  == null) continue;

                cell.Init(i, OnCellClick);
            }
        }

        private void OnCellClick(int cellIndex)
        {
            if (_currentCellIndex != -1) cells[_currentCellIndex].SetCellSprite(standardBoxSprite);

            cells[cellIndex].SetCellSprite(selectedBoxSprite);
            _currentCellIndex = cellIndex;

            _onPanelItemClick?.Invoke(cells[cellIndex].BlockType);
        }

        public void SetItemToSlot(int slotIndex, BlockType blockType)
        {
            var blockSprite = blockType == BlockType.None ? clearIcon : _blockController.BlockVisual.GetSprite(blockType, BlockVisualType.Full);

            cells[slotIndex].SetItem(blockSprite, blockType);
        }

        public void KeySelectItem(int index)
        {
            cells[index].DoAction();
        }
    }
}