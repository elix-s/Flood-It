using UnityEngine;
using System.Collections.Generic;

public static class FloodFillAlgorithm
{
    public static void PerformFloodFill(Tile[,] grid, Vector2Int startPos, Color targetColor)
    {
        int gridSizeX = grid.GetLength(0);
        int gridSizeY = grid.GetLength(1);
        
        if (!IsValidCoordinate(startPos, gridSizeX, gridSizeY))
        {
            Debug.LogError($"[FloodFillAlgorithm] Invalid start position: {startPos}");
            return;
        }

        Tile startTile = grid[startPos.x, startPos.y];
        
        if (startTile == null)
        {
             Debug.LogError($"[FloodFillAlgorithm] Tile at start position {startPos} is null.");
             return;
        }

        Color originalColor = startTile.TileColor; 
        
        if (originalColor == targetColor)
        {
            return;
        }
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(startPos);
        visited.Add(startPos);

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            Tile currentTile = grid[currentPos.x, currentPos.y];
            
            if (currentTile != null)
            {
                 currentTile.SetColor(targetColor);
            }
            else
            {
                Debug.LogWarning($"[FloodFillAlgorithm] Found null tile at {currentPos} during fill.");
                continue; 
            }
            
            Vector2Int[] neighbors = new Vector2Int[] {
                new Vector2Int(currentPos.x, currentPos.y + 1), 
                new Vector2Int(currentPos.x, currentPos.y - 1), 
                new Vector2Int(currentPos.x - 1, currentPos.y), 
                new Vector2Int(currentPos.x + 1, currentPos.y)  
            };
            
            foreach (Vector2Int neighborPos in neighbors)
            {
                if (IsValidCoordinate(neighborPos, gridSizeX, gridSizeY))
                {
                    Tile neighborTile = grid[neighborPos.x, neighborPos.y];
                    
                    if (neighborTile != null &&
                        !visited.Contains(neighborPos) &&
                        neighborTile.TileColor == originalColor) 
                    {
                        visited.Add(neighborPos);        
                        queue.Enqueue(neighborPos);       
                    }
                }
            }
        }
    }
    
    private static bool IsValidCoordinate(Vector2Int coord, int gridSizeX, int gridSizeY)
    {
        return coord.x >= 0 && coord.x < gridSizeX && coord.y >= 0 && coord.y < gridSizeY;
    }
}