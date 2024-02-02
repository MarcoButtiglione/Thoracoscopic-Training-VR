using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using XPBD_Engine.Scripts.Physics;
using XPBD_Engine.Scripts.Physics.RigidBody;
using XPBD_Engine.Scripts.Physics.SoftBody;
using XPBD_Engine.Scripts.Utilities;

namespace XPBD_Engine.Scripts.Physics
{
    public class PhysicalWorld : MonoBehaviour
    {
        public static PhysicalWorld instance;

        public Vector3 gravity;                         //The gravity of the world
        public WorldBoundType worldBoundType;           //The type of the world bound
        public Vector3 worldBoundSize;                  //The size of the world bound
        public Vector3 worldBoundCenter;                //The center of the world bound
        public float worldBoundRadius;                  //The radius of the world bound
        public Vector3 worldCapsulePos1;                //The first position of the capsule
        public Vector3 worldCapsulePos2;                //The second position of the capsule
        public int numSubsteps = 10;                    //The number of substeps
        public bool paused;                             //If the simulation is paused
        
        private SoftBodyClassic[] _classicSoftBodies;
        //private SoftBodyAdvanced[] _advancedSoftBodies;
        //private Ball[] _balls;
        
        private void OnEnable()
        {
            instance = this;
            _classicSoftBodies = FindObjectsOfType<SoftBodyClassic>();
        }
        private void OnDisable()
        {
            instance = null;
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                paused = !paused;
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                foreach (var softBody in _classicSoftBodies)
                {
                    softBody.RestartSoftBody();
                }
            }
        }
        private void FixedUpdate()
        {
            Simulate();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            switch (worldBoundType)
            {
                case WorldBoundType.Cube:
                    Gizmos.DrawWireCube(worldBoundCenter, worldBoundSize);
                    break;
                case WorldBoundType.Sphere:
                    Gizmos.DrawWireSphere(worldBoundCenter, worldBoundRadius);
                    break;
                case WorldBoundType.Capsule:
                    CustomGizmo.DrawWireCapsule(worldCapsulePos1,worldCapsulePos2, worldBoundRadius);
                    break;
                case WorldBoundType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                
            }
        }    

    
        private void Simulate()
        {
            if (paused)
                return;
            
            
            
            var sdt = Time.fixedDeltaTime / numSubsteps;
            HandleSimulationSoftBodyClassic(sdt);
            //HandleSimulationSoftBodyAdvanced(sdt);
            //HandleSimulationBalls();
        }
        private void HandleSimulationSoftBodyClassic(float sdt)
        {
            if (_classicSoftBodies.Length ==0)
                return;
            for (var step = 0; step < numSubsteps; step++) {
                for (var i = 0; i < _classicSoftBodies.Length; i++) 
                    _classicSoftBodies[i].PreSolve(sdt, gravity,worldBoundType,worldBoundCenter,worldBoundSize,worldBoundRadius,worldCapsulePos1,worldCapsulePos2);
					
                for (var i = 0; i < _classicSoftBodies.Length; i++) 
                    _classicSoftBodies[i].Solve(sdt);

                for (var i = 0; i < _classicSoftBodies.Length; i++) 
                    _classicSoftBodies[i].PostSolve(sdt);
            }
        }
        /*
         
        private void HandleSimulationSoftBodyAdvanced(float sdt)
        {
            if (_advancedSoftBodies.Length ==0)
                return;
            for (var step = 0; step < numSubsteps; step++) {
                for (var i = 0; i < _advancedSoftBodies.Length; i++) 
                    _advancedSoftBodies[i].PreSolve(sdt, gravity,worldBoundSize,worldBoundCenter);
					
                for (var i = 0; i < _advancedSoftBodies.Length; i++) 
                    _advancedSoftBodies[i].Solve(sdt);

                for (var i = 0; i < _advancedSoftBodies.Length; i++) 
                    _advancedSoftBodies[i].PostSolve(sdt);
            }
            //for (var i = 0; i < _advancedSoftBodies.Length; i++) 
            //    _advancedSoftBodies[i].UpdateMeshes();
        }
        private void HandleSimulationBalls()
        {
            if (_balls.Length ==0)
                return;
            for (var i = 0; i < _balls.Length; i++)
                _balls[i].Simulate(gravity,Time.fixedDeltaTime,worldBoundSize);
        }
        */
        public IEnumerable<SoftBodyClassic> GetSoftBodies()
        {
            return _classicSoftBodies;
        }

        public void SwitchPaused()
        {
            paused = !paused;
        }
    }
}


public enum WorldBoundType
{
    None,
    Cube,
    Sphere,
    Capsule
}
