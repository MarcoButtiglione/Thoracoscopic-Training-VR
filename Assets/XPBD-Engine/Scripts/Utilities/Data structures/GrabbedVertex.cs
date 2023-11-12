namespace XPBD_Engine.Scripts.Utilities.Data_structures
{
    public struct GrabbedVertex
    {
        public int index;
        public float invMass;

        public GrabbedVertex(int index, float invMass)
        {
            this.index = index;
            this.invMass = invMass;
        }
    }
}