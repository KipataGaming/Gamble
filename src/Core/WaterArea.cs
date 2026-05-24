using Godot;
using System;
using Game.Core;
using Game.Bridge;

public partial class WaterArea : Node3D
{
    public override void _Ready()
    {
        Area3D area = null;
        foreach (Node child in GetChildren())
        {
            if (child is Area3D a)
            {
                area = a;
                break;
            }
        }

        if (area != null)
        {
            area.Monitoring = true;
            area.Connect(Area3D.SignalName.BodyEntered, Callable.From<Node3D>(OnBodyEntered));
            area.Connect(Area3D.SignalName.BodyExited, Callable.From<Node3D>(OnBodyExited));
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        // Walk up parents to find the PlayerController
        Node current = body;
        while (current != null)
        {
            if (current is PlayerController player)
            {
                player.SetInWater(true);
                GD.Print("[Water] Player entered water (swimming enabled)");
                return;
            }
            current = current.GetParent();
        }
    }

    private void OnBodyExited(Node3D body)
    {
        Node current = body;
        while (current != null)
        {
            if (current is PlayerController player)
            {
                player.SetInWater(false);
                GD.Print("[Water] Player exited water");
                return;
            }
            current = current.GetParent();
        }
    }
}