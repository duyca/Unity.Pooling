using Cysharp.Threading.Tasks;
using UnityEngine;
using ZBase.Collections.Pooled.Generic;
using ZBase.Foundation.Pooling;
using ZBase.Foundation.Pooling.AddressableAssets;
using Grid = Sample.Environment.Grid;

namespace Pooling.Sample
{
    public class AddressableGameObjectPoolSample1 : MonoBehaviour
    {
        [SerializeField]private AssetRefGameObjectPrefab _prefab;
        private readonly Grid _grid = new(20, 15, true);
        private readonly List<GameObject> _spawned = new();
        
        private async UniTask Spawn()
        {
            var pool = SharedPool.Of<GlobalAssetRefGameObjectPool>();
            var go = await pool.Rent(_prefab);
            go.transform.position = _grid.GetAvailableSlot().position;
            go.SetActive(true);
            _spawned.Add(go);
        }

        private void Return()
        {
            var pool = SharedPool.Of<GlobalAssetRefGameObjectPool>();
            foreach (var go in _spawned)
                pool.Return(go);
            FreeSlot();
            _spawned.Clear();
        }

        private void FreeSlot()
        {
            foreach (var go in _spawned)
                _grid.FreeSlot(go.transform.position);
        }
        
        private async UniTask SpawnDisposableItems()
        {
            var pool = SharedPool.Of<AddressGameObjectPool>();
            var context = pool.DisposableContext();
            using var go = await context.Rent();
            go.Instance.transform.position = _grid.GetAvailableSlot().position;
            go.Instance.SetActive(true);
            _spawned.Add(go.Instance);
            await UniTask.Delay(1500);
        }
        
        private void ReleaseAll()
        {
            var pool = SharedPool.Of<AddressGameObjectPool>();
            pool.ReleaseInstances(0);
            _spawned.Clear();
        }
        
        private void OnDisable()=> ReleaseAll();
        
        
        private async void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 50), "Spawn"))
            {
                for (int i = 0; i < 100; i++)
                {
                    Spawn().Forget();
                }
                Debug.Log("Spawn 100 item from the AddressableGameObject pool");
            }
            
            if (GUI.Button(new Rect(10, 70, 150, 50), "Spawn Disposable Item"))
            {
                await SpawnDisposableItems();
                Debug.Log("Item automatically returned to pool when context is disposed");
            }
            
            if (GUI.Button(new Rect(10, 130, 150, 50), "Return"))
            {
                Return();
                Debug.Log("Return all item to the pool");
            }
            
            if (GUI.Button(new Rect(10, 190, 150, 50), "Release All"))
            {
                ReleaseAll();
                Debug.Log("Release all item from the pool");
            }
        }
    }
}