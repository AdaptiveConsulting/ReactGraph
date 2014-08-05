using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactGraph.Graph
{
    internal class DirectedGraph<T>
    {
        private readonly Dictionary<T, Vertex<T>> verticies = new Dictionary<T, Vertex<T>>();
        long vertexIdCounter;

        public int VerticiesCount
        {
            get { return verticies.Count; }
        }

        public int EdgesCount
        {
            get { return verticies.Values.Sum(v => v.Successors.Count()); }
        }

        public Edge<T> AddEdge(T source, T target, string sourceId, string targetId)
        {
            var sourceVertex = AddVertex(source, sourceId);
            var targetVertex = AddVertex(target, targetId);

            var edge = sourceVertex.Successors.FirstOrDefault(e => e.Target == targetVertex);

            if (edge != null)
            {
                return edge;
            }
            return sourceVertex.AddSuccessorEdge(targetVertex);
        }

        private void RemoveEdge(Edge<T> edge)
        {
            edge.Source.RemoveSuccessorEdge(edge);
        }

        public IEnumerable<Edge<T>> Edges
        {
            get { return verticies.Values.SelectMany(vertex => vertex.Successors); }
        }

        public IEnumerable<Vertex<T>> Verticies
        {
            get { return verticies.Values; }
        }

        /// <summary>
        /// Perform a depth first seach
        /// <see cref="http://en.wikipedia.org/wiki/Depth-first_search"/>
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public IList<Vertex<T>> DepthFirstSearch(T origin)
        {
            var stack = new Stack<Vertex<T>>();
            stack.Push(verticies[origin]);
            var result = new List<Vertex<T>>();

            while (stack.Count > 0)
            {
                var vertex = stack.Pop();
                if (!result.Contains(vertex)) // that's slow, we might want to have a hashmap for quicker check
                {
                    result.Add(vertex);
                    foreach (var edge in vertex.Successors)
                    {
                        stack.Push(edge.Target);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Search for source verticies (roots, ie. vertex which have no incoming edges)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vertex<T>> FindSources()
        {
            var perVertexCount = new Dictionary<Vertex<T>, int>();
            foreach (var vertex in verticies.Values)
            {
                perVertexCount[vertex] = 0;
            }

            foreach (var edge in Edges)
            {
                perVertexCount[edge.Target]++;
            }

            return perVertexCount.Where(kvp => kvp.Value == 0).Select(kvp => kvp.Key);
        }  

        public DirectedGraph<T> SubGraph(T origin)
        {
            var dfs = DepthFirstSearch(origin);

            var graph = new DirectedGraph<T>();

            foreach (var vertex in dfs)
            {
                foreach (var edge in vertex.Successors)
                {
                    if (dfs.Contains(edge.Target))
                    {
                        graph.AddEdge(vertex.Data, edge.Target.Data, vertex.Id, edge.Target.Id);
                    }
                }
            }

            return graph;
        } 

        /// <summary>
        /// <see cref="http://en.wikipedia.org/wiki/Topological_sorting"/>
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public IEnumerable<Vertex<T>> TopologicalSort(T origin)
        {
            var subGraph = SubGraph(origin);
            var result = new List<Vertex<T>>();
            var sources = new Stack<Vertex<T>>(subGraph.FindSources());

            while (sources.Count > 0)
            {
                var vertex = sources.Pop();
                result.Add(vertex);
                foreach (var edge in vertex.Successors.ToList())
                {
                    subGraph.RemoveEdge(edge);
                    if (!edge.Target.Predecessors.Any())
                    {
                        sources.Push(edge.Target);
                    }
                }
            }

            if (subGraph.EdgesCount > 0)
            {
                throw new InvalidOperationException("Graph contains at least one cycle.");
            }

            return result;
        }

        private Vertex<T> AddVertex(T data, string id)
        {
            if (!verticies.ContainsKey(data))
            {
                var newVertexId = id;
                if (string.IsNullOrEmpty(newVertexId))
                {
                    vertexIdCounter++;
                    newVertexId = "__" + vertexIdCounter;
                }

                var vertex = new Vertex<T>(data, newVertexId);
                verticies[data] = vertex;
            }
            return verticies[data];
        }

        /// <summary>
        /// <see cref="http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm"/>
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEnumerable<Vertex<T>>> DetectCyles()
        {
            var index = 0;
            var stack = new Stack<Vertex<T>>();
            var indexes = new Dictionary<Vertex<T>, int>();
            var lowlinks = new Dictionary<Vertex<T>, int>();
            var result = new List<List<Vertex<T>>>();

            foreach (var vertex in verticies.Values)
            {
                if (!indexes.ContainsKey(vertex))
                {
                    StrongConnect(vertex, ref index, stack, indexes, lowlinks, result);
                }
            }
            return result;
        }

        private static void StrongConnect(Vertex<T> v, ref int index, Stack<Vertex<T>> stack, Dictionary<Vertex<T>, int> indexes, Dictionary<Vertex<T>, int> lowlinks, List<List<Vertex<T>>> result)
        {
            // Set the depth index for vertex to the smallest unused index
            indexes[v] = index;
            lowlinks[v] = index;
            index++;
            stack.Push(v);

            // Consider successors of vertex
            foreach (var successor in v.Successors)
            {
                var w = successor.Target;
                if (!indexes.ContainsKey(w))
                {
                    // Successor w has not yet been visited; recurse on it
                    StrongConnect(w, ref index, stack, indexes, lowlinks, result);
                    lowlinks[v] = Math.Min(lowlinks[v], lowlinks[w]);
                }
                else if (stack.Contains(w))
                {
                    // Successor w is in stack S and hence in the current SCC
                    lowlinks[v] = Math.Min(lowlinks[v], indexes[w]);
                }
            }

            // If v is a root node, pop the stack and generate an SCC
            if (lowlinks[v] == indexes[v])
            {
                var scc = new List<Vertex<T>>();
                Vertex<T> w;
                do
                {
                    w = stack.Pop();
                    scc.Add(w);
                } 
                while (w != v);
                
                if (scc.Count > 1) result.Add(scc);
            }
        }
    }
}
