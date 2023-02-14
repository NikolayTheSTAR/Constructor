using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheSTAR.Utility;

namespace TheSTAR.Constructor
{
    public class ConstructorCamera : MonoBehaviour, IArrowsControlable, IResetable
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Camera _camera;

        private const float DEFAULT_SPEED = 10;
        public const float MIN_SPEED_SCALE = 0.1f;
        public const float MAX_SPEED_SCALE = 10;
        public const float MIN_SIZE_SCALE = 0.1f;
        public const float MAX_SIZE_SCALE = 10;
        private const float DIAGONAL_SPEEP_MULTIPLY = 0.707f;
        private float _speed = DEFAULT_SPEED;
        private bool _moveRight;
        private bool _moveLeft;
        private bool _moveUp;
        private bool _moveDown;

        private float _currentSizeScale = 1;
        public float CurrentSizeScale => _currentSizeScale;
        private float _currentSpeedScale = 1;
        public float CurrentSpeedScale => _currentSpeedScale;


        public bool ControlableWithWASD => true;

        private const int DEFAULT_CAMERA_SIZE = 7;

        public void Update()
        {
            float velocityX = 0;
            float velocityY = 0;

            if (_moveRight) velocityX += _speed;
            if (_moveLeft) velocityX -= _speed;
            if (_moveUp) velocityY += _speed;
            if (_moveDown) velocityY -= _speed;

            if (velocityX != 0 && velocityY != 0)
            {
                velocityX *= DIAGONAL_SPEEP_MULTIPLY;
                velocityY *= DIAGONAL_SPEEP_MULTIPLY;
            }

            _rigidbody.velocity = new Vector2(velocityX, velocityY);

            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        }
        
        public void SetForceUp(bool force) => _moveUp = force;
        public void SetForceDown(bool force) => _moveDown = force;
        public void SetForceRight(bool force) => _moveRight = force;
        public void SetForceLeft(bool force) => _moveLeft = force;

        public void Stop()
        {
            SetForceUp(false);
            SetForceDown(false);
            SetForceLeft(false);
            SetForceRight(false);
        }
        
        public void SetSizeScale(float scale)
        {
            scale = MathUtility.Limit(scale, MIN_SIZE_SCALE, MAX_SIZE_SCALE);
            _camera.orthographicSize = DEFAULT_CAMERA_SIZE * scale;
            _currentSizeScale = scale;
        }
        
        public void SetSpeedScale(float scale)
        {
            scale = MathUtility.Limit(scale, MIN_SPEED_SCALE, MAX_SPEED_SCALE);
            _speed = DEFAULT_SPEED * scale;
            _currentSpeedScale = scale;
        }

        public void Reset()
        {
            SetSizeScale(1);
            SetSpeedScale(1);
        }

        public void SetColor(Color color)
        {
            _camera.backgroundColor = color;
        }
    }
}