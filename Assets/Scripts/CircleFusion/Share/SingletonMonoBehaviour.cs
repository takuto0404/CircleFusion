using System;
using UnityEngine;

namespace CircleFusion.Share
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    Type t = typeof(T);

                    instance = (T)FindObjectOfType(t);
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (this != Instance)
            {
                Destroy(this);
                Debug.LogError(
                    typeof(T) +
                    " は既に他のGameObjectにアタッチされているため、コンポーネントを破棄しました." +
                    " アタッチされているGameObjectは " + Instance.gameObject.name + " です.");
            }
        }
    }
}