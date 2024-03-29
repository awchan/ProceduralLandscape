﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;

namespace Project1
{
    using SharpDX.Toolkit.Graphics;
    abstract public class GameObject
    {
        public BasicEffect basicEffect;
        public VertexInputLayout inputLayout;
        public Game game;

        // Own
        // public Project1Game game;
        // End my own

        public abstract void Update(GameTime gametime);
        public abstract void Draw(GameTime gametime);
    }
}
