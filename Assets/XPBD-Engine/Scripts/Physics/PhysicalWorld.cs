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
        public Vector3 gravity;
        public WorldBoundType worldBoundType;
        public Vector3 worldBoundSize;
        public Vector3 worldBoundCenter;
        public float worldBoundRadius;
        public Vector3 worldCapsulePos1;
        public Vector3 worldCapsulePos2;
        public int numSubsteps = 10;
        public bool paused;
        
        
        private SoftBodyClassic[] _classicSoftBodies;
        private SoftBodyAdvanced[] _advancedSoftBodies;
        private Ball[] _balls;

        
        private void Start()
        {
            _classicSoftBodies = FindObjectsOfType<SoftBodyClassic>();
            _advancedSoftBodies = FindObjectsOfType<SoftBodyAdvanced>();
            _balls = FindObjectsOfType<Ball>();
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
            HandleSimulationSoftBodyAdvanced(sdt);
            HandleSimulationBalls();
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
