using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;
using TMPro;
using Zenject;

namespace TheSTAR.Console
{
    public sealed partial class Console : MonoBehaviour
    {
        #region Fields

        [SerializeField] private TextMeshProUGUI _currentCommandText;
        [SerializeField] private TextMeshProUGUI _hintCommandText;
        [SerializeField] private ConsoleMessageLine[] _messageLines = new ConsoleMessageLine[10];
        
        [Inject] private InputController _input;
        [Inject] private Constructor.Constructor _constructor;
        [Inject] private Constructor.ConstructorCamera _camera;

        private bool _isShow = false;
        public bool IsShow => _isShow;
        private CommandVisualData _normalCommandVisual = new CommandVisualData(CommandColorType.Normal, Color.white);
        private CommandVisualData _errorCommandVisual = new CommandVisualData(CommandColorType.Error, Color.red);
        private CommandVisualData _usableCommandVisual = new CommandVisualData(CommandColorType.Usable, Color.yellow);
        private CommandBranch _commandsTree;
        private int _nextBranchCandidateIndex = -1; // index of next command branch hint
        private int _nextBranchEnumVariantIndex = -1; // index of enum value (enum branch only)
        private CommandBranch _lastSafeBranch;

        private List<int> _commandRoute = new List<int>(); // contains CommandBranch indexes for current command

        private const char HINT_AVAILABLE_VARIANTS_SYMBOL = 'q';

        #endregion Fields

        public void Init()
        {
            LoadCommands();
        }

        #region Show/Hide

        public void Show()
        {
            gameObject.SetActive(true);
            _isShow = true;
            _input.OnAnyKeyDown += EnterKey;
            _input.SetNeedCheckAnyKeys(true);
            _lastSafeBranch = _commandsTree;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _isShow = false;

            _input.OnAnyKeyDown -= EnterKey;
            _input.SetNeedCheckAnyKeys(false);
            
            ClearCommand();
        }

        #endregion Show/Hide

        #region Command

        private void EnterKey(KeyCode keyCode)
        {
            char symbol = '_';
            EnterType enterType = EnterType.Symbol;

            switch (keyCode)
            {
                case KeyCode.Slash:
                case KeyCode.Escape:
                enterType = EnterType.Hide;
                break;

                case KeyCode.Return:
                enterType = EnterType.Enter;
                break;

                case KeyCode.Space:
                case KeyCode.Tab:
                enterType = EnterType.Space;
                break;

                case KeyCode.Backspace:
                enterType = EnterType.Delete;
                break;

                case KeyCode.Minus:
                symbol = '-';
                break;

                case KeyCode.Plus:
                symbol = '+';
                break;

                case KeyCode.Alpha0:
                symbol = '0';
                break;

                case KeyCode.Alpha1:
                symbol = '1';
                break;

                case KeyCode.Alpha2:
                symbol = '2';
                break;

                case KeyCode.Alpha3:
                symbol = '3';
                break;

                case KeyCode.Alpha4:
                symbol = '4';
                break;

                case KeyCode.Alpha5:
                symbol = '5';
                break;

                case KeyCode.Alpha6:
                symbol = '6';
                break;

                case KeyCode.Alpha7:
                symbol = '7';
                break;

                case KeyCode.Alpha8:
                symbol = '8';
                break;

                case KeyCode.Alpha9:
                symbol = '9';
                break;

                case KeyCode.Question:
                symbol = HINT_AVAILABLE_VARIANTS_SYMBOL;
                break;

                default:
                symbol = (keyCode.ToString().ToLower().ToCharArray()[0]);
                break;      
            }

            switch (enterType)
            {
                case EnterType.Symbol:
                    if (symbol == ' ' && _currentCommandText.text[_currentCommandText.text.Length - 1] == ' ') break;
                    else _currentCommandText.text += symbol;

                    UpdateCommandVisual();
                    break;

                case EnterType.Delete:
                    if (_currentCommandText.text.Length <= 1) Hide();
                    else
                    {
                        bool isSpace = _currentCommandText.text[_currentCommandText.text.Length - 1] == ' ';
                        if (isSpace)
                        {
                            _commandRoute.RemoveAt(_commandRoute.Count - 1);

                            CommandBranch tempBranch = _commandsTree;
                            for (int i = 0; i < _commandRoute.Count; i++) tempBranch = tempBranch.NextElements[_commandRoute[i]];
                            _lastSafeBranch = tempBranch;
                        }
                        
                        _currentCommandText.text = _currentCommandText.text.Substring(0, _currentCommandText.text.Length - 1);
                    }

                    UpdateCommandVisual();
                    break;

                case EnterType.Space:
                    if (_nextBranchCandidateIndex == -1) break;

                    if (_lastSafeBranch.NextElements[_nextBranchCandidateIndex] is ValueBranch)
                    {
                        // todo: logic with value branch
                        break;
                    }
                    else
                    {
                        _commandRoute.Add(_nextBranchCandidateIndex);
                        _currentCommandText.text = GenerateSafeCommandPart(out _lastSafeBranch);
                        UpdateCommandVisual();
                        break;  
                    }

                case EnterType.Enter:
                    DoCommand();
                    ClearCommand();
                    break;
                
                case EnterType.Hide:
                    Hide();
                    break;
            }
        }

