using System;
#nullable enable
using Godot;
namespace Game.Core;

/// <summary>
/// Centralized Pub/Sub system. 
/// Systems publish events here, and other systems subscribe.
/// </summary>
public static class EventBroker
{
    // --------------------------------------------------------
    // COMBAT EVENTS
    // --------------------------------------------------------
    
    // The Event Definition (What subscribers listen to)
    public static event Action<string, float>? OnEntityDamaged;
    
    // The Trigger Method (What publishers call)
    public static void TriggerEntityDamaged(string entityId, float damageAmount) 
    {
        OnEntityDamaged?.Invoke(entityId, damageAmount);
    }

    // --------------------------------------------------------
    // INVENTORY / ACHIEVEMENT EVENTS
    // --------------------------------------------------------
    
    public static event Action<string, int>? OnItemCollected;
    
    public static void TriggerItemCollected(string itemId, int quantity)
    {
        OnItemCollected?.Invoke(itemId, quantity);
    }

    public static event Action<string>? OnEnemyDefeated;
    
    public static void TriggerEnemyDefeated(string enemyId)
    {
        OnEnemyDefeated?.Invoke(enemyId);
    }
}