----------------Perspective----------------
I've altered the Projection matrix to display vertices even further away.
The code came with 'zFar' set to 100.0f but I've changed it to 600.0f.

----------------Controls----------------
Standard WASD movement with mouse rotation.
Spacebar for 'jetpack' (straight up, irrespective of orientation)
Control to go down (straight towards the ground, irrespective of orientation)
L to turn on 'gravity'
K to turn off 'gravity'

----------------Collision----------------
The user/player is prohibited from moving underground.
I've also made it so that if the player is moving along ground and encounters a vertex that's too high, then the player is unable to move there.
If this makes it hard to move around then it can easily be gotten around by using the 'jetpack' (spacebar) to go over it.

----------------Lighting----------------
The sun lighting has been implemented (you'll have to wait at most 15 seconds to see the light rise up).
There's diffuse lighting from 'Lighting0'.
There's specular lighting and so you'll see things being shiny.
There's ambient lighting. This allows one to see the landscape when the sun is 'down'

----------------Implementation (how the landscape is generated)----------------
I create an n+1 by n+1 array named myLandscapeVertices of VertexPositionNormalColors, where n is some positive power of 2. 

I have put n at 512 in my code. 

For any cell [i, j] of the array, myLandscapeVertices[i, j] = new VertexPositionNormalColor(new Vector3(i, calculated_y_height_from_diamond_square, j), normal, color). So the cell [i, j] contains the properties at x = i, z = j. 

I set the corners (the corners are [0, 0]; [0, n]; [n, 0]; [n, n]) to an initial height of 35. 

I then calculate the y value of the midpoint which I calculate as the mean of the y values at the corners PLUS (a random number between -maxRng/2 and maxRng/2 multiplied by 'jaggedness'^(power)). (maxRng is currently set at 120.) The variable 'jaggedness' is set at 0.47f and on the very first iteration 'power' is set at 0. Which means jaggedness^power = 1 on the first iteration. I increment power by 1 for each iteration and so this will have the effect of reducing the random number generated. Also, I make the point [n/2, n/2] have a y value of AT LEAST 37 so that at least some snow (coloured white) is shown. This is left there for the sake of showing that there is a snow coloured height in the code.

I then calculate the y values of the midpoint edges of the square. (The midpoint edges ARE NOT the point where the two diagonals of the square meet; it's the middle of the edge of the square.) So that means for [n/2, 0] (the middle bottom point of the square, middle bottom from the point of view of an observer looking down on the XZ plane), the y value is the mean of the y value at [0, 0] and the y value at [n, 0] + some random number from -maxRng to maxRng * jaggedness^power, where power is still 0 for the first iteration (i.e. the power has NOT yet been incremented).

Some explanations of the diamond square algorithm say that you can do some 'wrapping' when calculating these midpoint edges of the square; I don't do any wrapping. 

After all the y values of the midpoint edges of the other squares have been calculated, I then do some preparation for the next iteration: I increment power by 1 (which will have the effect of reducing the random number generated) and I also divide 'side' by 2. My implementation of the algorithm terminates when 'side' Side represents the side length of the square(s) whose midpoints are being considered. Once side <= 1, the iteration (and thus the algorithm) terminates.


After this, I loop through myLandscapeVertices to change the color of the vertex, depending on the y value. 

I then calculate the normals for each vertex by summing the normals of the triangles that the vertex is a part of. I then normalize the summation of the normals and set this normalized vector as the normal of the vertex.

After this is done, I create an array called anticlockwiseVertices of type VertexPositionNormalColor and of size 18n^2. 

18n^2 because there are n^2 squares (of size 1x1) in our n x n array. Each of these squares can be decomposed into 2 triangles and so there are 2n^2 triangles. Each of these triangles has 3 points so 2n^2 * 3 = 6n^2 points. So 6n^2 of these will be used to create the triangles that form the landscape.

I then also have water, which is a plane of size n^2. So this plane will also have 6n^2 points. 

I also want to be able to see the water while submerged, so I also create a plane in the opposite direction of the other water plane (due to backface culling). This will also have 6n^2 points.

So in total, there will be three 6n^2 points giving us 18n^2.

I input the VertexPositionNormalColor's in myLandscapeVertices into the array anticlockwiseVertices in the order so that our landscape can be seen from above (rather than from below). This fills up the first 6n^2 buckets of array anticlockwiseVertices.

I then fill up the next 6n^2 buckets of array anticlockwiseVertices with the upward facing water. This water has a normal of (0, 1, 0).

I then fill up the final 6n^2 bucks of array anticlockwiseVertices with the downward facing water. This water has a normal of (0, -1, 0).

I then assign anticlockwiseVertices to variable 'vertices'.