        private void UpdateCommandVisual()
        {
            _nextBranchCandidateIndex = -1; 
            _nextBranchEnumVariantIndex = -1; // (enum branch only)
            string _nextBranchHintText = "";
            ShowBranchHintType showHintType = ShowBranchHintType.Normal;

            // safe / unsafe command parts
            
            GetSafeAndUnsafePartsFromCurrentCommand(out var safePart, out var unsafePart, out var lastSafeCommandBranch);

            // find next branch hint

            CommandBranch candidate; // next branch hint candidate
            //bool isCandidateValueBranch = false; // is next branch hint candidate a value branch (e.g. int, enum)
            
            if (!ArrayUtility.IsNullOfEmpty(lastSafeCommandBranch.NextElements))
            {
                for (int i = 0; i < lastSafeCommandBranch.NextElements.Length; i++)
                {
                    candidate = lastSafeCommandBranch.NextElements[i];

                    if (candidate is ValueBranch)
                    {
                        if (candidate is IntBranch)
                        {
                            if (unsafePart == "" || MathUtility.IsIntValue(unsafePart, out _))
                            {
                                _nextBranchCandidateIndex = i;
                                _nextBranchHintText = candidate.KeyWord;
                                showHintType = ShowBranchHintType.Integer;
                                break;
                            }
                        }
                        else
                        {
                            EnumBranch enumCandidate = candidate as EnumBranch;

                            if (enumCandidate != null)
                            {
                                // get enum values

                                string[] stringValues = enumCandidate.GetVariants;

                                // check enum values

                                bool isFindValueCandidate = false;
                                for (int v = 0; v < stringValues.Length; v++)
                                {
                                    if (unsafePart == "" || stringValues[v].StartsWith(unsafePart))
                                    {
                                        _nextBranchEnumVariantIndex = v;
                                        _nextBranchCandidateIndex = i;
                                        isFindValueCandidate = true;
                                        _nextBranchHintText = stringValues[v];
                                        break;
                                    }
                                }

                                if (isFindValueCandidate) break;
                            }
                        }
                    }
                    else if (candidate.KeyWord.StartsWith(unsafePart))
                    {
                        _nextBranchCandidateIndex = i;
                        _nextBranchHintText = candidate.KeyWord;
                        break;
                    }
                }
            }
            
            // update visual

            bool isSafePartJustCompleted = (unsafePart == "");

            if (isSafePartJustCompleted)
            {
                if (lastSafeCommandBranch.CanBeFinal)
                {
                    _hintCommandText.gameObject.SetActive(false);
                    _currentCommandText.color = _usableCommandVisual.Color;
                }
                else
                {
                    if (_nextBranchCandidateIndex == -1)
                    {
                        _hintCommandText.gameObject.SetActive(false);
                        _currentCommandText.color = _normalCommandVisual.Color;
                    }
                    else
                    {
                        if (showHintType == ShowBranchHintType.Integer) _nextBranchHintText = $"[{_nextBranchHintText}]";
                        
                        _hintCommandText.text = safePart + _nextBranchHintText;

                        _hintCommandText.gameObject.SetActive(true);
                        _currentCommandText.color = _normalCommandVisual.Color;
                    }
                }
            }
            else
            {
                if (unsafePart == HINT_AVAILABLE_VARIANTS_SYMBOL.ToString())
                {
                    _hintCommandText.gameObject.SetActive(false);
                    _currentCommandText.color = _usableCommandVisual.Color;
                }
                else if (_nextBranchCandidateIndex == -1)
                {
                    _hintCommandText.gameObject.SetActive(false);
                    _currentCommandText.color = _errorCommandVisual.Color;
                }
                else
                {
                    if (showHintType == ShowBranchHintType.Integer) _hintCommandText.gameObject.SetActive(false);
                    else
                    {
                        _hintCommandText.text = safePart + _nextBranchHintText;
                        _hintCommandText.gameObject.SetActive(true);
                    }
                    
                    _currentCommandText.color = _normalCommandVisual.Color;
                }
            }

            
        }

