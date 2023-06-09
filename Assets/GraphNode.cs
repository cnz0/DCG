public class GraphNode {

    public GraphNode() { }

    public GraphNode(float distance, bool in_mst)
    {
        dist = distance;
        mst = in_mst;
    }
    public float dist { get; set; }
    public bool mst { get; set; }

    public override string ToString() {
        return string.Format("DIST", dist);
    }
 }