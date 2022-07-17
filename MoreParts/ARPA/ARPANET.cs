using SFS.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MorePartsMod.ARPA
{
    class ARPANET
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
                node.mark = false;
            }
        }

        public bool IsConnected(Node origin)
        {
            foreach (Node node in this._nodes)
            {
                
                if (node == origin || node.mark)
                {
                    continue;
                }

                if (!origin.IsAvailableTo(node))
                {
                    continue;
                }

                if (node.IsOrigin)
                {
                    origin.next = node;
                    return true;
                }

                node.mark = true;
                if (this.IsConnected(node))
                {
                    origin.next = node;
                    return true;
                }
                node.mark = false;
            }

            return false;
        }

        public bool CheckRoute(Node origin)
        {
            if (origin.IsOrigin)
            {
                return true;
            }

            if (!origin.IsAvailableTo(origin.next))
            {
                return false;
            }
            return this.CheckRoute(origin.next);

        }

        public void ClearRoute(Node origin)
        {
            if(origin == null)
            {
                return;
            }
            this.ClearRoute(origin.next);
            origin.next = null;
        }
    
    }
}