        enum ShowBranchHintType
        {
            Normal, // word
            Integer // [x]
        }

        private void GetSafeAndUnsafePartsFromCurrentCommand(out string safePart, out string unsafePart, out CommandBranch lastSafeCommandBranch)
        {
            safePart = GenerateSafeCommandPart(out lastSafeCommandBranch);
            unsafePart = _currentCommandText.text.Remove(0, safePart.Length);
        }

        private string GenerateSafeCommandPart(out CommandBranch lastSafeCommandBranch)
        {
            // Build current command by Route indexes:
            // (in following example indexes may be wrong)
            // [0] [1] [0]
            // [/] [set] [grid]

            CommandBranch currentBranch = _commandsTree;
            StringBuilder sb = new StringBuilder("/");

            for (int i = 0; i < _commandRoute.Count; i++)
            {
                sb.Append(currentBranch.NextElements[_commandRoute[i]].KeyWord);
                sb.Append(' ');
                currentBranch = currentBranch.NextElements[_commandRoute[i]];
            }

            lastSafeCommandBranch = currentBranch;

            return sb.ToString();
        }

        private void DoCommand()
        {
            string formatCommand = FormatCommandForMessage();

            Action commandAction = FindActionForCommand();

            if (commandAction == null) PrintMessage($"Not done: {formatCommand}");
            else
            {
                PrintMessage($"Done: {formatCommand}");
                commandAction();
            }

            Action FindActionForCommand()
            {
                GetSafeAndUnsafePartsFromCurrentCommand(out var safePart, out var unsafePart, out var currentBranch);

                if (unsafePart != "")
                {
                    if (unsafePart == HINT_AVAILABLE_VARIANTS_SYMBOL.ToString())
                    {
                        return () =>
                        {
                            StringBuilder sb = new StringBuilder("Variants: ");
                            sb.Append(safePart);

                            if (currentBranch.NextElements != null)
                            {
                                foreach (var element in currentBranch.NextElements) sb.Append($"[{element.KeyWord}] ");
                            }

                            PrintMessage(sb.ToString());
                        };
                    }
                    else
                    {
                        // int
                        if (MathUtility.IsIntValue(unsafePart, out var intValue))
                        {
                            IntBranch ib = (IntBranch)Array.Find(currentBranch.NextElements, (info) => info is IntBranch);
                            if (ib == null) return null;
                            else return () => ib.IntCommandAction(intValue);
                        }

                        // other
                        else
                        {
                            CommandBranch b;
                            for (int i = 0; i < currentBranch.NextElements.Length; i++)
                            {
                                b = currentBranch.NextElements[i];
                                if (b == null) continue;

                                EnumBranch enumBranch = b as EnumBranch;

                                if (enumBranch != null)
                                {
                                    string[] variants = enumBranch.GetVariants;

                                    int variantIndex = Array.IndexOf(variants, unsafePart);

                                    if (variantIndex == -1) continue;
                                    else return enumBranch.GetEnumAction(variantIndex);
                                }
                                else if (b.KeyWord == unsafePart)
                                {
                                    _commandRoute.Add(_nextBranchCandidateIndex);
                                    _currentCommandText.text = GenerateSafeCommandPart(out _);

                                    return FindActionForCommand();
                                }
                            }

                            return null;
                        }
                    }
                }
                else
                {
                    if (currentBranch.CanBeFinal) return () => currentBranch.CommandAction?.Invoke();
                    else return null;
                }
            }
        }

        private void ClearCommand()
        {
            _commandRoute.Clear();
            _lastSafeBranch = _commandsTree;
            _currentCommandText.text = _commandsTree.KeyWord;
            UpdateCommandVisual();
        }

        #endregion Command

        #region Messages

        public void PrintMessage(string message)
        {
            PrintMessage(message, Color.white);
        }

