using AndrewDowsett.ObjectPooling;
using UnityEngine;

namespace AndrewDowsett.ObjectUIBinding
{
    public class ObjectBoundToUI : MonoBehaviour
    {
        private UIBoundToObject boundUI;

        public void Show(Color color = default)
        {
            boundUI = ObjectPool.Pools["UIBoundToObject"].Get() as UIBoundToObject;
            if (boundUI == null)
            {
                Debug.Log($"Couldn't get object {typeof(UIBoundToObject).ToString()} from UIBoundToObject for {gameObject.name}.");
                return;
            }
            boundUI.SetColor(color);
            boundUI.Bind(this);
        }

        public void Hide()
        {
            if (boundUI != null)
                boundUI.Despawn();
        }

        private void OnDisable()
        {
            ObjectPool _pool = ObjectPool.Pools["UIBoundToObject"];
            if (boundUI != null && _pool != null)
                _pool.Release(boundUI);
        }
    }
}