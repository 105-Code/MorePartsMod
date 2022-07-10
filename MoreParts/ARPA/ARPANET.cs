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
            this.insertNode(worldLocation, worldLocation.transform.gameObject, true);
        }

        public bool isConnected(Node source)
        {
            source.mark = true;
            bool result = this._isConnected(source);
            this.clearMarks();
            return result;
        }

        public bool _isConnected(Node origin)
        {
            Double2 position = getAbsolutePosition(origin);

            foreach (Node node in this._nodes)
            {
                if (node == origin || node.mark)
                {
                    continue;
                }
                Double2 nodePosition = getAbsolutePosition(node);
                Vector2 direction = (nodePosition - position).normalized;                

                if (node.IsOrigin)
                {

                    PlanetModule planet = node.isAvailabe(direction);
                    if (planet != null && planet.planet.name != "Earth")
                    {
                        continue;
                    }

                    if (direction.y < 0)
                    {
                        return true;
                    }
                    continue;
                }

                if (node.isAvailabe(direction) == null)
                {
                    node.mark = true;
                    bool result = this._isConnected(node);
                    if (result)
                    {
                        return true;
                    }
                    node.mark = false;
                    continue;
                }
            }
            return false;
        }

        /*public Node getAvailabeNode(Node source)
        {
            Node response = this._getAvailabeNode(source);
            this.clearMarks();
            return response;
        }

        private Node _getAvailabeNode(Node origin)
        {
            Double2 position = getAbsolutePosition(origin);


            foreach (Node node in this._nodes)
            {
                if(node == origin || node.mark)
                {
                    continue;
                }
                Double2 nodePosition = getAbsolutePosition(node);
                Vector2 direction = (nodePosition - position).normalized;
                Debug.Log("Node "+node.Id+" Direction:"+direction);
                node.mark = true;

                if (node.IsOrigin)
                {
                   
                    PlanetModule planet = node.isAvailabe(direction);
                    if(planet != null && planet.planet.name != "Earth") {
                        continue;
                    }
                    
                    if(direction.y < 0)
                    {
                        return node;
                    }
                    continue;
                }

                if (node.isAvailabe(direction) == null)
                {
                    return node;
                }
            }
            return null;
        }
        */
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

           /* if (node.isAvailabeNextNode())
            {
                if (node.nextNode.IsOrigin)
                {
                    return true;
                }
                return this.checkRoute(node.nextNode);
            }*/
            return false;
        }

        /*private bool _isConnected(Node origin)
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
        }*/

     

        /*public bool isConnected(Node origin)
        {
            bool result;
            
            // check if already there is a route
            if (this._routeOrigin == origin)
            {
        
                result = this.checkRoute(origin);
                if (result)
                {
                    return true; // there is a active route
                }
            }

            result = this._isConnected(origin); // search new route
            this.clearMarks();
            if (result)// there is a route
            {
 
                this._routeOrigin = origin; // set the new route
            }
            return result;
        }*/

        public Node insertNode(WorldLocation worldLocation,GameObject dish, bool isOrigin = false)
        {
            Node newNode = new Node(this._currentNode, worldLocation, dish, isOrigin);
            this._currentNode++;
            if (isOrigin)
            {
                this._origin = newNode;
            }

            this._nodes.Add(newNode);
            Debug.Log("Satellite "+ newNode.Id +" Connected! Total:"+this._nodes.Count);
            return newNode;
        }

        public int totalSatellites()
        {
            return this._nodes.Count;
        }
        public Node getNode(int index)
        {
            return this._nodes[index];
        }

        public void destroyNode(Node target)
        {
            this._nodes.Remove(target);
            Debug.Log("Cantidad de stelites activos "+this._nodes.Count());
        }

        public static Double2 getAbsolutePosition(Node node)
        {
            return node.Position.planet.Value.GetSolarSystemPosition() + node.Position.Value.position;
        }
    
    }
}
