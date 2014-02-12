using UnityEngine;
namespace RoomOfRequirement.Architectural {
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> {
        public bool IsGameStatic;
        private static T _instance = null;

        public static T Instance {
            get {
                // Instance required for the first time, we look for it
                if (_instance == null) {
                    _instance = GameObject.FindObjectOfType(typeof(T)) as T;

                    // Object not found, we create a temporary one
                    if (_instance == null) {
                        Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");
                        _instance = new GameObject("Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                        // Problem during the creation, this should not happen
                        if (_instance == null) {
                            Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                        }
                    }
                    _instance.Init();
                }
                return _instance;
            }
        }

        // If no other monobehaviour request the instance in an awake function
        // executing before this one, no need to search the object.
        private void Awake() {
            if (_instance == null) {
                _instance = this as T;
                _instance.Init();

                if (IsGameStatic) {
                    DontDestroyOnLoad(gameObject);
                }
            } else if (_instance != this) {
                Destroy(gameObject);
            }
        }

        // This function is called when the instance is used the first time
        // Put all the initializations you need here, as you would do in Awake
        public virtual void Init() { }

        // Make sure the instance isn't referenced anymore when the user quit, just in case.
        private void OnApplicationQuit() {
            _instance = null;
        }
    }
}
