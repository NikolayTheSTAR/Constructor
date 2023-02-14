using TheSTAR.Controllers;
using UnityEngine;
using Zenject;
using TheSTAR.Utility;
using UnityEngine.EventSystems;
using TheSTAR.CreativeSpace;
using TheSTAR.GUI.ChooseItem;
using TheSTAR.Utility.Pointer;

namespace TheSTAR.Constructor
{
    public class Constructor : MonoBehaviour
    {
        #region Fields

        [SerializeField] private WorldSpace worldSpace;
        [SerializeField] private GridSpace gridSpace;
        [SerializeField] private Pointer mousePointer;

        [Space]
        [SerializeField] private ChooseItemPanel chooseItemPanel;
 
        [Header("Fon")]
        [SerializeField] private Color grayFonColor;
        [SerializeField] private Color brownFonColor;
        
        [Inject] private InputController _inputController;
        [Inject] private ConstructorCamera _constructorCamera;
        [Inject] private Console.Console _console;
        [Inject] private BlockController _blockController;
        
        public int GetWidth => gridSpace.UsedWidth;
        public int GetHeight => gridSpace.UsedHeight;
        
        public int GetPoolWidth => gridSpace.GetPoolWidth;
        public int GetPoolHeight => gridSpace.GetPoolHeight;

        private bool _isCtorPointerDown = false;

        [SerializeField] private BlockType paintType;

        #endregion Fields

        private void Start()
        {
            _blockController.Init();
            ResetGridSize();

            paintType = (BlockType)2;
            
            var controlKits = new InputController.ControlKit[]
            {
                new (KeyCode.Alpha0, () => KeyChooseItemSlot(9)),
                new (KeyCode.Alpha1, () => KeyChooseItemSlot(0)),
                new (KeyCode.Alpha2, () => KeyChooseItemSlot(1)),
                new (KeyCode.Alpha3, () => KeyChooseItemSlot(2)),
                new (KeyCode.Alpha4, () => KeyChooseItemSlot(3)),
                new (KeyCode.Alpha5, () => KeyChooseItemSlot(4)),
                new (KeyCode.Alpha6, () => KeyChooseItemSlot(5)),
                new (KeyCode.Alpha7, () => KeyChooseItemSlot(6)),
                new (KeyCode.Alpha8, () => KeyChooseItemSlot(7)),
                new (KeyCode.Alpha9, () => KeyChooseItemSlot(8)),

                new (KeyCode.C, CameraCentralization),
                new (KeyCode.Slash, () =>
                {
                    if (_console.IsShow) _console.Hide();
                    else _console.Show();
                })
            };
            _inputController.Init(_constructorCamera, controlKits);
            _console.Init();
            mousePointer.InitPointer(
                OnCtorPointerDown,
                OnCtorPointerDrag,
                OnCtorPointerUp
            );
            
            InitChooseItemPanel();
            KeyChooseItemSlot(0);
        }

        private void InitChooseItemPanel()
        {
            chooseItemPanel.Init(OnChoosePanelItemClick);

            // blocks

            var blockTypes = EnumUtility.GetValues<BlockType>();
            
            for (var i = 0; i < chooseItemPanel.Length - 1; i++) chooseItemPanel.SetItemToSlot(i, blockTypes[i + 2]);

            // clear
            chooseItemPanel.SetItemToSlot(chooseItemPanel.Length - 1, BlockType.None);
        }

        #region Pointer

        private void OnCtorPointerDown(PointerEventData e)
        {
            _isCtorPointerDown = true;
            
            GetMouseWorldCoordinates(out var x, out var y);

            // check grid bounds
            if (!(x >= 0 && x < GetWidth && y >= 0 && y < GetHeight)) return;

            // interaction
            GridInteraction(paintType, x, y);
        }

