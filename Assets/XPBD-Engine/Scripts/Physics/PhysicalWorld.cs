using System.Collections.Generic;
using UnityEngine;
using XPBD_Engine.Scripts.Physics.RigidBody;
using XPBD_Engine.Scripts.Physics.SoftBody;

namespace XPBD_Engine.Scripts.Physics
{
    public class PhysicalWorld : MonoBehaviour
    {
        public Vector3 gravity;
        public BoxCollider worldCollider;
        private Vector3 _worldBoxMin;
        private Vector3 _worldBoxMax;
        public int numSubsteps = 10;
        public bool paused;

        private SoftBodyClassic[] _classicSoftBodies;
        private SoftBodyAdvanced[] _advancedSoftBodies;
        private Ball[] _balls;

        private bool _isPressedJump;
        private bool _isPressedSqueeze;
        private void Start()
        {
            _classicSoftBodies = FindObjectsOfType<SoftBodyClassic>();
            _advancedSoftBodies = FindObjectsOfType<SoftBodyAdvanced>();
            _balls = FindObjectsOfType<Ball>();
            var worldSize = worldCollider.size;
            var worldCenter =worldCollider.center;
            _worldBoxMin = worldCenter - worldSize / 2.0f;
            _worldBoxMax = worldCenter + worldSize / 2.0f;
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
                    _classicSoftBodies[i].PreSolve(sdt, gravity,_worldBoxMax,_worldBoxMin);
					
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
                    _advancedSoftBodies[i].PreSolve(sdt, gravity,_worldBoxMax,_worldBoxMin);
					
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
                _balls[i].Simulate(gravity,Time.fixedDeltaTime,_worldBoxMax);
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
