using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

// Own
using SharpDX.Toolkit.Input;
// End my own

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    class Landscape : ColoredGameObject
    {
        // Own
        private Vector3 initialCameraPosition;
        private Vector3 cameraDirection;
        private Vector3 cameraOffset;
        private const float speed = 3f; // Default speed should be 3
        private const float waterHeight = 3f;
        private Camera camera;
        private const float heightAboveGround = 7f; // Can think of this as the head height
        private Color waterColor = new Color(0.67f, 0.90f, 0.93f, 1f) * 0.9f;
        private float gravityStrength = 2.5f; // Default should be 2.5
        // set n to some positive power of 2
        private int n = 512;
        private float cornerHeight = 35.0f;
        // Used in the diamond-square algorithm. Lower = smoother but not as 'peaky'
        private float jaggedness = 0.47f;
        private int maxRng = 120;
        private float on = 1; // This turns on whether I add the random number to the midpoints of the edges of the 'squares'
        private float ambientLightIntensity = 15.0f;
        // End my own

        public Landscape(Project1Game game)
        {
            Random rng = new Random(3);

            // Initialize the heights of the corners to the same value      
            // If desired, they can be changed to have varying heights.
            float topLeftHeight = cornerHeight;
            float topRightHeight = cornerHeight;
            float bottomLeftHeight = cornerHeight;
            float bottomRightHeight = cornerHeight; 

            // Array indices are in [x, z]
            VertexPositionNormalColor[,] myLandscapesVertices = new VertexPositionNormalColor[n + 1, n + 1];

            myLandscapesVertices[0, n] = new VertexPositionNormalColor(new Vector3((float)(0), topLeftHeight, (float)(n)), new Vector3(0, 1, 0), Color.Gray);
            myLandscapesVertices[n, n] = new VertexPositionNormalColor(new Vector3((float)(n), topRightHeight, (float)(n)), new Vector3(0, 1, 0), Color.Gray);
            myLandscapesVertices[n, 0] = new VertexPositionNormalColor(new Vector3((float)(n), bottomRightHeight, (float)(0)), new Vector3(0, 1, 0), Color.Gray);
            myLandscapesVertices[0, 0] = new VertexPositionNormalColor(new Vector3((float)(0), bottomLeftHeight, (float)(0)), new Vector3(0, 1, 0), Color.Gray); 
            
            // This is the beginning of the Diamond-Square algorithm (iterative, as opposed to recursive)
            // The first pass goes through the n x n square. Towards the end of the first pass, I halve the side (i.e. side /= 2)
            // Then I go through the four squares with sides of length 'side', etc.
            // The vertices are first set to have a normal of (0, 1, 0), which will be changed afterwards.
            int side = n;
            int half = -1; // Initialize half to something (the -1 has no meaning)

            double power = 0; // This is used for reducing the random number generated
            while (side > 1) {
                half = side / 2;
                // This double for loop allows the algorithm to consider each square of length 'side'
                for (int i = 0; i < n; i += side) {
                    for (int j = 0; j < n; j += side) {
                        myLandscapesVertices[i + half, j + half] = new VertexPositionNormalColor(
                                                                    new Vector3(
                                                                        i + half,
                                                                        (float)(myLandscapesVertices[i, j].Position.Y + myLandscapesVertices[i + side, j].Position.Y +
                                                                        myLandscapesVertices[i + side, j + side].Position.Y + myLandscapesVertices[i, j + side].Position.Y) / 4.0f +
                                                                        (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power),
                                                                        j + half),
                                                                    new Vector3(0, 1, 0),
                                                                   Color.White);
                         
                        
                        // Middle bottom point of square
                        if (j == 0) {
                            myLandscapesVertices[i + half, j] = new VertexPositionNormalColor(
                                                                    new Vector3(
                                                                        i + half,
                                                                        (myLandscapesVertices[i, j].Position.Y + myLandscapesVertices[i + side, j].Position.Y +
                                                                        myLandscapesVertices[i + half, j + half].Position.Y) / 3.0f +
                                                                        (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                        j),
                                                                    new Vector3(0, 1, 0),
                                                                    Color.Green);
                        } else {
                            myLandscapesVertices[i + half, j] = new VertexPositionNormalColor(
                                                                    new Vector3(
                                                                            i + half,
                                                                            (myLandscapesVertices[i, j].Position.Y + myLandscapesVertices[i + side, j].Position.Y +
                                                                            myLandscapesVertices[i + half, j + half].Position.Y + myLandscapesVertices[i + half, j - half].Position.Y) / 4.0f +
                                                                            (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                            j),
                                                                    new Vector3(0, 1, 0),
                                                                    Color.Green);
                        }

                        // Left middle of square
                        if (i == 0) {
                            myLandscapesVertices[i, j + half] = new VertexPositionNormalColor(
                                                                    new Vector3(
                                                                        i, 
                                                                        (myLandscapesVertices[i, j].Position.Y + myLandscapesVertices[i, j + side].Position.Y +
                                                                        myLandscapesVertices[i + half, j + half].Position.Y) / 3.0f +
                                                                        (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                        j + half),
                                                                    new Vector3(0, 1, 0),
                                                                    Color.Green);
                        } else {
                            myLandscapesVertices[i, j + half] = new VertexPositionNormalColor(
                                                                    new Vector3(
                                                                        i,
                                                                        (myLandscapesVertices[i, j].Position.Y + myLandscapesVertices[i, j + side].Position.Y +
                                                                        myLandscapesVertices[i + half, j + half].Position.Y + myLandscapesVertices[i - half, j + half].Position.Y) / 4.0f +
                                                                        (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                        j + half),
                                                                    new Vector3(0, 1, 0),
                                                                    Color.Green);
                        }

                        // Right middle of square
                        if (i + side == n) {
                            myLandscapesVertices[i + side, j + half] = new VertexPositionNormalColor(
                                                                            new Vector3(
                                                                                i + side,
                                                                                (myLandscapesVertices[i + side, j].Position.Y + myLandscapesVertices[i + side, j + side].Position.Y +
                                                                                myLandscapesVertices[i + half, j + half].Position.Y) / 3.0f +
                                                                                (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                                j + half),
                                                                            new Vector3(0, 1, 0),
                                                                            Color.Green);
                        } else {
                            myLandscapesVertices[i + side, j + half] = new VertexPositionNormalColor(
                                                                            new Vector3(
                                                                                i + side,
                                                                                (myLandscapesVertices[i + side, j].Position.Y + myLandscapesVertices[i + side, j + side].Position.Y +
                                                                                myLandscapesVertices[i + half, j + half].Position.Y + myLandscapesVertices[i + side + half, j + half].Position.Y) / 4.0f +
                                                                                (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                                j + half),
                                                                            new Vector3(0, 1, 0),
                                                                            Color.Green);
                        }

                        // Middle top of square
                        if (j + side == n) {
                            myLandscapesVertices[i + half, j + side] = new VertexPositionNormalColor(
                                                                            new Vector3(
                                                                                i + half,
                                                                                (myLandscapesVertices[i, j + side].Position.Y + myLandscapesVertices[i + side, j + side].Position.Y +
                                                                                myLandscapesVertices[i + half, j + side - half].Position.Y) / 3.0f +
                                                                                (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                                j + side),
                                                                            new Vector3(0, 1, 0),
                                                                            Color.Green);
                        } else {
                            myLandscapesVertices[i + half, j + side] = new VertexPositionNormalColor(
                                                                            new Vector3(
                                                                                i + half,
                                                                                (myLandscapesVertices[i, j + side].Position.Y + myLandscapesVertices[i + side, j + side].Position.Y +
                                                                                myLandscapesVertices[i + half, j + side - half].Position.Y + myLandscapesVertices[i + half, j + side + half].Position.Y) / 4.0f +
                                                                                (float)(rng.Next(0, maxRng) - maxRng / 2) * (float)Math.Pow(jaggedness, power) * on,
                                                                                j + side),
                                                                            new Vector3(0, 1, 0),
                                                                            Color.Green);
                        }

                        
                    }
                    
                }
                // The relevant squares of side length 'side' have now been considered. Divide side by 2 to continue the algorithm
                power += 1.0;
                side /= 2; 
            }

            // Set color of terrain based on height Position.Y
            for (int j = 0; j < n + 1; j++) {
                for (int i = 0; i < n + 1; i++) {
                    if (myLandscapesVertices[i, j].Position.Y > 40) {
                        myLandscapesVertices[i, j].Color = Color.White; // White for snow
                    } else if (myLandscapesVertices[i, j].Position.Y > 35f) {
                        myLandscapesVertices[i, j].Color = Color.Gray; // Gray for rocky edge/'gray'ish snow
                    } else if (myLandscapesVertices[i, j].Position.Y > 30f) {
                        myLandscapesVertices[i, j].Color = Color.DarkSlateGray;
                    } else if (myLandscapesVertices[i, j].Position.Y > 20f) {
                        myLandscapesVertices[i, j].Color = Color.DarkGreen; // DarkGreen for grass
                    } else if (myLandscapesVertices[i, j].Position.Y > 15f) {
                        myLandscapesVertices[i, j].Color = Color.Green; // Green for grass
                    } else if (myLandscapesVertices[i, j].Position.Y > 10f) {
                        myLandscapesVertices[i, j].Color = Color.LightGreen; // LightGreen for more 'lush' grass
                    } else if (myLandscapesVertices[i, j].Position.Y > 5f) {
                        myLandscapesVertices[i, j].Color = Color.Brown; // Brown for soil
                    } else if (myLandscapesVertices[i, j].Position.Y > waterHeight) {
                        myLandscapesVertices[i, j].Color = Color.Yellow; // Yellow for sand
                    } else {
                        myLandscapesVertices[i, j].Color = Color.Coral; // Coral for coral
                    }
                }
            }

            // Properly set the normals for each vertex by summing the normals of each polygon adjacent to the vertex
            // Then normalize (which is not entirely necessary)
            for (int j = 0; j < n+1; j++) {
                for (int i = 0; i < n+1; i++) {
                    Vector3 current = myLandscapesVertices[i, j].Position;

                    if (i != 0 && j != 0 && i != n && j != n) {
                        // calculate the normals for the vertices of every point (a, b) of the grid where 1 <= a, b <= n-1
                        Vector3 normal = Vector3.Cross(myLandscapesVertices[i - 1, j].Position - current, myLandscapesVertices[i, j + 1].Position - current) +
                                         Vector3.Cross(myLandscapesVertices[i, j + 1].Position - current, myLandscapesVertices[i + 1, j].Position - current) +
                                         Vector3.Cross(myLandscapesVertices[i + 1, j].Position - current, myLandscapesVertices[i, j - 1].Position - current) +
                                         Vector3.Cross(myLandscapesVertices[i, j - 1].Position - current, myLandscapesVertices[i - 1, j].Position - current);
                        myLandscapesVertices[i, j].Normal = Vector3.Normalize(normal);
                    }
                    
                    // The rest of the if statements (below) calculate the normals for the vertices on the edge of the grid
                    if (j == 0) {
                        // Inputs the normals of the vertices along the bottom row of the grid
                        if (i == 0) {
                            // Grid point (0, 0)
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i, j + 1].Position - current, myLandscapesVertices[i + 1, j].Position - current));
                        } else if (i == n) {
                            // Grid point (n, 0)
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i - 1, j].Position - current, myLandscapesVertices[i, j + 1].Position - current));
                        } else {
                            // Grid points (i, 0) i.e. along bottom row of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i - 1, j].Position - current, myLandscapesVertices[i, j + 1].Position - current) +
                                                                Vector3.Cross(myLandscapesVertices[i, j + 1].Position - current, myLandscapesVertices[i + 1, j].Position - current));
                        }
                    }
                    if (j != n && j != 0 && (i == 0 || i == n)) {
                        if (i == 0) {
                            // Grid points (0, j) except (0, n) i.e. along left edge of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i, j + 1].Position - current, myLandscapesVertices[i + 1, j].Position - current) +
                                                                Vector3.Cross(myLandscapesVertices[i + 1, j].Position - current, myLandscapesVertices[i, j - 1].Position - current));
                        } else if (i == n) {
                            // Grid points (n, j) except (n, n) i.e. along right edge of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i - 1, j].Position - current, myLandscapesVertices[i, j + 1].Position - current) +
                                                                Vector3.Cross(myLandscapesVertices[i, j - 1].Position - current, myLandscapesVertices[i - 1, j].Position - current));
                        }
                    }
                    if (j == n) {
                        // Inputs the normals of the vertices along the top row of the grid
                        if (i == 0) {
                            // Grid point (0, n) i.e. top left point of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i + 1, j].Position - current, myLandscapesVertices[i, j - 1].Position - current));
                        } else if (i == n) {
                            // Grid point (n, n) i.e. top right point of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i, j - 1].Position - current, myLandscapesVertices[i - 1, j].Position - current));
                        } else {
                            // Grid point (i, n) i.e. along top edge of grid
                            myLandscapesVertices[i, j].Normal = Vector3.Normalize(Vector3.Cross(myLandscapesVertices[i, j - 1].Position - current, myLandscapesVertices[i - 1, j].Position - current) +
                                                                Vector3.Cross(myLandscapesVertices[i + 1, j].Position - current, myLandscapesVertices[i, j - 1].Position - current));
                        }
                    }


                }
            }
            /*
            for (int j = 0; j < n + 1; j++) {
                for (int i = 0; i < n + 1; i++) {
                    Console.WriteLine(myLandscapesVertices[i, j].Normal);
                }
            }*/
            
            // There are n^2 squares (of size 1x1) in our n x n grid and each square is going to be made into two triangles so we have n^2 * 2 triangles = 2n^2 triangles.
            //  ___
            // |\  |
            // | \ |
            // |__\|    

            // Each triangle has 3 points so 2n^2 triangles = 2n^2 * 3 points = 6n^2
            // We also want to render the water. The water plane will also have the same number of plane and same number of points so we have 6n^2 * 2 points = 12n^2
            // We also want to render the water so that it's seeable when we're submerged so we have 12n^2 + 6n^2 points = 18n^2 points.
            VertexPositionNormalColor[] anticlockwiseVertices = new VertexPositionNormalColor[18 * n * n];

            int indexTracker = 0;
            // Put the vertices into counter-clockwise form in tmp
            for (int iterZ = 0; iterZ < n; ++iterZ) {
                for (int iterX = 0; iterX < n; ++iterX) {
                    // One triangle of a square
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX, iterZ];
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX, iterZ + 1];
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX + 1, iterZ];

                    // The other triangle of the square
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX + 1, iterZ];
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX, iterZ + 1];
                    anticlockwiseVertices[indexTracker++] = myLandscapesVertices[iterX + 1, iterZ + 1];
                }
            }

            //Water
            Vector3 waterNormal = new Vector3(0, 1, 0);
            
            for (int iterZ = 0; iterZ < n; ++iterZ) {
                for (int iterX = 0; iterX < n; ++iterX) {
                    // One triangle of a square
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ)), waterNormal, waterColor);
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ + 1)), waterNormal, waterColor);// myLandscapesVertices[iterX, iterZ + 1];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ)), waterNormal, waterColor); //myLandscapesVertices[iterX + 1, iterZ];

                    // The other triangle of the square
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ)), waterNormal, waterColor); // myLandscapesVertices[iterX + 1, iterZ];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ + 1)), waterNormal, waterColor); // myLandscapesVertices[iterX, iterZ + 1];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ + 1)), waterNormal, waterColor); // myLandscapesVertices[iterX + 1, iterZ + 1];
                }
            }

            // To see water while submerged, we need to draw the water plane by inputting vertices of triangle in reverse direction (because otherwise backface culling means the water 'plane' won't be rendered)
            for (int iterZ = 0; iterZ < n; ++iterZ) {
                for (int iterX = 0; iterX < n; ++iterX) {
                    // One triangle of a square
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ)), -waterNormal, waterColor);
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ)), -waterNormal, waterColor); //myLandscapesVertices[iterX + 1, iterZ];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ + 1)), -waterNormal, waterColor);// myLandscapesVertices[iterX, iterZ + 1];
                    
                    // The other triangle of the square
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ)), -waterNormal, waterColor); // myLandscapesVertices[iterX + 1, iterZ];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX + 1), waterHeight, (float)(iterZ + 1)), -waterNormal, waterColor); // myLandscapesVertices[iterX + 1, iterZ + 1];
                    anticlockwiseVertices[indexTracker++] = new VertexPositionNormalColor(new Vector3((float)(iterX), waterHeight, (float)(iterZ + 1)), -waterNormal, waterColor); // myLandscapesVertices[iterX, iterZ + 1];  
                }
            }
            // End water

            vertices = Buffer.Vertex.New(game.GraphicsDevice, anticlockwiseVertices);

            basicEffect = new BasicEffect(game.GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.LookAtLH(new Vector3(0, 0, 0), new Vector3(0, 0, 1), Vector3.UnitY),
                // Reminder: zNear 0.1f, zFar was set at 100 
                Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 600.0f),
                World = Matrix.Identity,
            };
            
            // Turn on lighting
            basicEffect.LightingEnabled = true;

            // Misc.
            //basicEffect.DirectionalLight0.Direction = new Vector3(0, -1, 0);
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight2.Enabled = false;

            // Sun light
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f); // higher absolute numbers leads to higher intensity light
            basicEffect.DirectionalLight0.SpecularColor = Color.LightYellow.ToVector3();
            basicEffect.SpecularPower = 50f; // Lower specular power leads to lighting looking quite unrealistic
 
            // Ambient Light
            basicEffect.AmbientLightColor = Vector3.Multiply(new Vector3(0.01f, 0.01f, 0.01f), ambientLightIntensity); // I use this for lighting when the 'sun' has gone 'down'

            // Directional light
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.1f, 0.1f, 0.1f);
            basicEffect.DirectionalLight1.Direction = new Vector3(-1, -1, -1);
            // End light

            inputLayout = VertexInputLayout.FromBuffer(0, vertices);

            // Make sure we start at a point above ground
            float minCameraStartingYPosition = Math.Max(myLandscapesVertices[n / 2, n / 2].Position.Y, waterHeight);

            // initialCameraPosition never changes throughout the program
            // what changes is the cameraOffset.
            // So, the position of the camera at any point in time is initialCameraPosition + cameraOffset.
            // cameraDirection is which way we're looking at from the point of view of a person standing at point (0, 0, 0)
            initialCameraPosition = new Vector3(n / 2, minCameraStartingYPosition + heightAboveGround, n / 2);
            cameraDirection = new Vector3(0, 0, 1);
            cameraOffset = Vector3.Zero;

            this.game = game;
            camera = new Camera(game, initialCameraPosition, cameraDirection, speed, heightAboveGround, myLandscapesVertices, gravityStrength);
        }

        public override void Update(GameTime gameTime)
        {
            //var time = gameTime.ElapsedGameTime.Milliseconds;
            // This is the sun for the game
            basicEffect.DirectionalLight0.Direction = Vector3.Transform(new Vector3(1, 0, 0), Quaternion.RotationAxis(new Vector3(0, 0, 1), ((float)gameTime.TotalGameTime.TotalMilliseconds + 10000) / 4000));

            // Changed zFar from 100 to 600
            basicEffect.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)game.GraphicsDevice.BackBuffer.Width / game.GraphicsDevice.BackBuffer.Height, 0.1f, 600.0f);

            camera.Update(gameTime, basicEffect);
        }

        public override void Draw(GameTime gameTime)
        {           
            // Setup the vertices
            game.GraphicsDevice.SetVertexBuffer(vertices);
            game.GraphicsDevice.SetVertexInputLayout(inputLayout);
            game.GraphicsDevice.SetBlendState(game.GraphicsDevice.BlendStates.AlphaBlend);
            // Apply the basic effect technique and draw the rotating cube
            basicEffect.CurrentTechnique.Passes[0].Apply();
            game.GraphicsDevice.Draw(PrimitiveType.TriangleList, vertices.ElementCount);

           
        }
    }
}
