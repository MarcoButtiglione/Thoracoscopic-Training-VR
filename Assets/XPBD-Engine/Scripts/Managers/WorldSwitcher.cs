using System.Collections.Generic;
using UnityEngine;

namespace XPBD_Engine.Scripts.Managers
{
    public class WorldSwitcher : MonoBehaviour
    {
        public List<GameObject> worlds;
        public List<MeshRenderer> buttonMeshRenderers;
        public Material activeMaterial;
        public Material inactiveMaterial;
        
        private int _currentWorldIndex;
        private void Awake()
        {
            _currentWorldIndex = 0;
            foreach (var world in worlds)
            {
                world.SetActive(false);
            }
            worlds[_currentWorldIndex].SetActive(true);
            buttonMeshRenderers[_currentWorldIndex].material = activeMaterial;
            
            
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                NextWorld();
            }
        }
        private void NextWorld()
        {
            worlds[_currentWorldIndex].SetActive(false);
            buttonMeshRenderers[_currentWorldIndex].material = inactiveMaterial;
            _currentWorldIndex++;
            if (_currentWorldIndex >= worlds.Count)
            {
                _currentWorldIndex = 0;
            }
            worlds[_currentWorldIndex].SetActive(true);
            buttonMeshRenderers[_currentWorldIndex].material = activeMaterial;
        }
        
        public void ChangeWorldByIndex(int index)
        {
            worlds[_currentWorldIndex].SetActive(false);
            buttonMeshRenderers[_currentWorldIndex].material = inactiveMaterial;
            _currentWorldIndex = index;
            worlds[_currentWorldIndex].SetActive(true);
            buttonMeshRenderers[_currentWorldIndex].material = activeMaterial;
        }
    }
}
