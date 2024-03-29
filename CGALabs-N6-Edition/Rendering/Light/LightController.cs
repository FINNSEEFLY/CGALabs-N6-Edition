﻿using System.Numerics;

namespace CGALabs_N6_Edition.Rendering.Light
{
    public class LightController
    {
        public Vector3 LightSourcePosition { get; private set; }

        private const float Sensitivity = 0.1f;

        public LightController()
        {
            LightSourcePosition = new Vector3(0, 500, 0);
        }

        public void RotateY(int xOffset)
        {
            LightSourcePosition = Vector3.Transform(LightSourcePosition, Matrix4x4.CreateRotationY(Sensitivity * -xOffset));
        }

        public void RotateX(int yOffset)
        {
            LightSourcePosition = Vector3.Transform(LightSourcePosition, Matrix4x4.CreateRotationX(Sensitivity * yOffset));
        }
    }
}
