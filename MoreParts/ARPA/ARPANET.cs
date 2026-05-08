using SFS.World;
using System.Collections.Generic;

namespace MorePartsMod.ARPA
{
    public class ARPANET
    {
        private List<Node> _nodes; // all satellites
        private int _counter;

        public ARPANET(WorldLocation worldLocation)
        {
            this._nodes = new List<Node>();
            this._counter = 0;
            this.Insert(worldLocation, true);
        }

        public Node Insert(WorldLocation worldLocation, bool isOrigin = false)
        {
            Node newNode = new Node(this._counter, worldLocation, isOrigin);
            this._counter++;
            this._nodes.Add(newNode);
            return newNode;
        }

        public void Remove(Node target)
        {
            this._nodes.Remove(target);
        }

        public bool IsConnected(Node start)
        {
            List<Node> queue = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            Dictionary<Node, Node> parent = new Dictionary<Node, Node>();

            queue.Add(start);
            visited.Add(start);

            for (int head = 0; head < queue.Count; head++)
            {
                Node current = queue[head];

                if (current.IsOrigin)
                {
                    Node child = current;
                    Node prev;
                    while (parent.TryGetValue(child, out prev))
                    {
                        prev.Next = child;
                        child = prev;
                    }
                    return true;
                }

                foreach (Node neighbor in this._nodes)
                {
                    if (visited.Contains(neighbor)) continue;
                    if (!current.IsAvailableTo(neighbor)) continue;
                    visited.Add(neighbor);
                    parent[neighbor] = current;
                    queue.Add(neighbor);
                }
            }

            return false;
        }

        public bool CheckRoute(Node origin)
        {
            Node aux = origin;
            while(aux != null)
            {
                if (aux.IsOrigin) return true;
                if (aux.Next == null) return false;
                if (!aux.IsAvailableTo(aux.Next)) return false;
                aux = aux.Next;
            }
            return false;
        }

        public void ClearRoute(Node origin)
        {
            if(origin == null)
            {
                return;
            }
            this.ClearRoute(origin.Next);
            origin.Next = null;
        }
    
    }
}
