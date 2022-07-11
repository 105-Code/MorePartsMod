using MorePartsMod.Buildings;
using SFS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.ARPA
{
    class Node
    {
        private WorldLocation _worldLocation;
        private bool _isOrigin;
        private int _id;
        private Transform _startPoint;

        public Node next;
        public bool mark;

        public int Id { get => this._id; }
        public bool IsOrigin { get => this._isOrigin; }
        public WorldLocation WorlLocation {  get => this._worldLocation; }
      


        public Node(int id, WorldLocation worldLocation,GameObject dish, bool isOrigin =false)
        {
            this._id = id;
            this.mark = false;
            this.next = null;
            this._worldLocation = worldLocation;
            this._isOrigin = isOrigin;
            this._startPoint = dish.transform;
        }

        public PlanetHelper isAvailabe(Vector2 direction)
        {
            //Debug.Log("ReycastAll from Node"+this.Id+" "+ this._startPoint.position.ToString()+" Direction"+ direction.ToString());
            RaycastHit2D[] hits = Physics2D.RaycastAll(this._startPoint.position, direction);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider.gameObject.HasComponent<PlanetHelper>())
                {
                    return hit.collider.gameObject.GetComponent<PlanetHelper>();
                }
            }
            return null; // there is not hits
        }

    }
}
