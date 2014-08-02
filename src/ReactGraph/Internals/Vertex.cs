using System.Collections.Generic;

namespace ReactGraph.Internals
{
    internal class Vertex<T>
    {
        private readonly List<Edge<T>> predecessors = new List<Edge<T>>();
        private readonly List<Edge<T>> successors = new List<Edge<T>>();

        public Vertex(T data)
        {
            Data = data;
        }

        public T Data { get; set; }

        public IEnumerable<Edge<T>> Predecessors { get { return predecessors; } } 

        public IEnumerable<Edge<T>> Successors { get { return successors; } }

        public void AddSuccessorEdge(Vertex<T> targetVertex)
        {
            var edge = new Edge<T>(this, targetVertex);
            successors.Add(edge);
            targetVertex.AddPredecessor(edge);
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