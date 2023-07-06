using System;
using System.Collections.Generic;
using UnityEngine;
using pair = System.Tuple<int, int>;

public static class PathGenerator
{
    public static readonly pair[] neighbors =
    {
        Tuple.Create(1, 0),
        Tuple.Create(-1, 0),
        Tuple.Create(0, 1),
        Tuple.Create(0, -1)
    };

    public static bool IsValidNeighbor(int n, int m, pair neighbor)
    {
        return neighbor.Item1 >= 0 && neighbor.Item1 < n && neighbor.Item2 >= 0 && neighbor.Item2 < m;
    }

    private static void DFS(Cell[,] maze, pair currentCell, pair goalCell, Stack<pair> path, ref bool pathFound, List<pair> removed)
    {
        if (currentCell == goalCell || !maze[goalCell.Item1, goalCell.Item2].IsWall)
        {
            pathFound = true;
            maze[goalCell.Item1, goalCell.Item2].IsWall = false;
            return;
        }

        maze[currentCell.Item1, currentCell.Item2].IsWall = false;
        path.Push(currentCell);
        int n = maze.GetLength(0);
        int m = maze.GetLength(1);

        pair[] currentNeighbors = (pair[])neighbors.Clone();
        Randomizer.Randomize(currentNeighbors);
        foreach (var neighborToAdd in currentNeighbors)
        {
            var neighbor = Tuple.Create(currentCell.Item1 + neighborToAdd.Item1, currentCell.Item2 + neighborToAdd.Item2);
            if (IsValidNeighbor(n, m, neighbor) && maze[neighbor.Item1, neighbor.Item2].IsWall)
            {
                DFS(maze, neighbor, goalCell, path, ref pathFound, removed);
            }
        }

        if (!pathFound && !(path.Peek().Equals(goalCell)))
        {
            var p = path.Pop();
            removed.Add(p);
        }
    }

    public static Tuple<Cell[,], Stack<pair>> GenerateMaze(pair size, pair startCell, pair goalCell, float noisePercent)
    {
        bool pathFound = false;
        Cell[,] maze = null;
        Stack<pair> path = new Stack<pair>();
        while (pathFound == false && path.Count <= 2)
        {
            maze = new Cell[size.Item1, size.Item2];
            path = new Stack<pair>();
            int randomWalls = (int)(size.Item1 * size.Item2 * noisePercent);
            for (int i = 0; i < maze.GetLength(0); i++)
            {
                for (int j = 0; j < maze.GetLength(1); j++)
                {
                    maze[i, j] = new Cell();
                }
            }

            System.Random r = new System.Random();
            List<pair> randoms = new List<pair>();
            for (int i = 0; i < randomWalls; i++)
            {
                int x = r.Next(2, maze.GetLength(0) - 1);
                int y = r.Next(2, maze.GetLength(1) - 1);
                var tmp = Tuple.Create(x, y);
                if (tmp != startCell && tmp != goalCell)
                {
                    randoms.Add(Tuple.Create(x, y));
                    maze[x, y].IsWall = false;
                }
            }

            maze[startCell.Item1, startCell.Item2].IsWall = true;
            maze[goalCell.Item1, goalCell.Item2].IsWall = true;
            List<pair> removed = new List<pair>();
            DFS(maze, startCell, goalCell, path, ref pathFound, removed);

            // Debug.Log($"Path found: {pathFound}");

            foreach (var item in randoms)
            {
                maze[item.Item1, item.Item2].IsWall = true;
            }

            foreach (var item in removed)
            {
                maze[item.Item1, item.Item2].IsWall = true;
            }
        }

        return Tuple.Create(maze, path);
    }
}