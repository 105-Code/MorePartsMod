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

        public void ClearMarks()
        {
            foreach (Node node in this._nodes)
            {
                node.Mark = false;
            }
        }

        public bool IsConnected(Node origin)
        {
            foreach (Node node in this._nodes)
            {
                
                if (node == origin || node.Mark)
                {
                    continue;
                }

                if (!origin.IsAvailableTo(node))
                {
                    continue;
                }

                if (node.IsOrigin)
                {
                    origin.Next = node;
                    return true;
                }

                node.Mark = true;
                if (this.IsConnected(node))
                {
                    origin.Next = node;
                    return true;
                }
                node.Mark = false;
            }

            return false;
        }

        public bool CheckRoute(Node origin)
        {
            Node aux = origin;
            while(aux != null)
            {
                if (aux.IsOrigin)
                {
                    return true;
                }

                if (!aux.IsAvailableTo(aux.Next))
                {
                    return false;
                }
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
