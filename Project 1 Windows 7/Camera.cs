using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

// Own
using SharpDX.Toolkit.Input;
using SharpDX.Toolkit.Graphics; // for Basic Effect
// End own

namespace Project1
{
    public class Camera
    {
        private MouseState prevMouseState, currentMouseState;

        private Vector3 initialCameraPosition;
        private Vector3 cameraDirection;
        private Vector3 cameraOffset;
        private Project1Game game;
        public Matrix ViewMatrix { get ; private set; }
        private float heightAboveGround;
        private VertexPositionNormalColor[,] myLandscapeVertices;
        private bool gravity = false;
        private Vector3 upVector;
        private float maxSlope = 2f;
        private float gravityStrength;
        private float jetpackStrength;

        private float speed;

        public Vector3 getInitialCameraPosition()
        {
            return initialCameraPosition;
        }

        public Vector3 getCameraDirection()
        {
            return cameraDirection;
        }

        public Vector3 getCameraOffset()
        {
            return cameraOffset;
        }

        public Camera(Project1Game game, Vector3 initialCameraPosition, Vector3 cameraDirection, float speed, float heightAboveGround, VertexPositionNormalColor[,] myLandscapeVertices, float gravityStrength)
        {
            this.initialCameraPosition = initialCameraPosition;
            this.cameraDirection = cameraDirection;
            this.game = game;
            this.speed = speed;
            this.heightAboveGround = heightAboveGround;
            this.myLandscapeVertices = myLandscapeVertices;
            this.upVector = new Vector3(0, 1, 0);
            prevMouseState = game.mouseManager.GetState();
            this.gravityStrength = gravityStrength;
            this.jetpackStrength = gravityStrength * 1.5f;
        }

