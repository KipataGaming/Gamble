using Godot;
using System.Collections.Generic;

namespace Game.Core;

public partial class CombatManager : Node
{
    public static CombatManager Instance { get; private set; } = null!;

    // The mathematical state of all active entities: EntityID -> Current Health
    private readonly Dictionary<string, float> _entityHealth = new();

    public override void _EnterTree()
    {
        Instance = this;
        
        // Mocking some starting data for testing
        _entityHealth["player"] = 100.0f;
        _entityHealth["zombie_01"] = 50.0f;
    }

    public override void _Ready()
    {
        // Subscribe to the Event Broker (The Senses)
        EventBroker.OnEntityDamaged += HandleEntityDamaged;
    }

    public override void _ExitTree()
    {
        // Professional Standard: Always unsubscribe to prevent memory leaks when the game closes
        EventBroker.OnEntityDamaged -= HandleEntityDamaged;
    }

    private void HandleEntityDamaged(string entityId, float damage)
    {
        // If we aren't tracking this entity, ignore it
        if (!_entityHealth.ContainsKey(entityId)) return;

        // 1. The Math
        // In the future, you'd check InventoryManager here for armor stats to reduce this damage
        _entityHealth[entityId] -= damage;

        GD.Print($"[Combat] {entityId} took {damage} damage. Health remaining: {_entityHealth[entityId]}");

        // 2. The Result
        if (_entityHealth[entityId] <= 0)
        {
            GD.Print($"[Combat] {entityId} has been defeated!");
            
            // Shout back into the void that something died. 
            // The AchievementManager will hear this and update the "Enemy Slayer" progress!
            EventBroker.TriggerEnemyDefeated(entityId);
            
            // Clean up the memory
            _entityHealth.Remove(entityId);
        }
    }
}