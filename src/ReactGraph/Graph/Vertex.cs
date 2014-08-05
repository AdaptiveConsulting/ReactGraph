using System.Collections.Generic;

namespace ReactGraph.Graph
{
    internal class Vertex<T>
    {
        private readonly List<Edge<T>> predecessors = new List<Edge<T>>();
        private readonly List<Edge<T>> successors = new List<Edge<T>>();

        public Vertex(T data, string id)
        {
            Data = data;
            Id = id;
        }

        public T Data { get; private set; }

        public string Id { get; private set; }

        public IEnumerable<Edge<T>> Predecessors { get { return predecessors; } } 

        public IEnumerable<Edge<T>> Successors { get { return successors; } }

        public Edge<T> AddSuccessorEdge(Vertex<T> targetVertex)
        {
            var edge = new Edge<T>(this, targetVertex);
            successors.Add(edge);
            targetVertex.AddPredecessor(edge);
            return edge;
        }

        public void RemoveSuccessorEdge(Edge<T> edge)
        {
            successors.Remove(edge);
            edge.Target.RemovePredecessor(edge);
        }

        private void AddPredecessor(Edge<T> edge)
        {
            predecessors.Add(edge);
        }

        private void RemovePredecessor(Edge<T> edge)
        {
            predecessors.Remove(edge);
        }
    }
}