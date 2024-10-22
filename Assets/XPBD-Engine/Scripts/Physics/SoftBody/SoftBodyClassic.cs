using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPBD_Engine.Scripts.Physics.Grabber.Interfaces;
using XPBD_Engine.Scripts.Utilities;
using XPBD_Engine.Scripts.Utilities.Data_structures;

namespace XPBD_Engine.Scripts.Physics.SoftBody
{
	//Same as SoftBodySimulation but is using Vector3 instead of arrays where an index in the array is x, y, or z 
	//This makes the code simpler to read buy maye a little slower according to the guy in the video, but I don't notice much difference...
	[RequireComponent(typeof(MeshFilter))]
	public class SoftBodyClassic : MonoBehaviour,IGrabbable
	{
		#region Variables
		//Tetrahedralizer data structures
		public TextAsset modelJson;
		public TetVisMesh tetVisMesh;
		//Min 0.25
		public float meshScale = 1f;
		public Vector3 meshRotation;
		[Range(0f,500f)]public float edgeCompliance= 100.0f;
		[Range(0f,500f)]public float volCompliance = 0.0f;
		public List<StaticSide> staticSides;

		public List<SphericalAnchor> sphericalAnchors;
		//------------------------------
		private Vector3 _initPosition;
		//The Unity mesh to display the soft body mesh
		private Mesh _mesh;
		//Physics variables
		private Vector3[] _pos; 
		private Vector3[] _prevPos;
		private Vector3[] _vel;
		//Ids of the data structures
		private int[] _tetIds;
		private int[] _edgeIds;
		//Number of vertices and tets
		private int _numParticles;
		private int _numTets;
		//For soft body physics using tetrahedrons
		//The volume of each undeformed tetrahedron
		private float[] _restVolumes;
		//The length of an undeformed tetrahedron edge
		private float[] _restEdgeLengths;
		//Inverse mass w = 1/m where m is how much mass is connected to a particle
		//If a particle is fixed we set its mass to 0
		private float[] _invMass;
		//Soft body behavior settings
		//Compliance (alpha) is the inverse of physical stiffness (k)
		//alpha = 0 means infinitely stiff (hard)
		private float _edgeCompliance;
		//Should be 0 or the mesh becomes very flat even for small values 
		private float _volCompliance;
		//Temp variable
		private Vector3[] _temp;
		//Should be global so we don't have to create them a million times
		private Vector3[] _gradients;
		//Environment collision data 
		
		private int[][] _volIdOrder=  { 
			new [] { 1, 3, 2 }, 
			new [] { 0, 2, 3 }, 
			new [] { 0, 3, 1 }, 
			new [] { 0, 1, 2 } 
		};
		//Grabbing with mouse to move mesh around
		//The id of the particle we grabed with mouse
		//private int _grabId;
		//We grab a single particle and then we sit its inverted mass to 0. When we ungrab we have to reset its inverted mass to what itb was before 
		//private float _grabInvMass;

		private List<GrabbedVertex> _grabbedVertices;
		
		#endregion

		#region MonoBehaviour
		void Start()
		{
			StartSoftBody();
			InitMesh();
		}

		private void StartSoftBody()
		{
			tetVisMesh = JsonUtility.FromJson<TetVisMesh>(modelJson.text);
			_numParticles = tetVisMesh.verts.Length / 3;
			_numTets = tetVisMesh.tetIds.Length / 4;
			
			_pos = GetPosFromTetMesh(tetVisMesh.verts);
			//Particle previous position
			//Not needed because is already set to 0s
			_prevPos = new Vector3[_numParticles];
			//Particle velocity
			//Not needed because is already set to 0s
			_vel = new Vector3[_numParticles];

			_tetIds = tetVisMesh.tetIds;
			_edgeIds = tetVisMesh.tetEdgeIds;
			_restVolumes = new float[_numTets];
			_restEdgeLengths = new float[_edgeIds.Length / 2];	
			_invMass = new float[_numParticles];
		
			_edgeCompliance = edgeCompliance;
			_volCompliance = volCompliance;

			_temp = new Vector3[4];
			_gradients = new Vector3[4];
		

			InitPhysics();

			_initPosition = gameObject.transform.position;
			Translate(_initPosition);
			gameObject.transform.position = Vector3.zero;

			
		
			//_grabId = -1; 
			//_grabInvMass = 0.0f;
			_grabbedVertices = new List<GrabbedVertex>();
			_volIdOrder = new[]
			{
				new[] { 1, 3, 2 },
				new[] { 0, 2, 3 },
				new[] { 0, 3, 1 },
				new[] { 0, 1, 2 }
			};
		}
		
