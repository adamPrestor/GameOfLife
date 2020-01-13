using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level
{
  public int GameSize;
  public int MovesToComplete = 10; // moves neccessery to complete a level
  public bool SetRules = true; // setting the game mode
  public Vector3Int[] cells; // level map

  public int PopulationLowerBound = 3;
  public int PopulationUpperBound = 3;
  public int BirthLowerBound = 3;
  public int BirthUpperBound = 3;

  public bool[,,] Table { get; }

  // limitations - if neccessery - TODO

  // init
  public Level(int GS, int MTC, bool SR)
  {
    this.GameSize = GS;
    this.MovesToComplete = MTC;
    this.SetRules = SR;
    this.Table = new bool[GS, GS, GS];
  }
  
  public void SetLevelBounds(int PLB, int PUB, int BLB, int BUB)
  {
    this.PopulationLowerBound = PLB;
    this.PopulationUpperBound = PUB;
    this.BirthLowerBound = BLB;
    this.BirthUpperBound = BUB;
  }

  #region public invironment method

  #region add methods
  public void AddPoint(Vector3Int position) { Point(position); }
  public void AddSquare(Vector3Int center, int side)
  {
    AddQuad(center, Vector3Int.one * side);
  }

  public void AddQuad(Vector3Int center, Vector3Int sides)
  {
    Quad(center, sides);
  }

  public void AddEmptyQuad(Vector3Int center, Vector3Int sides)
  {
    AddQuad(center, sides);
    RemoveQuad(center, sides - Vector3Int.one * 2);
  }

  public void AddBall(Vector3Int center, int radius)
  {
    for(int r = 0; r <= radius/2; r++)
    {
      AddDisc(center + Vector3Int.up * r, radius - 2*r);
      AddDisc(center + Vector3Int.down * r, radius - 2*r);
    }
  }

  public void AddEmptyBall(Vector3Int center, int radius)
  {
    AddBall(center, radius);
    RemoveBall(center, radius - 2);
  }

  public void AddDisc(Vector3Int center, int radius)
  {
    Disc(center, radius);
  }

  public void AddCylinder(Vector3Int center, Vector3Int direction, int radius, int height)
  {
    for(int h = 0; h<height; h++)
    {
      AddDisc(center + direction * h, radius);
    }
  }

  public void AddEmptyCylinder(Vector3Int center, Vector3Int direction, int radius, int height)
  {
    for (int h = 0; h < height; h++)
    {
      AddDisc(center + direction * h, radius);
      RemoveDisc(center + direction * h, radius - 2);
    }
  }
  #endregion

  #region remove methods
  public void RemovePoint(Vector3Int position) { Point(position, false); }
  public void RemoveSquare(Vector3Int center, int side) { RemoveQuad(center, Vector3Int.one * side); }
  public void RemoveQuad(Vector3Int center, Vector3Int sides) { Quad(center, sides, false); }
  public void RemoveDisc(Vector3Int center, int radius) { Disc(center, radius, false); }
  public void RemoveBall(Vector3Int center, int radius)
  {
    for (int r = 0; r <= radius / 2; r++)
    {
      RemoveDisc(center + Vector3Int.up * r, radius - 2 * r);
      RemoveDisc(center + Vector3Int.down * r, radius - 2 * r);
    }
  }
  public void RemoveCylinder(Vector3Int center, Vector3Int direction, int radius, int height)
  {
    for (int h = 0; h < height; h++)
    {
      RemoveDisc(center + direction * h, radius);
    }
  }
  #endregion

  #endregion

  #region private environment methods

  private void Quad(Vector3Int center, Vector3Int sides, bool add=true)
  {
    Vector3Int startingPoint = ValidateVector(center - new Vector3Int(sides.x / 2, sides.y / 2, sides.z / 2));

    for (int i = 0; i < sides.x; i++)
      for (int j = 0; j < sides.y; j++)
        for (int k = 0; k < sides.z; k++)
          Table[ValidateAxis(startingPoint.x + i), ValidateAxis(startingPoint.y + j), ValidateAxis(startingPoint.z + k)] = add;
  }

  private void Disc(Vector3Int center, int radius, bool add=true)
  {
    center = ValidateVector(center);
    Vector3Int startingPoint = ValidateVector(center - (new Vector3Int(1, 0, 1) * (radius / 2)));
    Vector3Int point;
    
    for (int i = 0; i < radius; i++)
      for (int k = 0; k < radius; k++)
      {
        point = new Vector3Int((startingPoint.x + i), (startingPoint.y), (startingPoint.z + k));
        if (ManhatanDistance(center - point) <= radius / 2)
          Table[ValidateAxis(point.x), ValidateAxis(point.y), ValidateAxis(point.z)] = add;
      }
  }

  private void Point(Vector3Int position, bool add=true)
  {
    Table[ValidateAxis(position.x), ValidateAxis(position.y), ValidateAxis(position.z)] = add;
  }

  #endregion

  #region support functions
  private Vector3Int ValidateVector(Vector3Int vec)
  {
    return new Vector3Int(ValidateAxis(vec.x), ValidateAxis(vec.y), ValidateAxis(vec.z));
  }

  private int ValidateAxis(int value)
  {
    while (value < 0)
      value += GameSize;
    return value % GameSize;
  }

  public int ManhatanDistance(Vector3Int vec)
  {
    return (int)(Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z));
  }
  #endregion
}