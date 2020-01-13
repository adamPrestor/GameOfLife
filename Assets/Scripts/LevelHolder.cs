using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHolder : MonoBehaviour
{
  private Level newLevel;
  private List<Level> levels = new List<Level>();

  private int LevelIndex = 0;

  private void Start()
  {
    // level 1
    newLevel = new Level(9, 50, true);
    newLevel.AddBall(new Vector3Int(4, 4, 4), 7);
    levels.Add(newLevel);

    // level 2 
    newLevel = new Level(15, 30, true);
    newLevel.AddSquare(new Vector3Int(3, 1, 3), 3);
    levels.Add(newLevel);

    // level 3
    newLevel = new Level(18, 30, true);
    newLevel.AddEmptyBall(new Vector3Int(3, 3, 3), 7);
    levels.Add(newLevel);

    // level 4
    newLevel = new Level(14, 40, true);
    newLevel.AddCylinder(new Vector3Int(3, 0, 3), Vector3Int.up, 5, 4);
    levels.Add(newLevel);

    // level 5
    newLevel = new Level(12, 40, true);
    newLevel.AddEmptyCylinder(new Vector3Int(3, 0, 3), Vector3Int.up, 5, 8);
    levels.Add(newLevel);

    // level 6
    newLevel = new Level(7, 30, true);
    newLevel.AddCylinder(new Vector3Int(2, 0, 3), Vector3Int.up + Vector3Int.right * 2, 5, 17);
    levels.Add(newLevel);

    // level 7
    newLevel = new Level(9, 40, true);
    newLevel.AddEmptyQuad(new Vector3Int(4, 4, 4), Vector3Int.one * 7);
    newLevel.RemoveQuad(new Vector3Int(4, 4, 4), new Vector3Int(3, 3, 9));
    levels.Add(newLevel);

    // level 8
    newLevel = new Level(9, 40, true);
    newLevel.AddBall(new Vector3Int(4, 4, 4), 9);
    newLevel.RemoveSquare(new Vector3Int(4, 7, 7), 4);
    levels.Add(newLevel);

  }

  public Level GetLevel()
  {
    if (LevelIndex >= levels.Count)
      return null;
    newLevel = levels[LevelIndex];
    LevelIndex++;
    return newLevel;
  }
}
