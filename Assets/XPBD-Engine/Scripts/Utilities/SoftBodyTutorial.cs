using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace XPBD_Engine.Scripts.Utilities
{
	[RequireComponent(typeof(MeshFilter))]
	public class SoftBodyTutorial : MonoBehaviour
	{
		public TextAsset modelJson;
		[FormerlySerializedAs("tetMesh")] public TetVisMesh tetVisMesh;

		private Mesh _mesh;
	
		[Range(0f,500f)]public float edgeCompliance= 100.0f;
		[Range(0f,500f)]public float volCompliance = 0.0f;

		private int _numParticles;
		private int _numTets;
		private float[] _pos; //Use an array of float instead of a list of vect3 because is more efficient and the mesh in are usually stored in this way
		private float[] _prevPos;
		private float[] _vel;

		private int[] _tetIds;
		private int[] _edgeIds;
		private float[] _restVol;
		private float[] _edgeLengths;
		private float[] _invMass;

		private float _edgeCompliance;
		private float _volCompliance;
	
		private float[] _temp;
		private float[] _grads;

		private int[][] _volIdOrder;

	
		// Start is called before the first frame update
		void Start()
		{
			tetVisMesh = JsonUtility.FromJson<TetVisMesh>(modelJson.text);
		
			_numParticles = tetVisMesh.verts.Length / 3;
			_numTets = tetVisMesh.tetIds.Length / 4;
			_pos = tetVisMesh.verts;
			_prevPos = new float[tetVisMesh.verts.Length / 3];
			_vel = new float[3 * _numParticles];

			_tetIds = tetVisMesh.tetIds;
			_edgeIds = tetVisMesh.tetEdgeIds;
			_restVol = new float[_numTets];
			_edgeLengths = new float[_edgeIds.Length / 2];	
			_invMass = new float[_numParticles];
		
			_edgeCompliance = edgeCompliance;
			_volCompliance = volCompliance;

			_temp = new float[4 * 3];
			_grads = new float[4 * 3];

			InitPhysics();

			_mesh = new Mesh();
			GetComponent<MeshFilter>().mesh = _mesh;
			_mesh.Clear();
		
			_mesh.SetVertexBufferParams(_pos.Length/3, new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3));
			_mesh.SetVertexBufferData<float>(_pos, 0, 0, _pos.Length);
			_mesh.SetIndexBufferParams( tetVisMesh.tetSurfaceTriIds.Length, IndexFormat.UInt32);
			_mesh.SetIndexBufferData<int>( tetVisMesh.tetSurfaceTriIds, 0, 0,  tetVisMesh.tetSurfaceTriIds.Length);
 
			_mesh.SetSubMesh(0, new SubMeshDescriptor(0, tetVisMesh.tetSurfaceTriIds.Length));
			_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
		
			_mesh.RecalculateNormals();
		

			_volIdOrder = new[]
			{
				new[] { 1, 3, 2 },
				new[] { 0, 2, 3 },
				new[] { 0, 3, 1 },
				new[] { 0, 1, 2 }
			};
		}
		// Update is called once per frame
		void Update()
		{
        
		}
    
		public void translate(float x,float y,float z)
		{
			for (var i = 0; i < _numParticles; i++) {
				VectorHelper.VecAdd(_pos,i, new[]{x,y,z},0);
				VectorHelper.VecAdd(_prevPos,i, new[]{x,y,z},0);
			}
		}


		private void InitPhysics()
		{
			for (var i = 0; i < _numTets; i++) {
				var vol =GetTetVolume(i);
				_restVol[i] = vol;
				var pInvMass = vol > 0.0f ? 1.0f / (vol / 4.0f) : 0.0f;
				_invMass[_tetIds[4 * i]] += pInvMass;
				_invMass[_tetIds[4 * i + 1]] += pInvMass;
				_invMass[_tetIds[4 * i + 2]] += pInvMass;
				_invMass[_tetIds[4 * i + 3]] += pInvMass;
			}
			for (var i = 0; i < _edgeLengths.Length; i++) {
				var id0 = _edgeIds[2 * i];
				var id1 = _edgeIds[2 * i + 1];
				_edgeLengths[i] = (float)Math.Sqrt(VectorHelper.VecDistSquared(_pos,id0, _pos,id1));
			}
		}
		private float GetTetVolume(int nr) 
		{
			var id0 = _tetIds[4 * nr];
			var id1 = _tetIds[4 * nr + 1];
			var id2 = _tetIds[4 * nr + 2];
			var id3 = _tetIds[4 * nr + 3];
			VectorHelper.VecSetDiff(_temp,0, _pos,id1, _pos,id0);
			VectorHelper.VecSetDiff(_temp,1, _pos,id2, _pos,id0);
			VectorHelper.VecSetDiff(_temp,2, _pos,id3, _pos,id0);
			VectorHelper.VecSetCross(_temp,3, _temp,0, _temp,1);
			return VectorHelper.VecDot(_temp,3, _temp,2) / 6.0f;
		}

		private void UpdateMeshes() 
		{
			_mesh.SetVertexBufferParams(_pos.Length/3, new VertexAttributeDescriptor(VertexAttribute.Position,VertexAttributeFormat.Float32,3));
			_mesh.SetVertexBufferData<float>(_pos, 0, 0, _pos.Length);
			_mesh.SetIndexBufferParams( tetVisMesh.tetSurfaceTriIds.Length, IndexFormat.UInt32);
			_mesh.SetIndexBufferData<int>( tetVisMesh.tetSurfaceTriIds, 0, 0,  tetVisMesh.tetSurfaceTriIds.Length);
 
			_mesh.SetSubMesh(0, new SubMeshDescriptor(0, tetVisMesh.tetSurfaceTriIds.Length));
			_mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
		
			_mesh.RecalculateNormals();
		}
    
    
		public void PreSolve(float dt, float[] gravity)
		{
			for (var i = 0; i < _numParticles; i++) {
				if (_invMass[i] == 0.0)
					continue;
				VectorHelper.VecAdd(_vel,i, gravity,0, dt);
				VectorHelper.VecCopy(_prevPos,i, _pos,i);
				VectorHelper.VecAdd(_pos,i, _vel,i, dt);
				var y = _pos[3 * i + 1];
				if (y < 0.0) {
					VectorHelper.VecCopy(_pos,i, _prevPos,i);
					_pos[3 * i + 1] = 0.0f;
				}
			}
	    
		}
		public void Solve(float dt)
		{
			SolveEdges(edgeCompliance, dt);
			SolveVolumes(volCompliance, dt);
		}
		public void PostSolve(float dt)
		{
			for (var i = 0; i < _numParticles; i++) {
				if (_invMass[i] == 0.0)
					continue;
				VectorHelper.VecSetDiff(_vel,i, _pos,i, _prevPos,i, 1.0f / dt);
			}
			UpdateMeshes();
		}
    
		private void SolveEdges(float compliance,float dt)
		{
			var alpha = compliance / dt /dt;

			for (var i = 0; i < _edgeLengths.Length; i++) {
				var id0 = _edgeIds[2 * i];
				var id1 = _edgeIds[2 * i + 1];
				var w0 = _invMass[id0];
				var w1 = _invMass[id1];
				var w = w0 + w1;
				if (w == 0.0)
					continue;

				VectorHelper.VecSetDiff(_grads,0, _pos,id0, _pos,id1);
				var len = (float)Math.Sqrt(VectorHelper.VecLengthSquared(_grads,0));
				if (len == 0.0)
					continue;
				VectorHelper.VecScale(_grads,0, 1.0f / len);
				var restLen = _edgeLengths[i];
				var C = len - restLen;
				var s = -C / (w + alpha);
				VectorHelper.VecAdd(_pos,id0, _grads,0, s * w0);
				VectorHelper.VecAdd(_pos,id1, _grads,0, -s * w1);
			}
		}
		private void SolveVolumes(float compliance,float dt)
		{
			var alpha = compliance / dt /dt;

			for (var i = 0; i < _numTets; i++) {
				var w = 0.0;
						
				for (var j = 0; j < 4; j++) {
					var id0 = _tetIds[4 * i + _volIdOrder[j][0]];
					var id1 = _tetIds[4 * i + _volIdOrder[j][1]];
					var id2 = _tetIds[4 * i + _volIdOrder[j][2]];

					VectorHelper.VecSetDiff(_temp,0, _pos,id1, _pos,id0);
					VectorHelper.VecSetDiff(_temp,1, _pos,id2, _pos,id0);
					VectorHelper.VecSetCross(_grads,j, _temp,0, _temp,1);
					VectorHelper.VecScale(_grads,j, 1.0f/6.0f);

					w += _invMass[_tetIds[4 * i + j]] * VectorHelper.VecLengthSquared(_grads,j);
				}
				if (w == 0.0)
					continue;

				var vol = GetTetVolume(i);
				var restVol = _restVol[i];
				var C = vol - restVol;
				var s = -C / (w + alpha);

				for (var j = 0; j < 4; j++) {
					var id = _tetIds[4 * i + j];
					VectorHelper.VecAdd(_pos, id, _grads, j, (float)s * _invMass[id]);
				}
			}
		}

    
	}
	
}