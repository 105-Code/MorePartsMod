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
        private Node _origin; // space center antenna
        private int _currentNode; // total satellites inserted
        private Node _routeOrigin; // first router satellite

        public ARPANET(WorldLocation worldLocation)
        {
            this._nodes = new List<Node>();
            this._currentNode = 0;
            this.insertNode(worldLocation, true);
        }

        private void clearMarks()
        {
            foreach(Node node in this._nodes)
            {
                node.mark = false;
            }
        }

        private bool checkRoute(Node node)
        {
            //Debug.Log("Checking satellite "+node.Id);
            if (node.IsOrigin)
            {
                return true;
            }

            if (node.isNearNextNode())
            {
                if (node.nextNode.IsOrigin)
                {
                    return true;
                }
                return this.checkRoute(node.nextNode);
            }
            return false;
        }

        private bool _isConnected(Node origin)
        {
            if (origin.mark)
            {
                return false;
            }

            origin.mark = true;

            foreach (Node destination in this._nodes)
            {
                if (!origin.isNear(destination.Position))
                {
                    continue;
                }

                // para la interferencia de cuerpos celestes
                if (false)
                {
                    continue;
                }

                if (destination == this._origin)
                {
                    origin.nextNode = destination;
                    return true;
                }

                if (this._isConnected(destination))
                {
                    origin.nextNode = destination;
                    return true;
                }
            }
            return false;
        }

        public Node getClosestNode(Node rocketNode)
        {
            foreach (Node node in this._nodes)
            {
                if (node == rocketNode)
                {
                    continue;
                }

                if (node.isNear(rocketNode.Position))
                {
                    //Debug.Log("Closest Satellite is "+node.Id);
                    return node;
                }
            }
            //Debug.Log("No Satellite near ");
            return null;
        }

        public bool isConnected(Node origin)
        {
            bool result;
            
            // check if already there is a route
            if (this._routeOrigin == origin)
            {
                //Debug.Log("Checking active route");
                result = this.checkRoute(origin);
                if (result)
                {
                    //Debug.Log("Active route");
                    return true; // there is a active route
                }
            }
            //Debug.Log("Route no valid");
            //Debug.Log("Searching Route");
            result = this._isConnected(origin); // search new route
            this.clearMarks();
            if (result)// there is a route
            {
                //Debug.Log("There is new route");
                this._routeOrigin = origin; // set the new route
            }
            return result;
        }

        public Node insertNode(WorldLocation worldLocation, bool isOrigin = false)
        {
            Node newNode = new Node(this._currentNode, worldLocation, isOrigin);
            this._currentNode++;
            if (isOrigin)
            {
                this._origin = newNode;
            }
            /*foreach(Node node in this._nodes)
            {
                newNode.insertArc(node);
                node.insertArc(newNode);
            }*/

            this._nodes.Add(newNode);
            Debug.Log("Satellite "+ newNode.Id +" Connected! Total:"+this._nodes.Count);
            return newNode;
        }

        public void destroyNode(Node target)
        {
            /*foreach (Node node in this._nodes)
            {
                node.destroyArc(target);
            }*/
            this._nodes.Remove(target);
            Debug.Log("Cantidad de stelites activos "+this._nodes.Count());
        }

        public static float calculateDistance(Vector2 destination, Vector2 origin)
        {
            
            // more fast than Math.pow
            float temp = destination.x - origin.x;
            float x = temp * temp;
            temp = destination.y - origin.y;
            float y = temp * temp;
            return (float)Math.Sqrt(x + y);
        }
    
    }
}
