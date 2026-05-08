using SFS;
using SFS.World;
using SFS.WorldBase;

namespace MorePartsMod.ARPA
{
    public class Node
    {

        public Node Next { set; get; }
        public int Id { get; private set; }
        public bool IsOrigin { get; private set; }
        public WorldLocation WorldLocation { get; private set; }


        public Node(int id, WorldLocation worldLocation, bool isOrigin =false)
        {
            this.Id = id;
            this.Next = null;
            this.WorldLocation = worldLocation;
            this.IsOrigin = isOrigin;
        }

        public bool IsAvailableTo(Node target)
        {
            Double2 origin = this.GetAbsolutePosition();
            Double2 dest = target.GetAbsolutePosition();
            foreach (Planet planet in Base.planetLoader.planets.Values)
            {
                if (this.HitPlanet(planet.Radius, planet.GetSolarSystemPosition(), origin, dest))
                {
                    return false;
                }
            }
            return true;
        }

        // segment vs circle: project planet center onto the origin->target segment,
        // hit if the closest point lies on the segment and is inside the planet.
        private bool HitPlanet(double planetRadius, Double2 planetCenter, Double2 origin, Double2 target)
        {
            double dx = target.x - origin.x;
            double dy = target.y - origin.y;
            double lenSq = dx * dx + dy * dy;
            if (lenSq <= 0) return false;

            double t = ((planetCenter.x - origin.x) * dx + (planetCenter.y - origin.y) * dy) / lenSq;
            if (t <= 0 || t >= 1) return false;

            double closestX = origin.x + t * dx;
            double closestY = origin.y + t * dy;
            double distX = closestX - planetCenter.x;
            double distY = closestY - planetCenter.y;
            return (distX * distX + distY * distY) < planetRadius * planetRadius;
        }

        public Double2 GetAbsolutePosition()
        {
            return this.WorldLocation.planet.Value.GetSolarSystemPosition() + this.WorldLocation.Value.position;
        }

    }
}
