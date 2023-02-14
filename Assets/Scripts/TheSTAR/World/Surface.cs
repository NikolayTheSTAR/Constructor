using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Surface : MonoBehaviour
    {
        [SerializeField] private Color _shadowColor;
        [SerializeField] private GameObject _shadowObject;

        public void UpdateShadow(Vector2 shadowPos)
        {
            _shadowObject.transform.position = new Vector3(shadowPos.x, shadowPos.y, _shadowObject.transform.position.z);
        }
    }
}