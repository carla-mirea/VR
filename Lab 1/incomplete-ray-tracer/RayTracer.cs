using Microsoft.VisualBasic;
using System;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                try
                {
                    var intr = geometry.GetIntersection(ray, minDist, maxDist);

                    if (!intr.Valid || !intr.Visible) continue;

                    if (!intersection.Valid || !intersection.Visible)
                    {
                        intersection = intr;
                    }
                    else if (intr.T < intersection.T)
                    {
                        intersection = intr;
                    }
                }
                catch (NotImplementedException e)
                {
                    Console.WriteLine($"Unimplemented method in GetIntersection: {e.Message}");
                }
                
            }

            return intersection;
        }

        // We determine how each pixel in a 3D scene should appear based on lighting (illuminated by a light source or in shadow)
        private bool IsLit(Vector point, Light light)
        {
            // TODO: ADDed CODE HERE

            // Calculate the direction from the light to the point being checked
            Vector directionToLight = (light.Position - point).Normalize();
            double maxDist = (light.Position - point).Length();

            // Create a shadow ray from the point to the light source
            Line shadowRay = new Line(point + directionToLight * 0.001, light.Position); // Slight offset to avoid self-intersection

            // Check if there's any intersection along the shadow ray path
            var shadowIntersection = FindFirstIntersection(shadowRay, 0.001, maxDist);

            // If there's no intersection, the point is lit by this light
            return !shadowIntersection.Valid || !shadowIntersection.Visible;
        }

        // We determine the final image by tracing rays from a camera through each pixel in a grid (representing the screen)
        // The color is also determined for each pixel based on intersections with scene geometry and lighting
        public void Render(Camera camera, int width, int height, string filename)
        {
            // TODO: ADDed CODE HERE

            var background = new Color(0.2, 0.2, 0.2, 1.0); // Default background color
            var image = new Image(width, height);

            // Ensure proper orientation for ray-tracing calculations
            camera.Normalize();

            // Right vector for camera (cross product of direction and up)
            Vector right = (camera.Direction ^ camera.Up).Normalize();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // Calculate the pixel's position in view space
                    double x = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    double y = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    // Calculate the point on the view plane
                    Vector pointOnViewPlane = camera.Position +
                                              camera.Direction * camera.ViewPlaneDistance +
                                              right * x + camera.Up * y;

                    // Ray from the camera to the point on the view plane
                    Line ray = new Line(camera.Position, pointOnViewPlane);
                    Intersection firstIntersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    // Determine the color for the pixel
                    Color pixelColor = background;

                    if (firstIntersection.Visible && firstIntersection.Valid)
                    {
                        // Start with a black color for shading
                        Color finalColor = new Color(0, 0, 0, 1); 
                        Material material = firstIntersection.Geometry.Material;

                        foreach (var light in lights)
                        {
                            if (IsLit(firstIntersection.Position, light))
                            {
                                Vector normal = firstIntersection.Normal;
                                Vector lightDir = (light.Position - firstIntersection.Position).Normalize();
                                Vector viewDir = (camera.Position - firstIntersection.Position).Normalize();

                                // Ambient component
                                finalColor += material.Ambient * light.Ambient;

                                // Diffuse component
                                double diffuseFactor = Math.Max(0, normal.Dot(lightDir));
                                finalColor += material.Diffuse * light.Diffuse * diffuseFactor;

                                // Specular component
                                Vector reflection = 2 * (normal.Dot(lightDir)) * normal - lightDir;
                                double specularFactor = Math.Pow(Math.Max(0, viewDir.Dot(reflection)), material.Shininess);
                                finalColor += material.Specular * light.Specular * specularFactor;

                                finalColor *= light.Intensity; // Adjust by light intensity
                            }
                        }
                        pixelColor = finalColor;
                    }

                    // Set the pixel color in the image
                    image.SetPixel(i, j, pixelColor);
                }
            }

            image.Store(filename);
        }
    }
}