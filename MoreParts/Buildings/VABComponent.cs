using System;
using UnityEngine;

namespace MorePartsMod.Buildings
{
    [Obsolete("I need check this later",true)]
    class VABComponent : MonoBehaviour
    {

        public static void Setup(GameObject colonyPrefab)
        {
            GameObject building = colonyPrefab.transform.FindChild("Holder").gameObject.transform.FindChild("VAB").gameObject;
            building.AddComponent<VABComponent>();
        }
    }
}
