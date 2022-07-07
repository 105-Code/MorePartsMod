using SFS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.ARPA
{
    class Node
    {
        private WorldLocation _worldLocation;
        //private Dictionary<int,Arc> _arcs;
        private bool _isOrigin;
        public bool mark;
        private int _id;
        public Node nextNode;

        public int Id { get => this._id; }
        public bool IsOrigin { get => this._isOrigin; }

        public int Radius { get => 200000; } // 200 Km

        public Vector2 Position {  get => this._worldLocation.position.Value;
        }
      


        public Node(int id, WorldLocation worldLocation, bool isOrigin =false)
        {
            this._id = id;
            this.mark = false;
            this.nextNode = null;
            this._worldLocation = worldLocation;
            this._isOrigin = isOrigin;
            //this._arcs = new Dictionary<int,Arc>();
        }

        public bool isNear(Vector2 position)
        {
            return ARPANET.calculateDistance(position, this.Position) <= this.Radius;
        }
        
        public bool isNearNextNode()
        {
            //Debug.Log("Checking if " + this.nextNode.Id+" is near");
            return this.isNear(this.nextNode.Position);
        }

        /*
        public void insertArc(Node destination)
        {
            Arc arc = new Arc(destination);
            this._arcs.Add(destination._id,arc);
        }

        public bool destroyArc(Node target)
        {
            return this._arcs.Remove(target._id);
        }*/


    }
}
