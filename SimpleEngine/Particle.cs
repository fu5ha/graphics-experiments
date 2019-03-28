using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace SimpleEngine
{
    public class Particle
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Acceleration { get; set; }
        public float Age { get; set; }
        public float MaxAge { get; set; }
        public Vector3 Color { get; set; }
        public float Size { get; set; }
        public float SizeVelocity { get; set; }
        public float SizeAcceleration { get; set; }
        public Particle() { Age = -1; }
        public bool Update(float ElapsedGameTime)
        {
            if (Age < 0) return false;
            Velocity += Acceleration * ElapsedGameTime;
            Position += Velocity * ElapsedGameTime;
            SizeVelocity += SizeAcceleration * ElapsedGameTime;
            Size += SizeVelocity * ElapsedGameTime;
            Age += ElapsedGameTime;
            if (Age > MaxAge)
            {
                Age = -1;
                return false;
            }
            return true;
        }
        public bool IsActive() { return Age < 0 ? false : true; }
        public void Activate() { Age = 0; }
        public void Init()
        {
            Age = 0; Size = 1; SizeVelocity = SizeAcceleration = 0;
        }
    }
}
