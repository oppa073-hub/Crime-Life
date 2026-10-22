using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CrimeLife.Core
{
    // 씬 전환과 저장/불러오기를 맡는 상주 오브젝트. 씬이 바뀌어도 살아남아야 해서 싱글톤으로 둔다.
    // 상태 자체는 각 컴포넌트(ISaveable)가 들고 있고, 여기서는 옮겨 담기만 한다.
    public class GameSession : MonoBehaviour
    {
        private static GameSession _instance;

        public static GameSession Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("GameSession");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<GameSession>();
                }
                return _instance;
            }
        }

        // 만들지 않고 있는 것만 돌려준다. 종료 중 OnDestroy에서 구독 해제할 때 쓴다.
        public static GameSession Current => _instance;

        private SaveData _data;
        private bool _restoreOnLoad;
        private string _pendingSpawnId;
        private InputAction _saveKey;
        private InputAction _loadKey;

        public event Action<string> Notice;

        // 게임을 켜자마자 만들어 둬야 F5/F9가 어느 씬에서든 먹는다
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() => _ = Instance;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            _saveKey = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/f5");
            _loadKey = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/f9");
            _saveKey.performed += _ => SaveToDisk();
            _loadKey.performed += _ => LoadFromDisk();
            _saveKey.Enable();
            _loadKey.Enable();
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _saveKey.Dispose();
            _loadKey.Dispose();
        }

        // 현재 상태를 담아 두고 다른 씬으로 간다. spawnId는 도착 지점(SpawnPoint)의 이름이다.
        public void GoTo(string sceneName, string spawnId)
        {
            _data = Capture(sceneName);
            _pendingSpawnId = spawnId;
            _restoreOnLoad = true;
            SceneManager.LoadScene(sceneName);
        }

        public string ConsumeSpawnId()
        {
            string id = _pendingSpawnId;
            _pendingSpawnId = null;
            return id;
        }

        public void SaveToDisk()
        {
            SaveFile.Write(Capture(SceneManager.GetActiveScene().name));
            Notice?.Invoke("저장했다");
        }

        public void LoadFromDisk()
        {
            if (!SaveFile.TryRead(out SaveData data))
            {
                Notice?.Invoke("불러올 저장 파일이 없다");
                return;
            }

            _data = data;
            _pendingSpawnId = null;
            _restoreOnLoad = true;
            SceneManager.LoadScene(data.SceneName);
        }

        private static SaveData Capture(string sceneName)
        {
            var data = new SaveData { SceneName = sceneName };
            foreach (ISaveable saveable in FindSaveables())
                saveable.Save(data);
            return data;
        }

        // sceneLoaded는 Awake/OnEnable 이후, Start 이전에 불려서 UI가 복원된 값을 읽게 된다
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_restoreOnLoad) return;
            _restoreOnLoad = false;

            foreach (ISaveable saveable in FindSaveables())
                saveable.Load(_data);
        }

        private static System.Collections.Generic.List<ISaveable> FindSaveables()
        {
            var result = new System.Collections.Generic.List<ISaveable>();
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is ISaveable saveable) result.Add(saveable);
            }
            return result;
        }
    }
}
