﻿using SFS.World;
using UnityEngine;
using System;
using SFS.WorldBase;
using SFS.World.Maps;
using System.Collections.Generic;
using SFS;
using SFS.UI;
using MorePartsMod.ARPA;
using MorePartsMod.Utils;
using MorePartsMod.Parts;

namespace MorePartsMod.Buildings
{
    public class AntennaComponent : MonoBehaviour
    {
        public static AntennaComponent main;

        public WorldLocation Location;
        public Color LineColor;

        private bool _hasTelecommunicationDish;
        private bool _enableTelecomunicationLines;
        private ARPANET _network;
        private Node _routeOrigin;
        private Planet _sunPlanet;

        public bool ShowTelecommunicationLines { set; private get; }

        private void Awake()
        {
            if (GameManager.main == null)
            {
                return;
            }
            main = this;
            this._network = new ARPANET(this.Location);
            this._hasTelecommunicationDish = false;
            this.ShowTelecommunicationLines = false;
            this._enableTelecomunicationLines = KeySettings.Main.Show_Telecommunication_lines;
            this._sunPlanet = this.getPrimaryPlanet();
        }

        private Planet getPrimaryPlanet()
        {
            foreach (Planet planet in Base.planetLoader.planets.Values)
            {
                if (planet.parentBody == null)
                {
                    return planet;
                }
            }
            return null;
        }

        private void Start()
        {
            if (GameManager.main == null)
            {
                return;
            }
            PlayerController.main.player.OnChange += this.OnChangePlayer;

            KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Telecommunication_Dish, this.ToggleRocketAntenna);
            KeySettings.AddOnKeyDown_World(KeySettings.Main.Toggle_Telecommunication_Lines, this.ToggleTelecommunicationlines);
        }


        private void ToggleTelecommunicationlines()
        {
            KeySettings.Main.ToggleShowTelecommunicationLines();
            this._enableTelecomunicationLines = KeySettings.Main.Show_Telecommunication_lines;
            if (this._enableTelecomunicationLines)
            {
                MsgDrawer.main.Log("Draw Lines On");
                return;
            }
            MsgDrawer.main.Log("Draw Lines Off");
        }

        private void ToggleRocketAntenna()
        {
            Rocket rocket = PlayerController.main.player.Value as Rocket;
            TelecommunicationDishModule[] antennas = rocket.partHolder.GetModules<TelecommunicationDishModule>();
            if (antennas.Length > 0)
            {
                antennas[0]._toggle();
            }
        }

        public Node AddNode(WorldLocation location, bool isOrigin = false)
        {
            return this._network.Insert(location, isOrigin);
        }

        public Node AddNode(TelecommunicationDishModule dish)
        {
            WorldLocation location = dish.Rocket.GetComponent<WorldLocation>();
            return this._network.Insert(location);
        }

        public void RemoveNode(TelecommunicationDishModule dish)
        {
            this._network.Remove(dish.Node);
            this._network.ClearRoute(this._routeOrigin);
        }

        public bool IsConnected(Node origin)
        {
            try
            {
                bool result;
                if (origin.Next != null)
                {
                    result = this._network.CheckRoute(origin);
                    if (result)
                    {
                        return true;
                    }
                    this._network.ClearRoute(origin);
                }
                origin.Mark = true;
                result = this._network.IsConnected(origin);
                if (result)
                {
                    this._routeOrigin = origin;
                }
                this._network.ClearMarks();
                return result;
            }
            catch (Exception error)
            {
                Debug.Log("Errror in isConnected");
                Debug.LogError(error);
                return false;
            }
        }

        public void OnChangePlayer()
        {
            this._network.ClearRoute(this._routeOrigin);
            Rocket rocket = (Rocket)PlayerController.main.player;
            if (rocket == null)
            {
                this._hasTelecommunicationDish = false;
                return;
            }
            this._hasTelecommunicationDish = rocket.partHolder.GetModules<TelecommunicationDishModule>().Length > 0;
        }

        public void DrawInMap()
        {
            MapUtils.DrawLandmarkInPlanet(this.Location.planet.Value, (float)this.Location.Value.position.AngleDegrees, this.Location.Value.position, "Space Center", Color.white);

            if (this._enableTelecomunicationLines && this._hasTelecommunicationDish && this.ShowTelecommunicationLines)
            {
                this.DrawTelecommunicationLines();
            }
        }

        private void DrawTelecommunicationLines()
        {
            if (this._routeOrigin == null)
            {
                return;
            }

            List<Vector3> points = new List<Vector3>();
            Node aux = this._routeOrigin;
            while (aux != null)
            {
                points.Add(aux.GetAbsolutePosition() / 1000);

                if (aux.IsOrigin)
                {
                    break;
                }

                aux = aux.Next;
            }

            Map.solidLine.DrawLine(points.ToArray(), this._sunPlanet, LineColor, LineColor);

        }

    }


}
