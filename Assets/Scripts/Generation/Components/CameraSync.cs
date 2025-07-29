using Unity.Entities;
using UnityEngine;

namespace Generation.Components
{
    public class CameraSync : MonoBehaviour
    {
        private Entity _cameraEntity;
        private EntityManager _em;

        private void Start()
        {
            _em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _cameraEntity = _em.CreateEntity(typeof(Camera));
        }

        private void Update()
        {
            _em.SetComponentData(_cameraEntity, new Camera { Position = transform.position });
        }
    }
}