		private void OnDestroy()
		{
			Destroy(_mesh);
		}
		private void OnDrawGizmos()
		{
			if (Application.isPlaying) return;
			if(modelJson==null) return;
			var tetVisMesh = JsonUtility.FromJson<TetVisMesh>(modelJson.text);
			var pos = GetPosFromTetMesh(tetVisMesh.verts);
			// Draw a yellow sphere at the transform's position
			Gizmos.DrawMesh(GetMesh(tetVisMesh,pos),transform.position);
		}

		private Vector3[] GetPosFromTetMesh(float[] vertices)
		{
			var numParticles = vertices.Length / 3;
			var pos = new Vector3[numParticles];
			Quaternion rotation = Quaternion.Euler(meshRotation);
			for (var i = 0; i < numParticles; i++)
			{
				var a = i * 3;
				var tempVect = new Vector3(vertices[a], vertices[a + 1], vertices[a + 2])*meshScale;
				tempVect = rotation *tempVect;
				pos[i] = tempVect;
			}
			return pos; 
		}
		#endregion
		
		#region Mesh
		//
		// Unity mesh 
		//
		//Init the mesh when the simulation is started
		private void InitMesh()
		{
			_mesh = GetMesh(tetVisMesh,_pos);
			GetComponent<MeshFilter>().mesh = _mesh;
		}

		private Mesh GetMesh(TetVisMesh tetMesh, IReadOnlyList<Vector3> vertexPositions)
		{
			var mesh = new Mesh();
			mesh.Clear();

			var vertices = new Vector3[tetMesh.vertexUvList.Length/2];
			var uvs = new Vector2[tetMesh.vertexUvList.Length/2];
			var triangles = tetMesh.tetSurfaceVertexUvIds;

			var meshUvs = new Vector2[tetMesh.uvs.Length / 2];
			for (int i = 0; i < tetMesh.uvs.Length/2; i++)
			{
				meshUvs[i] = new Vector2(tetMesh.uvs[2 * i], tetMesh.uvs[2 * i + 1]);
			}
			for (int i = 0; i < tetMesh.vertexUvList.Length/2; i++)
			{
				vertices[i] = vertexPositions[tetMesh.vertexUvList[2 * i + 0]];
				uvs[i] = meshUvs[tetMesh.vertexUvList[2 * i + 1]];
			}
			mesh.vertices = vertices;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			return mesh;
		}
		private void UpdateMeshes() 
		{
			var vertices = new Vector3[tetVisMesh.vertexUvList.Length/2];
			for (int i = 0; i < tetVisMesh.vertexUvList.Length/2; i++)
			{
				vertices[i] = _pos[tetVisMesh.vertexUvList[2 * i + 0]];
			}
			_mesh.vertices = vertices;
			_mesh.RecalculateBounds();
			_mesh.RecalculateNormals();
		}
		#endregion
		
		#region Physics
		private void InitPhysics()
		{
			for (var i = 0; i < _numTets; i++) {
				var vol =GetTetVolume(i);
				_restVolumes[i] = vol;
				var pInvMass = vol > 0.0f ? 1.0f / (vol / 4.0f) : 0.0f;
				_invMass[_tetIds[4 * i]] += pInvMass;
				_invMass[_tetIds[4 * i + 1]] += pInvMass;
				_invMass[_tetIds[4 * i + 2]] += pInvMass;
				_invMass[_tetIds[4 * i + 3]] += pInvMass;
			}
			for (var i = 0; i < _restEdgeLengths.Length; i++) {
				var id0 = _edgeIds[2 * i];
				var id1 = _edgeIds[2 * i + 1];
				_restEdgeLengths[i] = Vector3.Distance(_pos[id0], _pos[id1]);
			}

			if (staticSides.Count>0)
			{
				SetStatic();
			}

			foreach (var anchor in sphericalAnchors)
			{
				for (var index = 0; index < _pos.Length; index++)
				{
					var vertex = _pos[index];
					if (Vector3.Distance(anchor.transform.position, vertex+transform.position) <= anchor.radius)
					{
						SetVertexStatic(index);
					}
				}
			}
		}

