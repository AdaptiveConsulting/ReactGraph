namespace ReactGraph.Graph
{
    internal class Edge<T>
    {
        public Vertex<T> Target { get; private set; }
        public Vertex<T> Source { get; private set; }

        public Edge(Vertex<T> source, Vertex<T> target)
        {
            Source = source;
            Target = target;
        }
    }
}