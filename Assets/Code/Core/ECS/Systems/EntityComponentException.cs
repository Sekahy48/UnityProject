using System;

namespace ECS.Systems
{
    public class EntityComponentException : Exception
    {
        public EntityComponentException(string msg) : base(msg)
        {
        }
    }
} 