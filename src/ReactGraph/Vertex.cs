using System.Collections.Generic;

namespace ReactGraph
{
    public class Vertex<T>
    {
        private readonly List<Edge<T>> _predecessors = new List<Edge<T>>();
        private readonly List<Edge<T>> _successors = new List<Edge<T>>();

        public Vertex(T data)
        {
            Data = data;
        }

        public T Data { get; set; }

        public IEnumerable<Edge<T>> Predecessors { get { return _predecessors; } } 

        public IEnumerable<Edge<T>> Successors { get { return _successors; } }

        public void AddSuccessorEdge(Vertex<T> targetVertex)
        {
            var edge = new Edge<T>(this, targetVertex);
            _successors.Add(edge);
            targetVertex.AddPredecessor(edge);
        }

        public void RemoveSuccessorEdge(Edge<T> edge)
        {
            _successors.Remove(edge);
            edge.Target.RemovePredecessor(edge);
        }

        private void AddPredecessor(Edge<T> edge)
        {
            _predecessors.Add(edge);
        }

        private void RemovePredecessor(Edge<T> edge)
        {
            _predecessors.Remove(edge);
        }
    }
}