        public void Update(GameTime gameTime, BasicEffect basicEffect)
        {
            var time = gameTime.ElapsedGameTime.Milliseconds;

            Vector3 originalCameraOffset = cameraOffset;

            if (game.keyboardState.IsKeyDown(Keys.Left)) {
                cameraDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(new Vector3(0, 1, 0), -0.02f));
                cameraDirection = Vector3.Normalize(cameraDirection);
            }
            if (game.keyboardState.IsKeyDown(Keys.Right)) {
                cameraDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(new Vector3(0, 1, 0), 0.02f));
                cameraDirection = Vector3.Normalize(cameraDirection);
            }
            if (game.keyboardState.IsKeyDown(Keys.Up)) {
                cameraDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(new Vector3(1, 0, 0), -0.02f));
                cameraDirection = Vector3.Normalize(cameraDirection);
            }
            if (game.keyboardState.IsKeyDown(Keys.Down)) {
                cameraDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(new Vector3(1, 0, 0), 0.02f));
                cameraDirection = Vector3.Normalize(cameraDirection);
            }
            if (game.keyboardState.IsKeyDown(Keys.W)) {
                cameraOffset += Vector3.Multiply(cameraDirection, 0.012f * time * speed);
            }
            if (game.keyboardState.IsKeyDown(Keys.S)) {
                cameraOffset += Vector3.Multiply(cameraDirection, -0.012f * time * speed);
            }
            if (game.keyboardState.IsKeyDown(Keys.A)) {
                cameraOffset += Vector3.Multiply(Vector3.Cross(cameraDirection, Vector3.UnitY), 0.012f * time * speed);
            }
            if (game.keyboardState.IsKeyDown(Keys.D)) {
                cameraOffset += Vector3.Multiply(Vector3.Cross(cameraDirection, Vector3.UnitY), -0.012f * time * speed);
            }
            // The key 'Control' is used to go down (I don't know what game mechanic
            // name I can call this but I leave it in the project so that one can mvoe around the landscape using such a feature if needed).

            if (game.keyboardState.IsKeyDown(Keys.Control)) {
                cameraOffset += Vector3.Multiply(Vector3.UnitY, -0.012f * time * speed);
            }

            // Gravity
            // Press L to turn on fake gravity (i.e. no acceleration, just a constant decrease in height)
            // Press K to turn off fake gravity
            if (game.keyboardState.IsKeyDown(Keys.K)) {
                gravity = false;
            }
            if (game.keyboardState.IsKeyDown(Keys.L)) {
                gravity = true;
            }
            if (gravity) {
                cameraOffset.Y += -0.02f * time * gravityStrength;
            }

            // The following snippet of code is used for rotating the view with the mouse
            currentMouseState = game.mouseManager.GetState();
            if (currentMouseState != prevMouseState) {
                float deltaX = currentMouseState.X - (0.5f);
                float deltaY = currentMouseState.Y - (0.5f);

                // Rotation of view across XZ plane
                cameraDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(new Vector3(0, 1, 0), deltaX));

                // I want Rotation Axis to be the right of camera direction, I get this vector by taking the cross product of UnitY and cameraDirection
                // Rotation of view across YZ plane
                // I want to restrict how far up and how far down the player can look and so I test the rotation to see if it would go too far
                // If the rotation doesn't go too far up or down then I change the cameraDirection
                // If the rotation goes too far then nothing is done.
                Vector3 testUpDownDirection = Vector3.Transform(cameraDirection, Quaternion.RotationAxis(Vector3.Cross(Vector3.UnitY, cameraDirection), deltaY));
                if (Vector3.Dot(new Vector3(testUpDownDirection.X, 0f, testUpDownDirection.Z), testUpDownDirection) >= 0.01f) {
                    cameraDirection = testUpDownDirection;
                    cameraDirection = Vector3.Normalize(cameraDirection);
                }
            }

            int n = myLandscapeVertices.GetLength(0) - 1;
            // Out of bounds check
            // I don't allow for going right to the very edge of the map. So there's a 1 unit 'frame' around the edge of the square [0, 0] to [n, n]
            // which means the player can only walk on the XZ square formed by [1, 1] and [n - 1, n - 1]
            // -----Check for out of bounds in X axis
            if (initialCameraPosition.X + cameraOffset.X < 1) {
                cameraOffset.X = 1f - initialCameraPosition.X;
            } else if (initialCameraPosition.X + cameraOffset.X > (float)(n - 1)) {
                cameraOffset.X = (float)(n - 1) - initialCameraPosition.X;
            }
            // -----Check for out of bounds in Z axis
            if (initialCameraPosition.Z + cameraOffset.Z < 0) {
                cameraOffset.Z = 1f - initialCameraPosition.Z;
            } else if (initialCameraPosition.Z + cameraOffset.Z > (float)(n - 1)) {
                cameraOffset.Z = (float)(n - 1) - initialCameraPosition.Z;
            }
            // End out of bounds check

            // Collision check        
            int lowerX = (int)(initialCameraPosition.X + cameraOffset.X / 1);
            int lowerZ = (int)(initialCameraPosition.Z + cameraOffset.Z / 1);
            float xMod1 = (float)(initialCameraPosition.X % 1.0);
            float yMod1 = (float)(initialCameraPosition.Y % 1.0);

            // Explanation of how I calculate the y value position
            // When I'm at position (x, z), I might not have a y value at (x, z) because x or z might not both be integers.
            // Therfore, I find the height at (floor(x), floor(z)) which I do have from the array myLandscapeVertices.
            // Let the height at (floor(x), floor(z)) be y.
            // Then the height at (x, z) = y + (some fraction * the slope of the height from (floor(x), floor(z)) to (ceil(x), floor(z)) +
            //                                 (some fraction * the slope of the height from (floor(x), floor(z)) to (floor(x), ceil(z))
            // This can be thought of as an interpolation.
            float yValueOfLandscape = myLandscapeVertices[lowerX, lowerZ].Position.Y +
                                    (float)(myLandscapeVertices[lowerX, lowerZ + 1].Position.Y - myLandscapeVertices[lowerX, lowerZ].Position.Y) * xMod1 +
                                    (float)(myLandscapeVertices[lowerX + 1, lowerZ + 1].Position.Y - myLandscapeVertices[lowerX, lowerZ + 1].Position.Y) * yMod1;

            // If our current height (initialCameraPosition.Y + cameraOffset.Y) is less than the height we calculated above,
            // then we need to change it to reflect the height we should actually be at
            // but if the 'slope' is too high then we don't move (this is to prevent players 'teleporting' up to a high sloped peak)
            if (initialCameraPosition.Y + cameraOffset.Y < yValueOfLandscape + heightAboveGround) {
                if ((yValueOfLandscape + heightAboveGround - (initialCameraPosition.Y + cameraOffset.Y)) < maxSlope) {
                    cameraOffset.Y = yValueOfLandscape + heightAboveGround - (initialCameraPosition.Y); 
                } else {
                    cameraOffset = originalCameraOffset;
                }
            }
            // End collision check

            // We can now take care of the jetpack (used by pressing space). 
            if (game.keyboardState.IsKeyDown(Keys.Space)) {
                cameraOffset += Vector3.Multiply(Vector3.UnitY, 0.012f * time * speed * jetpackStrength);
            }

            basicEffect.View = Matrix.LookAtLH(initialCameraPosition + cameraOffset, initialCameraPosition + cameraOffset + cameraDirection, new Vector3(0, 1, 0));

            // Set the mouse to the middle of the window and set prevMouseState to the currentMouseState
            game.mouseManager.SetPosition(new Vector2(0.5f, 0.5f));
            prevMouseState = currentMouseState;
        }
    }
}