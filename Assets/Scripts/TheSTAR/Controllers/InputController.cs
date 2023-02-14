using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

public sealed class InputController : MonoBehaviour
{
    public bool canControlWithArrows = true;
    private bool _needCheckAnyKeys = false;
    private bool _isInit = false;
    private IArrowsControlable _arrowsControlable;
    private ControlKit[] _controlKits = new ControlKit[0];

    public delegate void AnyKeyDownEventHandler(KeyCode keyCode);
    public event AnyKeyDownEventHandler OnAnyKeyDown;

    private static readonly KeyCode[] keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Where(k => ((int)k < (int)KeyCode.Mouse0)).ToArray();

    public void Init(IArrowsControlable arrowsControlable, ControlKit[] controlKits = null)
    {
        _arrowsControlable = arrowsControlable;
        _controlKits = controlKits == null ? new ControlKit[0] : controlKits;

        _isInit = true;
    }

    public void SetNeedCheckAnyKeys(bool value)
    {
        _needCheckAnyKeys = value;

        if (value) _arrowsControlable.Stop();
    }

    void Update()
    {
        if (!_isInit) return;

        if (!_needCheckAnyKeys)
        {
            // Arrows Control

            if (canControlWithArrows)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) _arrowsControlable.SetForceLeft(true);
                if (Input.GetKeyUp(KeyCode.LeftArrow)) _arrowsControlable.SetForceLeft(false);

                if (Input.GetKeyDown(KeyCode.RightArrow)) _arrowsControlable.SetForceRight(true);
                if (Input.GetKeyUp(KeyCode.RightArrow)) _arrowsControlable.SetForceRight(false);

                if (Input.GetKeyDown(KeyCode.UpArrow)) _arrowsControlable.SetForceUp(true);
                if (Input.GetKeyUp(KeyCode.UpArrow)) _arrowsControlable.SetForceUp(false);

                if (Input.GetKeyDown(KeyCode.DownArrow)) _arrowsControlable.SetForceDown(true);
                if (Input.GetKeyUp(KeyCode.DownArrow)) _arrowsControlable.SetForceDown(false);

                if (_arrowsControlable.ControlableWithWASD)
                {
                    if (Input.GetKeyDown(KeyCode.A)) _arrowsControlable.SetForceLeft(true);
                    if (Input.GetKeyUp(KeyCode.A)) _arrowsControlable.SetForceLeft(false);

                    if (Input.GetKeyDown(KeyCode.D)) _arrowsControlable.SetForceRight(true);
                    if (Input.GetKeyUp(KeyCode.D)) _arrowsControlable.SetForceRight(false);

                    if (Input.GetKeyDown(KeyCode.W)) _arrowsControlable.SetForceUp(true);
                    if (Input.GetKeyUp(KeyCode.W)) _arrowsControlable.SetForceUp(false);

                    if (Input.GetKeyDown(KeyCode.S)) _arrowsControlable.SetForceDown(true);
                    if (Input.GetKeyUp(KeyCode.S)) _arrowsControlable.SetForceDown(false);
                }
            }

            if (!Input.anyKeyDown) return;
        
            // Control Kits

            foreach (var controlKit in _controlKits)
            {
                if (Input.GetKeyDown(controlKit.KeyCode)) controlKit.Action?.Invoke();
            }
        }
        else
        {
            // Check Any

            KeyCode keyCode;
            for (int i = 0; i < keyCodes.Length; i++)
            {
                keyCode = (KeyCode)i;
                if (Input.GetKeyDown(keyCode)) 
                    OnAnyKeyDown?.Invoke(keyCode);
            }
        }
    }

    public struct ControlKit
    {
        private KeyCode _keyCode;
        private Action _action;

        public KeyCode KeyCode => _keyCode;
        public Action Action => _action;

        public ControlKit(KeyCode keyCode, Action action)
        {
            _keyCode = keyCode;
            _action = action;
        }
    }
}