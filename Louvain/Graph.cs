using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LouvainCommunityPL {
    /// <summary>
    /// Represents an undirected graph.
    /// 
    /// Written from scratch by Kyle Miller (v-kymil@microsoft.com) February 2014
    /// </summary>
    public class Graph {

        /// <summary>
        /// Represents a weighted edge between nodes.
        /// </summary>
        public struct Edge {
            public int FromNode;
            public int ToNode;
            public double Weight;

            /// <summary>
            /// Constructs a weighted edge between two nodes.
            /// </summary>
            /// <param name="n1">The first node.</param>
            /// <param name="n2">The second node.</param>
            /// <param name="w">The edge's weight.</param>
            public Edge(int n1, int n2, double w) {
                FromNode = n1;
                ToNode = n2;
                Weight = w;
            }

            /// <summary>
            /// True iff the two nodes of the edge are the same.
            /// </summary>
            public bool SelfLoop { get { return FromNode == ToNode; } }
        }

        private Dictionary<int, Dictionary<int, double>> AdjacencyMatrix;
        private int NumEdges = 0;
        private double CurrSize = 0;

        public Graph() {
            AdjacencyMatrix = new Dictionary<int, Dictionary<int, double>>();
        }

        public Graph(Graph g) {
            this.AdjacencyMatrix = new Dictionary<int, Dictionary<int, double>>();
            foreach (var ilist in g.AdjacencyMatrix) {
                this.AdjacencyMatrix[ilist.Key] = new Dictionary<int, double>(ilist.Value);
            }
            this.NumEdges = g.NumEdges;
            this.CurrSize = g.CurrSize;
        }

        /// <summary>
        /// Adds a node to the graph.
        /// </summary>
        /// <param name="node">The node to add.</param>
        public void AddNode(int node) {
            EnsureIncidenceList(node);
        }

        /// <summary>
        /// Adds to the weight between node1 and node2 (edges are assumed to start with weight 0).
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="weight"></param>
        public void AddEdge(int node1, int node2, double weight) {
            AddDirectedEdge(node1, node2, weight);
            if (node1 != node2) {
                AddDirectedEdge(node2, node1, weight);
            }
            NumEdges += 1;
            CurrSize += weight;
        }

        private void AddDirectedEdge(int node1, int node2, double weight) {
            var ilist = EnsureIncidenceList(node1);
            double oldWeight;
            ilist.TryGetValue(node2, out oldWeight);
            ilist[node2] = oldWeight + weight;
        }

        public void SetEdge(int node1, int node2, double weight)
        {
            SetDirectedEdge(node1, node2, weight);
            if (node1 != node2)
            {
                SetDirectedEdge(node2, node1, weight);
            }
            NumEdges += 1;
            CurrSize += weight;
        }

        private void SetDirectedEdge(int node1, int node2, double weight)
        {
            var ilist = EnsureIncidenceList(node1);
            ilist[node2] = weight;
        }

        private Dictionary<int, double> EnsureIncidenceList(int node) {
            Dictionary<int, double> ilist;
            if (!AdjacencyMatrix.TryGetValue(node, out ilist)) {
                ilist = AdjacencyMatrix[node] = new Dictionary<int, double>();
            }
            return ilist;
        }

        /// <summary>
        /// The number of edges in the graph.
        /// </summary>
        public int NumberOfEdges { get { return NumEdges; } }

        /// <summary>
        /// Computes the size of the graph, as the sum of edge weights in the graph.
        /// </summary>
        /// <returns>The sum of edge weights.</returns>
        public double Size { get { return CurrSize; } }

        /// <summary>
        /// Computes the degree of a node, as the sum of edge weights of incident edges.
        /// </summary>
        /// <param name="node">The node whose edges' weights should be summed.</param>
        /// <returns>The weighted degree of the node.</returns>
        public double Degree(int node) {
            double loop;
            AdjacencyMatrix[node].TryGetValue(node, out loop); // since self loop has two ends
            return AdjacencyMatrix[node].Values.Sum() + loop;
        }

        /// <summary>
        /// An iterator for the nodes in the graph.
        /// </summary>
        public IEnumerable<int> Nodes { get { return AdjacencyMatrix.Keys; } }

        /// <summary>
        /// An iterator for the edges in the graph.
        /// </summary>
        public IEnumerable<Edge> Edges {
            get {
                foreach (var entry1 in AdjacencyMatrix) {
                    foreach (var entry2 in entry1.Value) {
                        if (entry1.Key <= entry2.Key) {
                            // don't double-count non-self-loops
                            yield return new Edge(entry1.Key, entry2.Key, entry2.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of incident edges for a node.
        /// </summary>
        /// <param name="node">The node from which the returned edges will be incident.</param>
        /// <returns>An enumeration of incident edges.</returns>
        public IEnumerable<Edge> IncidentEdges(int node) {
            Dictionary<int, double> incidence;
            if (AdjacencyMatrix.TryGetValue(node, out incidence)) {
                foreach (var entry in incidence) {
                    yield return new Edge(node, entry.Key, entry.Value);
                }
            } else {
                // nothing
            }
        }

        /// <summary>
        /// Returns the weight of the edge between two nodes.
        /// </summary>
        /// <param name="node1">The first node.</param>
        /// <param name="node2">The second node.</param>
        /// <param name="defaultValue">The default value to return if there is no such edge.</param>
        /// <returns>The weight of the edge (or the default value).</returns>
        public double EdgeWeight(int node1, int node2, double defaultValue) {
            Dictionary<int, double> ilist;
            if (!AdjacencyMatrix.TryGetValue(node1, out ilist)) {
                throw new IndexOutOfRangeException("No such node " + node1);
            }
            double value;
            if (!ilist.TryGetValue(node2, out value)) {
                return defaultValue;
            } else {
                return value;
            }
        }

        /// <summary>
        /// Produces the induced graph from the quotient described by the partition.  The partition is a dictionary from nodes to communities.
        /// The produced graph has nodes which are communities, and there is a link of weight w between communities if the sum of the weights of the links
        /// between their elements is w.
        /// </summary>
        /// <param name="partition">A dictionary where keys are graph nodes and values are the community to which the node belongs.</param>
        /// <returns>The quotient graph.</returns>
        public Graph Quotient(Dictionary<int, int> partition) {
            Graph ret = new Graph();
            foreach (int com in partition.Values) {
                ret.AddNode(com);
            }
            foreach (var edge in this.Edges) {
                ret.AddEdge(partition[edge.FromNode], partition[edge.ToNode], edge.Weight);
            }
            return ret;
        }

        /// <summary>
        /// Creates a 
        /// </summary>
        /// <returns></returns>
        public Graph RandomizedNodes(Random random) {
            Graph g = new Graph();
            List<int> nodes = this.Nodes.ToList();
            for (int i = nodes.Count - 1; i >= 1; i--) {
                int j = random.Next(i + 1);
                int v = nodes[i];
                nodes[i] = nodes[j];
                nodes[j] = v;
            }
            Dictionary<int, int> remapping = new Dictionary<int,int>();
            for (int i = 0; i < nodes.Count; i++) {
                remapping[nodes[i]] = i;
            }
            List<Edge> edges = this.Edges.ToList();
            for (int i = edges.Count - 1; i >= 1; i--) {
                int j = random.Next(i + 1);
                Edge v = edges[i];
                edges[i] = edges[j];
                edges[j] = v;
            }
            foreach (var edge in edges) {
                g.AddEdge(remapping[edge.FromNode], remapping[edge.ToNode], edge.Weight);
            }
            return g;
        }
    }
}