		private void SetVertexStatic(int index)
		{
			_invMass[index] = 0f;
		}
		private bool IsVertexStatic(int index)
		{
			return _invMass[index] == 0f;
		}
		private void SetStatic()
		{
			var posOrderByDescendingX = _pos.OrderByDescending(v => v.x);
			var posOrderByDescendingY = _pos.OrderByDescending(v => v.y);
			var posOrderByDescendingZ = _pos.OrderByDescending(v => v.z);

			var minX = posOrderByDescendingX.Last().x;
			var maxX = posOrderByDescendingX.First().x;
			var minY = posOrderByDescendingY.Last().y;
			var maxY = posOrderByDescendingY.First().y;
			var minZ = posOrderByDescendingZ.Last().z;
			var maxZ = posOrderByDescendingZ.First().z;

			for (int i = 0; i < _numParticles; i++)
			{
				var toSetStatic = false;
				foreach (var staticSide in staticSides)
				{
					if(toSetStatic) continue;
					switch (staticSide.axis)
					{
						case StaticAxis.X:
							if (staticSide.isPositiveSide)
							{
								if (_pos[i].x>maxX - staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							else
							{
								if (_pos[i].x<minX + staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							break;
						case StaticAxis.Y:
							if (staticSide.isPositiveSide)
							{
								if (_pos[i].y>maxY - staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							else
							{
								if (_pos[i].y<minY + staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							break;
						case StaticAxis.Z:
							if (staticSide.isPositiveSide)
							{
								if (_pos[i].z>maxZ - staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							else
							{
								if (_pos[i].z<minZ + staticSide.amount)
								{
									toSetStatic = true;
								}
							}
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				if(toSetStatic) SetVertexStatic(i);
			}
		}
		
		//Move the particles and handle environment collision
		public void PreSolve(float dt, Vector3 gravity, WorldBoundType boundType,Vector3 worldBoundCenter,Vector3 worldBoundSize,float worldSphereRadius,Vector3 worldCapsulePos1,Vector3 worldCapsulePos2)
		{
			//For each particle
			for (var i = 0; i < _numParticles; i++) {
				//This means the particle is fixed, so don't simulate it
				if (IsVertexStatic(i))
				{
					EnvironmentCollision(boundType,i,worldBoundSize,worldBoundCenter,worldSphereRadius,worldCapsulePos1,worldCapsulePos2);
					continue;
				}
				//Update vel
				_vel[i] += gravity * dt;
				//Save old pos
				_prevPos[i] = _pos[i];
				//Update pos
				_pos[i] += _vel[i] * dt;
				EnvironmentCollision(boundType,i,worldBoundSize,worldBoundCenter,worldSphereRadius,worldCapsulePos1,worldCapsulePos2);
			}
		}
		//Collision with invisible walls and floor
		private void EnvironmentCollision(WorldBoundType boundType,int i,Vector3 worldBoundSize,Vector3 worldBoundCenter,float worldSphereRadius,Vector3 worldCapsulePos1,Vector3 worldCapsulePos2)
		{
			switch(boundType) {
				case WorldBoundType.None:
					break;
				case WorldBoundType.Cube:
					var worldBoxMin = worldBoundCenter - worldBoundSize / 2.0f;
					var worldBoxMax = worldBoundCenter + worldBoundSize / 2.0f;
					HandleBoxWorldCollision(i,worldBoxMax,worldBoxMin);
					break;
				case WorldBoundType.Sphere:
					HandleSphereWorldCollision(i, worldBoundCenter, worldSphereRadius);
					break;
				case WorldBoundType.Capsule:
					HandleCapsuleWorldCollision(i, worldCapsulePos1,worldCapsulePos2, worldSphereRadius);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(boundType), boundType, null);
			}

		}
		private void HandleCapsuleWorldCollision(int i,Vector3 capsulePos1,Vector3 capsulePos2,float capsuleRadius)
		{
			var closestPoint = Intersections.GetClosestPointOnSegment(_pos[i], capsulePos1, capsulePos2);
			var distance = Vector3.Distance(_pos[i], closestPoint);
			if (distance>capsuleRadius)
			{
				_pos[i] = _prevPos[i];
				var dir = _pos[i] - closestPoint;
				dir.Normalize();

				_pos[i] = closestPoint + dir * capsuleRadius;
			}
		}
		private void HandleBoxWorldCollision(int i,Vector3 worldBoxMax,Vector3 worldBoxMin)
		{
			//Floor collision
			var x = _pos[i].x;
			var y = _pos[i].y;
			var z = _pos[i].z;

			//X
			if (x < worldBoxMin.x)
			{
				_pos[i] = _prevPos[i];
				_pos[i].x = worldBoxMin.x;
			}
			else if (x > worldBoxMax.x)
			{
				_pos[i] = _prevPos[i];
				_pos[i].x = worldBoxMax.x;
			}

			//Y
			if (y < worldBoxMin.y)
			{
				_pos[i] = _prevPos[i];
				_pos[i].y = worldBoxMin.y;
			}
			else if (y > worldBoxMax.y)
			{
				_pos[i] = _prevPos[i];
				_pos[i].y = worldBoxMax.y;
			}

			//Z
			if (z < worldBoxMin.z)
			{
				_pos[i] = _prevPos[i];
				_pos[i].z = worldBoxMin.z;
			}
			else if (z > worldBoxMax.z)
			{
				_pos[i] = _prevPos[i];
				_pos[i].z = worldBoxMax.z;
			}
		}
		private void HandleSphereWorldCollision(int i,Vector3 worldBoundCenter,float worldSphereRadius)
		{
			var distance = Vector3.Distance(worldBoundCenter, _pos[i]);

			if (distance>worldSphereRadius)
			{
				_pos[i] = _prevPos[i];
				var dir = _pos[i] - worldBoundCenter;
				dir.Normalize();

				_pos[i] = worldBoundCenter + dir * worldSphereRadius;
			}

		}

		//Handle the soft body physics
		public void Solve(float dt)
		{
			//Constraints
			//Enforce constraints by moving each vertex: x = x + deltaX
			//- Correction vector: deltaX = lambda * w * gradC
			//- Inverse mass: w
			//- lambda = -C / (w1 * |grad_C1|^2 + w2 * |grad_C2|^2 + ... + wn * |grad_C|^2 + (alpha / dt^2)) where 1, 2, ... n is the number of participating particles in the constraint.
			//		- n = 2 if we have an edge, n = 4 if we have a tetra
			//		- |grad_C1|^2 is the squared length
			//		- (alpha / dt^2) is what makes the costraint soft. Remove it and you get a hard constraint
			//- Compliance (inverse stiffness): alpha 
			SolveEdges(edgeCompliance, dt);
			SolveVolumes(volCompliance, dt);
		}
		//Fix velocity
		public void PostSolve(float dt)
		{
			var oneOverDt = 1f / dt;
			//For each particle
			for (var i = 0; i < _numParticles; i++) {
				if (IsVertexStatic(i))
					continue;
				_vel[i] = (_pos[i] - _prevPos[i]) * oneOverDt;
			}
			UpdateMeshes();
		}
		//Solve distance constraint
		//2 particles:
		//Positions: x0, x1
		//Inverse mass: w0, w1
		//Rest length: l_rest
		//Current length: l
		//Constraint function: C = l - l_rest which is 0 when the constraint is fulfilled 
		//Gradients of constraint function grad_C0 = (x1 - x0) / |x1 - x0| and grad_C1 = -grad_C0
		//Which was shown here https://www.youtube.com/watch?v=jrociOAYqxA (12:10)
		private void SolveEdges(float compliance,float dt)
		{
			var alpha = compliance / dt /dt;
			//For each edge
			for (var i = 0; i < _restEdgeLengths.Length; i++) {
				//2 vertices per edge in the data structure, so multiply by 2 to get the correct vertex index
				var id0 = _edgeIds[2 * i];
				var id1 = _edgeIds[2 * i + 1];
				var w0 = _invMass[id0];
				var w1 = _invMass[id1];
				var w = w0 + w1;
				//This edge is fixed so dont simulate
				if (w == 0.0)
					continue;
				//The current length of the edge l
				//x0-x1
				//The result is stored in grads array
				_gradients[0] = _pos[id0] - _pos[id1];
				//sqrMargnitude(x0-x1)
				var len = Vector3.Magnitude(_gradients[0]);
				//If they are at the same pos we get a divisio by 0 later so ignore
				if (len == 0.0)
					continue;
				//(xo-x1) * (1/|x0-x1|) = gradC
				_gradients[0] *= 1.0f / len;
				var restLen = _restEdgeLengths[i];
				var C = len - restLen;
				//lambda because |grad_Cn|^2 = 1 because if we move a particle 1 unit, the distance between the particles also grows with 1 unit, and w = w0 + w1
				var s = -C / (w + alpha);
				//Move the vertices x = x + deltaX where deltaX = lambda * w * gradC
				_pos[id0] += _gradients[0] * (s * w0);
				_pos[id1] += _gradients[0] * (-s * w1);

			}
		}
		//Solve volume constraint
		//Constraint function is now defined as C = 6(V - V_rest). The 6 is to make the equation simpler because of volume
		//4 gradients:
		//grad_C1 = (x4-x2)x(x3-x2) <- direction perpendicular to the triangle opposite of p1 to maximally increase the volume when moving p1
		//grad_C2 = (x3-x1)x(x4-x1)
		//grad_C3 = (x4-x1)x(x2-x1)
		//grad_C4 = (x2-x1)x(x3-x1)
		//V = 1/6 * ((x2-x1)x(x3-x1))*(x4-x1)
		//lambda =  6(V - V_rest) / (w1 * |grad_C1|^2 + w2 * |grad_C2|^2 + w3 * |grad_C3|^2 + w4 * |grad_C4|^2 + alpha/dt^2)
		//delta_xi = -lambda * w_i * grad_Ci
		//Which was shown here https://www.youtube.com/watch?v=jrociOAYqxA (13:50)
		private void SolveVolumes(float compliance,float dt)
		{
			var alpha = compliance / dt /dt;
			//For each tetra
			for (var i = 0; i < _numTets; i++) {
				float w = 0.0f;
				//Foreach vertex in the tetra
				for (var j = 0; j < 4; j++) {
					//The 3 opposite vertices ids
					var id0 = _tetIds[4 * i + _volIdOrder[j][0]];
					var id1 = _tetIds[4 * i + _volIdOrder[j][1]];
					var id2 = _tetIds[4 * i + _volIdOrder[j][2]];
					//(x4 - x2)
					_temp[0] = _pos[id1] - _pos[id0];
					//(x3 - x2)
					_temp[1] = _pos[id2] - _pos[id0];
					//(x4 - x2)x(x3 - x2)
					_gradients[j] = Vector3.Cross(_temp[0], _temp[1]);
					_gradients[j] *= 1.0f / 6.0f;
					//w1 * |grad_C1|^2
					w += _invMass[_tetIds[4 * i + j]] * Vector3.SqrMagnitude(_gradients[j]);
				}
				//All vertices are fixed so dont simulate
				if (w == 0.0)
					continue;

				var vol = GetTetVolume(i);
				var restVol = _restVolumes[i];
				var C = vol - restVol;
				var s = -C / (w + alpha);
				//Move each vertex
				for (var j = 0; j < 4; j++) {
					var id = _tetIds[4 * i + j];
					//Move the vertices x = x + deltaX where deltaX = lambda * w * gradC
					_pos[id] += _gradients[j] * (s * _invMass[id]);
				}
			}
	    
		}
		//Squash the mesh so it becomes flat against the ground
		#endregion

		#region Utilities
		//Calculate the volume of a tetrahedron
		private float GetTetVolume(int nr) 
		{
			//The 4 vertices belonging to this tetra 
			var id0 = _tetIds[4 * nr];
			var id1 = _tetIds[4 * nr + 1];
			var id2 = _tetIds[4 * nr + 2];
			var id3 = _tetIds[4 * nr + 3];
			var volume = Tetrahedron.Volume(_pos[id0], _pos[id1], _pos[id2], _pos[id3]);

			return volume;
		}
		public void Translate(Vector3 translation)
		{
			for (var i = 0; i < _numParticles; i++)
			{
				_pos[i] += translation;
				_prevPos[i] += translation;
			}
		}
		public void Squeeze()
		{
			for (int i = 0; i < _numParticles; i++)
			{
				//Set y coordinate to slightly above floor height
				_pos[i].y =  1f;
			}

			UpdateMeshes();
		}

		public void RestartSoftBody()
		{
			gameObject.transform.position=_initPosition ;
			StartSoftBody();
		}
		#endregion
		
		#region Grabber
		//Input pos is the pos in a triangle we get when doing ray-triangle intersection
		public int StartGrab(Vector3 triangleIntersectionPos)
		{
			//Find the closest vertex to the pos on a triangle in the mesh
			float minD2 = float.MaxValue;
			int index=-1;
			for (int i = 0; i < _numParticles; i++)
			{
				float d2 = Vector3.SqrMagnitude(triangleIntersectionPos - _pos[i]);
				if (d2 < minD2)
				{
					minD2 = d2;
					index = i;
				}
			}
			if(index==-1)
				return -1;
			_grabbedVertices.Add(new GrabbedVertex(index,_invMass[index]));
			//Set the inverted mass to 0 to mark it as fixed
			_invMass[index] = 0f;
			//Set the position of the vertex to the position where the ray hit the triangle
			_pos[index] = triangleIntersectionPos;
			return index;
		}

		public void StartGrabVertex(Vector3 grabPos, int vertexIndex)
		{
			if (_grabbedVertices.Exists(item => item.index == vertexIndex)) return;
			_grabbedVertices.Add(new GrabbedVertex(vertexIndex,_invMass[vertexIndex]));
			_invMass[vertexIndex] = 0f;
			_pos[vertexIndex] = grabPos;
		}

		public void MoveGrabbed(Vector3 newPos,int index)
		{
			if (!_grabbedVertices.Exists(item => item.index ==index)) return;
			_pos[index] = newPos;
		}
		public void EndGrab(Vector3 grabPos, Vector3 newParticleVel,int index)
		{
			if (!_grabbedVertices.Exists(item => item.index == index)) return;
			
			GrabbedVertex foundItem = _grabbedVertices.Find(item => item.index == index);
				//Set the mass to whatever mass it was before we grabbed it
			_invMass[index] = foundItem.invMass;
			_vel[index] = newParticleVel;
			_grabbedVertices.Remove(foundItem);


		}
		public void IsRayHittingBody(Ray ray, out PointerHit hit)
		{
			//Mesh data
			Vector3[] vertices = _pos;

			int[] triangles = tetVisMesh.tetSurfaceTriIds;

			//Find if the ray hit a triangle in the mesh
			Intersections.IsRayHittingMesh(ray, vertices,_invMass, triangles, out hit);
		}

		public bool IsSphereInsideBody(Vector3 center, float radius, out SphereHit bestVertex)
		{
			var vertices = _pos;
			bestVertex = null;
			var smallestDistance = float.MaxValue;
			for (var i = 0; i < vertices.Length; i++)
			{
				var vertex = vertices[i];
				if (!Intersections.IsVertexIntoSphere(vertex, center, radius, out var distance) || IsVertexStatic(i)) continue;
				if (!(distance < smallestDistance)) continue;
				smallestDistance =distance;
				bestVertex = new SphereHit(distance, vertex, i);
			}
			return bestVertex != null;
		}


		public Vector3 GetGrabbedPos(int index)
		{
			return _pos[index];
		}
		#endregion

		public Vector3 GetVertexPos(int index)
		{
			return _pos[index];
		}
		public Vector3 GetVertexNorm(int index)
		{
			return _mesh.normals[index];
		}
	}
}

[Serializable]
public enum StaticAxis
{
	X,
	Y,
	Z
}

[Serializable]
public class StaticSide
{
	public StaticAxis axis;
	public bool isPositiveSide;
	[Range(0f,3f)] public float amount;
}



