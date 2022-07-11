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
            this.insert(worldLocation, true);
        }

        public Node insert(WorldLocation worldLocation, bool isOrigin = false)
        {
            Node newNode = new Node(this._counter, worldLocation, isOrigin);
            this._counter++;
            this._nodes.Add(newNode);
            return newNode;
        }

        public void remove(Node target)
        {
            this._nodes.Remove(target);
        }

        public void clearMarks()
        {
            foreach (Node node in this._nodes)
            {
                node.mark = false;
            }
        }

        public bool isConnected(Node origin)
        {
            foreach (Node node in this._nodes)
            {
                
                if (node == origin || node.mark)
                {
                    continue;
                }

                if (!origin.isAvailableTo(node))
                {
                    //Debug.Log("Ruta no Valida");
                    continue;
                }
                //Debug.Log("Ruta valida desde" + origin.Id + " a " + node.Id);

                if (node.IsOrigin)
                {
                    origin.next = node;
                    return true;
                }

                node.mark = true;
                //Debug.Log("Siguiente Nodo");
                if (this.isConnected(node))
                {
                    origin.next = node;
                    return true;
                }
                node.mark = false;
            }

            return false;
        }

        public bool checkRoute(Node origin)
        {
            if (origin.IsOrigin)
            {
                return true;
            }

            if (!origin.isAvailableTo(origin.next))
            {
                return false;
            }
            return this.checkRoute(origin.next);

        }

        public void clearRoute(Node origin)
        {
            if(origin == null)
            {
                return;
            }
            this.clearRoute(origin.next);
            origin.next = null;
        }
    
    }
}
