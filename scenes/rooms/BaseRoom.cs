using Godot;
using System;
using System.Collections.Generic;

public partial class BaseRoom : Node3D
{
    public Node3D Enemies;
    public override void _Ready()
    {
        base._Ready();

        Enemies = GetNode<Node3D>("%Enemies");

        FillCeilings();
        PrepEnemies();
    }

    public void FillCeilings()
    {
        var floors = GetNode<GridMap>("%Floors");
        var ceilings = GetNode<GridMap>("%Ceilings");
        var cellIdsWithNoCeiling = new HashSet<int>();
        foreach (var cellName in new List<string> { "Ground", "Hole-Corner", "Hole-Side", "Hole-UTurn" })
        {
            cellIdsWithNoCeiling.Add(floors.MeshLibrary.FindItemByName(cellName));
        }
        var usedCells = floors.GetUsedCells();
        foreach (var cellCoords in usedCells)
        {
            var tileId = floors.GetCellItem(cellCoords);
            if (cellIdsWithNoCeiling.Contains(tileId))
            {
                ceilings.SetCellItem(cellCoords, 0);
            }
        }
    }

    public void PrepEnemies()
    {
        HashSet<Enemy> enemies = [];
        foreach (var node in Enemies.GetChildren())
        {
            if (node is Enemy enemy)
            {
                foreach (var otherEnemy in enemies)
                {
                    otherEnemy.Screamed += enemy.OnScreamHeard;
                    enemy.Screamed += otherEnemy.OnScreamHeard;
                }
                enemies.Add(enemy);
            }
        }
    }
}
