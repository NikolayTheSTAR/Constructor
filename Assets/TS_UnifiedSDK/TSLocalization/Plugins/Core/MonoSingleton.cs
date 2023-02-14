using UnityEngine;

namespace GM.Localization
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = FindObjectOfType(typeof(T)) as T;

                    if (s_Instance == null)
                    {
                        var gameObject = new GameObject(typeof(T).Name);
                        DontDestroyOnLoad(gameObject);

                        s_Instance = gameObject.AddComponent(typeof(T)) as T;
                    }
                }

                return s_Instance;
            }
        }

        protected virtual void OnDestroy()
        {
            //      Debug.Log("trying to destroy");
            //      Debug.Log(s_Instance);
            //      if (s_Instance)
            //	Destroy(s_Instance);

            //s_Instance = null;
            //s_IsDestroyed = true;
        }

    } 
}