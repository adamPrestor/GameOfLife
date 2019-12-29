using System;
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
  public GameObject[,,] FutureObjectsTable;
  private float TableSize;

  // labirynth utilities
  private bool[,,] labyrinth;
  public Vector3Int[] path; // denotes the path of the labyrinth - work in progress

  // player
  public PlayerControler player;
  private Vector3Int playerStart;

  // minimap utils
  public Camera minimapC;

  // UI
  public Text countdown;
  
  // gameobject for table
  public GameObject CellObject;
  public GameObject FutureCellObject;

  // table awarnes
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
  private bool firstRun = true;

  // Start is called before the first frame update
  void Start()
  {
    GameTable = new bool[GameSize, GameSize, GameSize];
    newGameTable = new bool[GameSize, GameSize, GameSize];
    GameObjectsTable = new GameObject[GameSize, GameSize, GameSize];
    FutureObjectsTable = new GameObject[GameSize, GameSize, GameSize];

    SetLabyrinth();

    TableSize = GameSize * CellWidth + (GameSize - 1) * CellGap;

    AssignTableValues();

    GenerateThePlayingField();

    InitiateMiniMap();

    InitiatePlayerPosition();

    IsCreated = true;
    //StartCoroutine(GameEngine());
    PlayTheGame();
  }

  private void SetLabyrinth()
  {
    labyrinth = new bool[GameSize, GameSize, GameSize];

    foreach(Vector3Int p in path)
    {
      labyrinth[p.x, p.y, p.z] = true;
    }
  }

  void Update()
  {
    // update the timer in the UI
    TimeLeft -= Time.deltaTime;

    if (TimeLeft < 0)
    {
      countdown.color = new Color(0.5f, 0.5f, 0.0f, 1);
      TimeLeft = 0.0000f;
    } else
    {
      countdown.color = new Color(0f, 0f, 0f, 1f);
    }

    countdown.text = "Move in: " + TimeLeft.ToString("N0");

    player.transform.position = GetTablePosition(player.position.x, player.position.y, player.position.z);
  }

  private void InitiateMiniMap()
  {
    minimapC.transform.position = new Vector3(TableSize * 1.5f, TableSize * 1.5f, TableSize / 2);
    minimapC.transform.rotation = Quaternion.Euler(135, 90, 90);
    minimapC.orthographicSize = TableSize * 0.8f;
  } 

  private void InitiatePlayerPosition()
  {
    //int pos_i = 0, pos_j = 0, pos_k = 0;

    //for (int i = 0; i < GameSize; i++)
    //  for (int j = 0; j < GameSize; j++)
    //    for (int k = 0; k < GameSize; k++)
    //      if (!GameTable[i, j, k])
    //      {
    //        pos_i = i;
    //        pos_j = j;
    //        pos_k = k;
    //        break;
    //      }
    
    player.position = path[0];
    player.Init(CellWidth, CellGap);
    player.transform.position = GetTablePosition(player.position.x, player.position.y, player.position.z);
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
          //ReplaceCell(i, j, k, !GameTable[i, j, k] & newGameTable[i, j, k], FutureObjectsTable);
        }

    // rewrite the newGameTable into the GameTable
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          GameTable[i, j, k] = newGameTable[i, j, k];
  }

  void AssignTableValues()
  {
    int distance;

    for (int i = 0; i<GameSize; i++)
      for (int j = 0; j<GameSize; j++)
        for (int k = 0; k<GameSize; k++)
        {
          distance = LabDist(new Vector3Int(i, j, k));
          if (labyrinth[i, j, k])
          {
            GameTable[i, j, k] = false;
          }
          else if (InNeighbourHood(i, j, k))
          {
            GameTable[i, j, k] = true;
          }
          else
            GameTable[i, j, k] = false;
        }
  }

  private bool InNeighbourHood(int pos_i, int pos_j, int pos_k)
  {
    // generate all neighbours of position i, j, k and check if any of them contain labyrinth
    for (int i = -1; i < 2; i++)
      for (int j = -1; j < 2; j++)
        for (int k = -1; k < 2; k++)
          if (IsValidPosition(new Vector3(pos_i + i, pos_j + j, pos_k + k)) && 
              labyrinth[pos_i + i, pos_j + j, pos_k + k] && !(i == pos_i && j == pos_j && k == pos_k)
            )
            return true;

    return false;
  }

  private int LabDist(Vector3Int startingPoint)
  {
    // find the closest labyrinth cell - depth first search
    Queue<LabDist> queue = new Queue<LabDist>();
    LabDist ld;
    queue.Enqueue(new LabDist(startingPoint, 0));
    while (queue.Count > 0)
    {
      ld = queue.Dequeue();
      if (labyrinth[ld.vect.x, ld.vect.y, ld.vect.z])
      {
        return ld.dist;
      }

      // add new items to the queue, if they are appropriate
      

    }
    return -1;
  }

  IEnumerator GameEngine()
  {
    while(true)
    {
      // play a step of the game
      PlayTheGame();

      TimeLeft = UpdateTime;

      yield return new WaitForSeconds(UpdateTime);

      // animations to move players, etc.
      player.Move();
      player.transform.position = GetTablePosition(player.position.x, player.position.y, player.position.z);

      yield return new WaitForSeconds(MoveTime);

      // player.transform.Translate(new Vector3(0, 0, CellWidth + CellGap));
    }
  }

  void GenerateThePlayingField()
  {
    for (int i = 0; i < GameSize; i++)
    {
      for (int j = 0; j < GameSize; j++)
      {
        for (int k = 0; k < GameSize; k++)
        {
          GameObjectsTable[i, j, k] = GenerateCell(i, j, k, false);
          // FutureObjectsTable[i, j, k] = GenerateCell(i, j, k, true);
        }
      }
    }
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

  private GameObject GenerateCell(int widthIndex, int heightIndex, int depthIndex, bool isFuture)
  {
    GameObject prefab;

    if (isFuture)
      prefab = FutureCellObject;
    else
      prefab = CellObject;
    // prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //prefab.GetComponent<Animator>().enabled = false;

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
