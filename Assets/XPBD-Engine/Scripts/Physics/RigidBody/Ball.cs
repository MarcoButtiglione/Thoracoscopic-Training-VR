using UnityEngine;

namespace XPBD_Engine.Scripts.Physics.RigidBody
{
    [RequireComponent(typeof(MeshFilter))]
    public class Ball : MonoBehaviour
    {
        public float radius;
        public Vector3 startingVelocity;
    
        private Mesh _mesh;
        private Vector3 _pos;
        private float _rad;
        [SerializeField]private Vector3 _vel;
    
        // Start is called before the first frame update
        void Start()
        {
            _pos = gameObject.transform.position;
            _rad = radius;
            _vel = startingVelocity;
        
            /*
        var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _mesh = Instantiate(obj.GetComponent<MeshFilter>().mesh);
        Destroy(obj);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        */
        }

        public void Simulate(Vector3 gravity, float dt,Vector3 worldSize)
        {
            _vel += gravity*dt;
            _pos+=_vel*dt;

            var size = worldSize;

            if (_pos.x < -size.x) {
                _pos.x = -size.x; _vel.x = -_vel.x;
            }
            if (_pos.x >  size.x) {
                _pos.x =  size.x; _vel.x = -_vel.x;
            }
            if (_pos.z < -size.z) {
                _pos.z = -size.z; _vel.z = -_vel.z;
            }
            if (_pos.z >  size.z) {
                _pos.z =  size.z; _vel.z = -_vel.z;
            }
            if (_pos.y < _rad) {
                _pos.y = _rad; _vel.y = -_vel.y;
            }

            gameObject.transform.position = _pos;


            //this.visMesh.position.copy(_pos);
            //this.visMesh.geometry.computeBoundingSphere();

        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
