
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
  // main tables for GoL
  public bool[,,] GameTable;
  private bool[,,] newGameTable;
  public GameObject[,,] GameObjectsTable;
  private float TableSize;
  
  // UI
  
  // gameobject for table
  public GameObject CellObject;

  // table awerness
  public static int GameSize = 10;
  private static readonly float CellGap = 0.8f;
  private static readonly float CellWidth = 1.0f;

  // game utilities
  public float UpdateTime = 0.8f;
  public float MoveTime = 2.0f;
  private float TimeLeft = 0.0f;
  private bool IsCreated = false;

  // game of life parameters
  public int NeighboursAliveLow;
  public int NeighboursAliveHigh;
  public int NeighboursDeadLow;
  public int NeighboursDeadHigh;

  // other
  public float prob = 0.6f;
  private readonly bool firstRun = true;

  // Start is called before the first frame update
  void Start()
  {
    GameTable = new bool[GameSize, GameSize, GameSize];
    newGameTable = new bool[GameSize, GameSize, GameSize];
    GameObjectsTable = new GameObject[GameSize, GameSize, GameSize];
    
    TableSize = GameSize * CellWidth + (GameSize - 1) * CellGap;

    AssignTableValues();

    GenerateThePlayingField();
    
    IsCreated = true;
    //StartCoroutine(GameEngine());
    PlayTheGame();
  }

  void Update()
  {
    
  }
  
  void PlayTheGame()
  {
    bool currentValue;

    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          currentValue = GetCellValue(i, j, k);
          newGameTable[i, j, k] = currentValue;
          ReplaceCell(i, j, k, GameTable[i, j, k], GameObjectsTable);
        }

    // rewrite the newGameTable into the GameTable
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          GameTable[i, j, k] = newGameTable[i, j, k];
  }

  void AssignTableValues()
  {
    for (int i = 0; i<GameSize; i++)
      for (int j = 0; j<GameSize; j++)
        for (int k = 0; k<GameSize; k++)
          GameTable[i, j, k] = Random.value < prob;
  }
  
  IEnumerator GameEngine()
  {
    while(true)
    {
      // play a step of the game
      PlayTheGame();

      TimeLeft = UpdateTime;

      yield return new WaitForSeconds(UpdateTime);
    }
  }

  void GenerateThePlayingField()
  {
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          GameObjectsTable[i, j, k] = GenerateCell(i, j, k);
  }

  bool GetCellValue(int i, int j, int k)
  {
    // get the number of live neighbours
    int liveNeighbours = 0;
    int timesInLoop = 0;
    int neededNeighboursLow;
    int neededNeighboursHigh;

    if (GameTable[i, j, k]) {
      neededNeighboursLow = NeighboursAliveLow; neededNeighboursHigh = NeighboursAliveHigh;
    } else {
      neededNeighboursLow = NeighboursDeadLow; neededNeighboursHigh = NeighboursDeadHigh;
    }

    int[] widths = { (i - 1 + GameSize) % GameSize, i, (i + 1) % GameSize };
    int[] heights = { (j - 1 + GameSize) % GameSize, j, (j + 1) % GameSize };
    int[] depths = { (k - 1 + GameSize) % GameSize, k, (k + 1) % GameSize };

    foreach (int w in widths)
      foreach (int h in heights)
        foreach (int d in depths)
        {
          timesInLoop++;
          if (GameTable[w, h, d])
            liveNeighbours++;
        }
    
    if (GameTable[i, j, k])
      liveNeighbours--;

    return neededNeighboursHigh >= liveNeighbours && liveNeighbours >= neededNeighboursLow;
  }

  private Vector3 GetTablePosition(int widthIndex, int heightIndex, int depthIndex)
  {
    float width = widthIndex * CellWidth + (widthIndex - 1) * CellGap + CellWidth / 2;
    float height = heightIndex * CellWidth + (heightIndex - 1) * CellGap + CellWidth / 2;
    float depth = depthIndex * CellWidth + (depthIndex - 1) * CellGap + CellWidth / 2;

    return new Vector3(width, height, depth);
  }

  private GameObject GenerateCell(int widthIndex, int heightIndex, int depthIndex)
  {
    GameObject prefab;
    
    //prefab = CellObject;
    prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

    Vector3 pos = GetTablePosition(widthIndex, heightIndex, depthIndex);
    pos.z -= 0.6f;
    Vector3 size = prefab.GetComponentInChildren<Renderer>().bounds.size;
    
    GameObject cellItem = Instantiate(prefab, pos, Quaternion.identity);

    Vector3 rescale = cellItem.transform.localScale;

    rescale.x = 1 / size.x;
    rescale.y = 1 / size.y;
    rescale.z = 1 / size.z;

    cellItem.transform.localScale = rescale;

    return cellItem;
  }

  private void ReplaceCell(int widthIndex, int heightIndex, int depthIndex, bool isFilled, GameObject[,,] table)
  {
    table[widthIndex, heightIndex, depthIndex].SetActive(isFilled);
  }

  public static Vector3Int TablePosition(Vector3 position)
  {
    // inverse method of the position estimation depending on table position
    Vector3Int pos = new Vector3Int();
    
    return pos;
  }

  public static bool IsValidPosition(Vector3 position)
  {
    return IsValidAxis(position.x) && IsValidAxis(position.y) && IsValidAxis(position.z);
  }

  public static bool IsValidAxis(float value)
  {
    return value < GameSize && value >= 0;
  }
}

public class LabDist
{
  public int dist;
  public Vector3Int vect;

  public LabDist(Vector3Int v, int d)
  {
    dist = d;
    vect = v;
  }
}
