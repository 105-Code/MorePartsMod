
using System;
using Newtonsoft.Json;
using UnityEngine;

namespace MorePartsMod.Buildings
{
    [Serializable]
    public class Building
    {
        public Double2 position;
        public float rotation;

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        public Building(Double2 pos, float rotation, GameObject gameObject)
        {
            this.position = pos;
            this.rotation = rotation;
            this.GameObject = gameObject;
        }
    }

}