        private void OnCtorPointerDrag(PointerEventData e)
        {
            if (!_isCtorPointerDown) return;

            GetMouseWorldCoordinates(out var x, out var y);

            // check grid bounds
            if (!(x >= 0 && x < GetWidth && y >= 0 && y < GetHeight)) return;

            // interaction
            GridInteraction(paintType, x, y);
        }

        private void OnCtorPointerUp(PointerEventData e)
        {
            _isCtorPointerDown = false;
            worldSpace.ResetInteractionGridLayer();
        }

        #endregion

        private void GridInteraction(BlockType paintType, int x, int y)
        {
            if (paintType == BlockType.None) worldSpace.ClearBlock(x, y);
            else worldSpace.CreateBlock(_blockController.GetBlockPrefab(paintType), x, y);
        }
        
        #region Paint

        private void OnChoosePanelItemClick(BlockType blockType)
        {
            SetPaintType(blockType);
        }

        private void KeyChooseItemSlot(int slotIndex)
        {
            chooseItemPanel.KeySelectItem(slotIndex);
        }

        public void SetPaintType(int index)
        {
            if (!EnumUtility.IsDefined<BlockType>(index)) return;

            SetPaintType((BlockType)index);
        }
        
        public void SetPaintType(BlockType blockType)
        {
            paintType = blockType;
        }

        #endregion Paint

        private static void GetMouseWorldCoordinates(out int x, out int y)
        {
            if (Camera.main == null)
            {
                x = y = -1;
                return;
            }
            
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float
            tempX = mousePosition.x,
            tempY = -mousePosition.y;
            x = (int)tempX - (tempX < 0 ? 1 : 0);
            y = (int)tempY - (tempY < 0 ? 1 : 0);
        }

        private void ResetGridSize()
        {
            SetGridSize(CreativeSpace.CreativeSpace.DefaultSize);
        }

        public void AddGridSize(int value)
        {
            AddGridSize(value, value);
        }
        
        public void AddGridSize(int width, int height)
        {
            SetGridSize(gridSpace.GetWidth + width, gridSpace.GetHeight + height);
        }
        
        public void SetGridSize(int size)
        {
            SetGridSize(size, size);
        }
        
        public void SetGridSize(int width, int height)
        {
            gridSpace.SetSize(width, height);
            worldSpace.SetSize(width, height);

            CameraCentralization();
        }

        public void ClearPool(out int deletedCellsCount)
        {
            gridSpace.ClearPool(out deletedCellsCount);
            worldSpace.ClearPool(out _);
        }

        public void CameraCentralization()
        {
            _constructorCamera.transform.position = new Vector3(GetWidth / 2f, -GetHeight / 2f, _constructorCamera.transform.position.z);
        }
    
        public void SetCameraPosition(WorldDirections dir)
        {
            float x = 0, y = 0, z = _constructorCamera.transform.position.z;

            switch (dir)
            {
                case WorldDirections.North:
                x = GetWidth / 2f;
                y = 0;
                break;

                case WorldDirections.South:
                x = GetWidth / 2f;
                y = -GetHeight;
                break;

                case WorldDirections.West:
                x = 0;
                y = -GetHeight / 2f;
                break;

                case WorldDirections.East:
                x = GetWidth;
                y = -GetHeight / 2f;
                break;
            }

            _constructorCamera.transform.position = new Vector3(x, y, z);
        }

        public void SetFonType(FonType t)
        {
            Color color;

            switch (t)
            {
                case FonType.Gray:
                color = grayFonColor;
                break;

                case FonType.Brown:
                color = brownFonColor;
                break;

                default:
                color = grayFonColor;
                break;
            }

            _constructorCamera.SetColor(color);
        }
    }
}

public enum WorldDirections
{
    North,
    South,
    West,
    East
}

public enum BlockType
{
    None,
    Ctor, // for constructor only
    Earth,
    EarthDark,
    Grass,
    GrassDark,
    Sand,
    Water,
    WaterWave,
    WoodFloor,
    StoneFloor,
    WoodBridge
}

public enum FonType
{
    Gray,
    Brown
}