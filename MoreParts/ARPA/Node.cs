using SFS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MorePartsMod.ARPA
{
    class Node
    {
        private WorldLocation _worldLocation;
        public GameObject dish;
        private bool _isOrigin;
        public bool mark;
        private int _id;
        public Node nextNode;

        public int Id { get => this._id; }
        public bool IsOrigin { get => this._isOrigin; }


        public WorldLocation Position {  get => this._worldLocation; }
      


        public Node(int id, WorldLocation worldLocation,GameObject dish, bool isOrigin =false)
        {
            this._id = id;
            this.mark = false;
            this.nextNode = null;
            this._worldLocation = worldLocation;
            this._isOrigin = isOrigin;
            this.dish = dish;
        }

        public PlanetModule isAvailabe(Vector2 direction)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(dish.transform.position, direction);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null)
                {
                    continue;
                }

                if (hit.collider.gameObject.HasComponent<PlanetModule>())
                {
                    PlanetModule planet = hit.collider.gameObject.GetComponent<PlanetModule>();
                    Debug.Log("Hit " + planet.planet.name);
                    return planet;
                }
            }
            return null;

        }

    }
}
