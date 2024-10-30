using System;


namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }
        
        
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        // Here we calculate the intersection of a ray with an ellipsoid
        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            // TODO: ADDed CODE HERE

            // Translate ray's origin to the ellipsoid's center
            Vector origin = line.X0 - Center;
            Vector direction = line.Dx;

            // Semi-axis lengths of the ellipsoid
            double a = SemiAxesLength.X;
            double b = SemiAxesLength.Y; 
            double c = SemiAxesLength.Z; 

            // Transform origin and direction based on ellipsoid's axes
            Vector originTransformed = new Vector(origin.X / a, origin.Y / b, origin.Z / c);
            Vector directionTransformed = new Vector(direction.X / a, direction.Y / b, direction.Z / c);

            // Normalize the direction vector
            directionTransformed.Normalize();

            // Coefficients for the quadratic equation
            double A = directionTransformed.Dot(directionTransformed); // Sum of the squares of transformed direction
            double B = 2 * originTransformed.Dot(directionTransformed); // 2 * dot product of origin and direction
            double C = originTransformed.Dot(originTransformed) - 1; // Sum of squares of transformed origin - 1

            // Discriminant of the quadratic equation
            double discriminant = B * B - 4 * A * C;

            if (discriminant < 0)
            {
                // No solution for the equation -> No Intersection
                return Intersection.NONE;
            }

            // Calculate the two possible solutions for t (potential intersection distances)
            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double t1 = (-B - sqrtDiscriminant) / (2 * A);
            double t2 = (-B + sqrtDiscriminant) / (2 * A);

            // Check if the intersections are within the specified distance range
            if (t2 < minDist || t1 > maxDist)
            {
                return Intersection.NONE;
            }

            // Select the closest intersection within range
            double t = (t1 >= minDist) ? t1 : t2;
            if (t < minDist || t > maxDist)
            {
                return Intersection.NONE;
            }

            // Calculate the intersection position and the normal at the point
            Vector intersectionPosition = line.CoordinateToPosition(t);
            Vector normal = Normal(intersectionPosition);

            return new Intersection(true, true, this, line, t, normal, Material, Color);
        }

        public Vector Normal(Vector point)
        {
            double dx = (point.X - Center.X) / SemiAxesLength.X;
            double dy = (point.Y - Center.Y) / SemiAxesLength.Y;
            double dz = (point.Z - Center.Z) / SemiAxesLength.Z;

            Vector normalized = new Vector(dx, dy, dz);
            return normalized.Normalize();
        }
    }
}
