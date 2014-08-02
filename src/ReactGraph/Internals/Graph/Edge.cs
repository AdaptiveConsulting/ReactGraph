namespace ReactGraph.Internals.Graph
{
    internal class Edge<T>
    {
        public Vertex<T> Target { get; set; }
        public Vertex<T> Source { get; set; }

        public Edge(Vertex<T> source, Vertex<T> target)
        {
            Source = source;
            Target = target;
        }
    }
}