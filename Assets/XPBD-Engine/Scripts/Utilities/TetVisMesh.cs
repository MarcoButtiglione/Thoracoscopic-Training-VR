namespace XPBD_Engine.Scripts.Utilities
{
    [System.Serializable]
    public class TetVisMesh
    {
        public string name;
        public float[] verts;
        public float[] uvs;
        public int[] tetIds;
        public int[] tetEdgeIds;
        public int[] tetSurfaceTriIds;
        public int[] tetSurfaceTriUvIds;
        public int[] tetSurfaceVertexUvIds;
        public int[] vertexUvList;
    }
}