        public void PrintMessage(string message, Color color)
        {
            for (int i = _messageLines.Length - 1; i >= 1; i--) _messageLines[i].SetMessage(_messageLines[i - 1]);
            _messageLines[0].SetMessage(message, color);
        }

        public void ClearMessages()
        {
            for (int i = 0; i < _messageLines.Length; i++) _messageLines[i].SetMessage("");
        }

        private string FormatCommandForMessage()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append(_currentCommandText.text);
            if (temp[temp.Length - 1] == ' ') temp = temp.Remove(temp.Length - 1, 1);

            return $"\"{temp}\"";
        }

        #endregion Messages

        private enum EnterType
        {
            Symbol,
            Delete,
            Space,
            Enter,
            Hide
        }

        private enum CommandColorType
        {
            Normal, // writing a command
            Error, // command can not be used
            Usable // command can be used
        }

        private struct CommandVisualData
        {
            private CommandColorType _colorType;
            private Color _color;
            public CommandColorType ColorType => _colorType;
            public Color Color => _color;

            public CommandVisualData(CommandColorType colorType, Color color)
            {
                _colorType = colorType;
                _color = color;
            }
        }

        #region Branch

        private class CommandBranch
        {
            // CommandBranch - is a part of full command. All commands consist of CommandBranches
            // E.g. [/] [set] [grid] [size] [x] - this command consist of 5 CommandBranches

            private string _keyWord;
            private CommandBranch[] _nextElements;
            private Action _commandAction;

            public string KeyWord => _keyWord;
            public bool CanBeFinal => _commandAction != null;
            public CommandBranch[] NextElements => _nextElements;
            public Action CommandAction => _commandAction;

            public CommandBranch() : this(
                keyWord: "",
                commandAction: null,
                nextElements: null) {}

            public CommandBranch(string keyWord) : this(
                keyWord: keyWord,
                commandAction: null,
                nextElements: null) {}

            public CommandBranch(string keyWord, Action commandAction) : this(
                keyWord, 
                commandAction: commandAction, 
                nextElements: null) {}

            public CommandBranch(string keyWord, CommandBranch[] nextElements) : this(
                keyWord: keyWord,
                commandAction: null,
                nextElements: nextElements) {}
                
            public CommandBranch(string keyWord, Action commandAction, CommandBranch[] nextElements)
            {
                _keyWord = keyWord;
                _commandAction = commandAction;
                _nextElements = nextElements;
            }
        }

        private abstract class ValueBranch : CommandBranch 
        {
            public ValueBranch() : base() {}
            public ValueBranch(string keyword) : base(keyword) {}
            public ValueBranch(string keyword, Action commandAction) : base(keyword, commandAction) {}
        }

        private class IntBranch : ValueBranch
        {
            private Action<int> _intCommandAction;
            public Action<int> IntCommandAction => _intCommandAction;

            public IntBranch() : base() {}

            public IntBranch(string keyword) : base(keyword) {}

            public IntBranch(string keyword, Action<int> intCommandAction) : base(keyword)
            {
                _intCommandAction = intCommandAction;
            }
        }

        private abstract class EnumBranch : ValueBranch
        {
            public EnumBranch() : base() {}
            public EnumBranch(string keyword) : base(keyword) {}
            public virtual string[] GetVariants => null;
            public virtual Action GetEnumAction(int index) => null;
        }

        private class EnumBranch<T> : EnumBranch where T : System.Enum
        {
            private Action<T> _enumCommandAction;

            public EnumBranch() : base() {}

            public EnumBranch(string keyword) : base(keyword) {}

            public EnumBranch(string keyword, Action<T> enumCommandAction) : base(keyword)
            {
                _enumCommandAction = enumCommandAction;
            }

            private T[] EnumValues
            {
                get
                {
                    if (_enumValues == null) _enumValues = EnumUtility.GetValues<T>();;
                    return _enumValues;
                }
            }

            private T[] _enumValues;

            public override string[] GetVariants
            {
                get
                {
                    if (_stringValues == null)
                    {
                        _stringValues = new string[EnumValues.Length];
                        for (int v = 0; v < EnumValues.Length; v++) _stringValues[v] = EnumValues[v].ToString().ToLower();
                    }
                    
                    return _stringValues;
                }
            }

            string[] _stringValues = null;

            public override Action GetEnumAction(int index)
            {
                return () => _enumCommandAction(EnumValues[index]);
            }
        }

        #endregion Branch
    }
}