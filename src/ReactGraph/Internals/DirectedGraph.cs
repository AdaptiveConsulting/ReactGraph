using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactGraph.Internals
{
    internal class DirectedGraph<T>
    {
        private readonly Dictionary<T, Vertex<T>> _verticies = new Dictionary<T, Vertex<T>>();

        public int VerticiesCount
        {
            get { return _verticies.Count; }
        }

        public int EdgesCount
        {
            get { return _verticies.Values.Sum(v => v.Successors.Count()); }
        }

        public void AddEdge(T source, T target)
        {
            var sourceVertex = AddVertex(source);
            var targetVertex = AddVertex(target);

            sourceVertex.AddSuccessorEdge(targetVertex);
        }

        private void RemoveEdge(Edge<T> edge)
        {
            edge.Source.RemoveSuccessorEdge(edge);
        }

        private IEnumerable<Edge<T>> Edges
        {
            get { return _verticies.Values.SelectMany(vertex => vertex.Successors); }
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
            stack.Push(_verticies[origin]);
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

        public IEnumerable<Vertex<T>> Verticies
        {
            get { return _verticies.Values; }
        }

        /// <summary>
        /// Search for source verticies (roots, ie. vertex which have no incoming edges)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vertex<T>> FindSources()
        {
            var perVertexCount = new Dictionary<Vertex<T>, int>();
            foreach (var vertex in _verticies.Values)
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
                        graph.AddEdge(vertex.Data, edge.Target.Data);
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

        private Vertex<T> AddVertex(T data)
        {
            if (!_verticies.ContainsKey(data))
            {
                var vertex = new Vertex<T>(data);
                _verticies[data] = vertex;
            }
            return _verticies[data];
        }

        public string ToDotLanguage(string title)
        {
            var sb = new StringBuilder();

            sb.AppendFormat("digraph {0} {{", title).AppendLine();

            foreach (var edge in Edges)
            {
                sb.AppendFormat("     {0} -> {1};", edge.Source.Data, edge.Target.Data).AppendLine();
            }

            sb.Append("}");

            return sb.ToString();
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

            foreach (var vertex in _verticies.Values)
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
