using System;
using Core.ECS.Systems;

namespace Core.ECS
{
    public class EntityComponentException : Exception
    {
        public EntityComponentException(string msg) : base(msg)
        {
        }
    }
} 