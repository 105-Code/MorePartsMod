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
        private Node _origin; // space center antenna




        public ARPANET(WorldLocation worldLocation)
        {
            this._nodes = new List<Node>();
            this._counter = 0;
            this.insert(worldLocation, worldLocation.transform.gameObject, true);
        }

        public Node insert(WorldLocation worldLocation, GameObject dish, bool isOrigin = false)
        {
            Node newNode = new Node(this._counter, worldLocation, dish, isOrigin);
            this._counter++;
            if (isOrigin)
            {
                this._origin = newNode;
            }
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
            Double2 position = getAbsolutePosition(origin);

            foreach (Node node in this._nodes)
            {
                
                if (node == origin || node.mark)
                {
                    continue;
                }
                Double2 nodePosition = getAbsolutePosition(node);
                Vector2 direction = (nodePosition - position ).normalized;
                PlanetHelper hit = origin.isAvailabe(direction);
                Debug.Log("From: Node " + origin.Id + " to: Node " + node.Id+" Direction: "+direction);

                if (node.IsOrigin)
                {
                    if(hit != null && hit.name != "Earth")
                    {
                        Debug.Log("Hit to origin" + hit.name);
                        // there is a hit whit a planet, so is no possible communicate with the origin node
                        continue;
                    }
                    if(direction.y < 0)
                    {
                        origin.next = node;
                        return true;
                    }
                    continue;
                }


                if(hit != null)
                {
                    Debug.Log("Hit "+hit.name);
                    continue;
                }

                node.mark = true;
                //Debug.Log("Serching next Node");
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
            Double2 position = getAbsolutePosition(origin);
            Double2 nodePosition = getAbsolutePosition(origin.next);
            Vector2 direction = (nodePosition - position).normalized;
            PlanetHelper hit = origin.isAvailabe(direction);
            if (origin.next.IsOrigin)
            {
                if (hit != null && hit.name != "Earth")
                {
                    // there is a hit whit a planet, so is no possible communicate with the origin node
                    return false;
                }
                if (direction.y < 0)
                {
                    return true;
                }
                return false;
            }

            if (hit != null)
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

        public static Double2 getAbsolutePosition(Node node)
        {
            return node.WorlLocation.planet.Value.GetSolarSystemPosition() + node.WorlLocation.Value.position;
        }
    
    }
}
