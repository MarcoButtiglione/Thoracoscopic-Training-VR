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
        
        private SoftBodyClassic[] _softBodies;          //The softbodies in the world

        
        private void OnEnable()
        {
            instance = this;
            _softBodies = FindObjectsOfType<SoftBodyClassic>();
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
                foreach (var softBody in _softBodies)
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
            HandleSimulationSoftBody(sdt);

        }
        private void HandleSimulationSoftBody(float sdt)
        {
            if (_softBodies.Length ==0)
                return;
            for (var step = 0; step < numSubsteps; step++) {
                for (var i = 0; i < _softBodies.Length; i++) 
                    _softBodies[i].PreSolve(sdt, gravity,worldBoundType,worldBoundCenter,worldBoundSize,worldBoundRadius,worldCapsulePos1,worldCapsulePos2);
					
                for (var i = 0; i < _softBodies.Length; i++) 
                    _softBodies[i].Solve(sdt);

                for (var i = 0; i < _softBodies.Length; i++) 
                    _softBodies[i].PostSolve(sdt);
            }
        }

        public IEnumerable<SoftBodyClassic> GetSoftBodies()
        {
            return _softBodies;
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
