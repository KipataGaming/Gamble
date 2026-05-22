using System;

namespace Game.Core
{
    public enum ResourceState { Alive, Destroyed }

    public class ResourceData
    {
        public int Health { get; private set; }
        public ResourceState State { get; private set; }

        public ResourceData(int maxHealth)
        {
            Health = maxHealth;
            State = ResourceState.Alive;
        }

        public bool TryHit(int damage)
        {
            if (State == ResourceState.Destroyed) return false;

            Health -= damage;
            if (Health <= 0)
            {
                Health = 0;
                State = ResourceState.Destroyed;
            }
            
            return true;
        }
    }
}