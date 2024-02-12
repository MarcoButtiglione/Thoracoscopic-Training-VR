using System.Collections.Generic;
using UnityEngine;
using XPBD_Engine.Scripts.Managers;

namespace XPBD_Activity
{
    public class MinigameSwitcher: MonoBehaviour
    {
        [SerializeField] private ActivityManager activityManager;
        [SerializeField] private MeshRenderer buttonOrderedMeshRenderers;
        [SerializeField] private MeshRenderer buttonRandomMeshRenderers;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;
        
        
        private void Awake()
        {
            HandleChangeButtonMaterial(activityManager.activityType);
        }

        private void HandleChangeButtonMaterial(ActivityType activityType)
        {
            if (activityType == ActivityType.Ordered)
            {
                buttonOrderedMeshRenderers.material = activeMaterial;
                buttonRandomMeshRenderers.material = inactiveMaterial;
            }
            else
            {
                buttonRandomMeshRenderers.material = activeMaterial;
                buttonOrderedMeshRenderers.material = inactiveMaterial;
            }
        }
        public void ClickOrderedButton()
        {
            ChangeActivityType(ActivityType.Ordered);
        }
        public void ClickRandomButton()
        {
            ChangeActivityType(ActivityType.Random);
        }
        
        public void ChangeActivityType(ActivityType activityType)
        {
            activityManager.ChangeActivityType(activityType, out var newActivity);
            HandleChangeButtonMaterial(newActivity);
        }
    }
}