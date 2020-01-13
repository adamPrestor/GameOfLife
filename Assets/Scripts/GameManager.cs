using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class GameManager : MonoBehaviour
{
  // main tables for GoL
  public bool[,,] GameTable;
  private bool[,,] NextGameTable;
  public GameObject[,,] GameObjectsTable;
  private int[,,] TimeAlive;

  // size of the game of life
  private readonly float realWidth = 40;
  private Vector3 botLeftPoint = new Vector3(-10, 0, -10);
  private readonly Vector3 InitBotLeftPoint = new Vector3(-10, 0, -10);

  // table awarness
  public int GameSize = 10; // TODO: read from Level class
  public float CellGap = 0.0f;
  private float CellWidth = 0.0f;

  // game utilities
  private readonly int[] GameSpeeds = { 1, 2, 4, 8 };
  public int GameSpeed = 0;
  private float UpdateTime;

  public float prob = 0.4f; // TODO: replace with Level class to create a level table
  private bool spread = false;
  private bool waveSpread = false;

  // coroutines
  Coroutine GameRoutine = null;
  Coroutine SpreadRoutine = null;
  Coroutine RotateRoutine = null;

  public bool IsPlaying => GameRoutine != null;

  // colors
  private Color lowLife = new Color(1.0f, 0f, 0f);
  private Color medLife = new Color(1.0f, 1.0f, 0f);
  private Color higLife = new Color(0f, 1f, 0f);
  private Color cellColor;

  // level holder
  public LevelHolder LH;
  private Level CurrentLevel;


  [Header("UI elements")]
  // UI elements
  public GameObject Playground;
  public Text MovesText;
  public Text SpeedText;
  public Slider UnderPopulationSlider;
  public Slider OverPopulationSlider;
  public Slider BirthSlider;
  public Slider OverBirthSlider;
  public Text UnderPopulationText;
  public Text OverPopulationText;
  public Text BirthText;
  public Text OverBirthText;

  // end game UI elements
  public GameObject notes;
  public Text SuccessText;
  public Text FailureText;

  // buttons
  public Button StartButton;
  public Button StopButton;
  public Button RestartButton;
  public Button NextLevelButton;
  public Button ExitButton;
  public Button SpeedButton;

  // toggles
  public Toggle RotateButton;
  public Toggle SplitButton;
  public Toggle WaveButton;

  [Header("Camera")]
  //public Camera camera;
  public GameObject center;

  // speech recognition
  private KeywordRecognizer keywordRecognizer;
  private Dictionary<string, Action> actions = new Dictionary<string, Action>();


  [Header("Level utilities")]
  // game of life parameters
  private int NeighboursAliveLow;
  private int NeighboursAliveHigh;
  private int NeighboursDeadLow;
  private int NeighboursDeadHigh;

  private int MovesToComplete; // TODO - erase this, load from Level class
  private int MovesDone = 0; // moves that we already have made

  private bool StillPlayable = true; // tells us whatever there are still some 
  private bool Completed = false;

  // test za shader
  int[,,] ShaderIn;
  int[,,] ShaderOut;
  public ComputeShader compute;
  int ShaderThreadSize = 10;
  int groupSize;

  // toggle buttons pressed colors
  float ch = 1f;
  Color ButtonPressed = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1f);
  Color ButtonReleased = new Color(1f, 1f, 1f, 1f);

  // Start is called before the first frame update
  void Start()
  {
    NextLevelButton.interactable = Completed;

    LoadLevel();

    // set the moves text
    MovesText.text = MovesDone.ToString() + "/" + MovesToComplete.ToString();

    // set slider texts
    OnSliderChange();

    // initiate cell size
    CellGap = 0;
    CalculateWidthAndBeggining();

    // set update speed
    SetUpdateTime();

    // assign the table values
    AssignTableValues();

    // generate the squares according to Neighbourhood
    GenerateThePlayingField();

    // initiate speech recognition utilities
    InitSpeechControl();
  }

  void Update()
  {
  }

  #region Button Press Functions
  public void PlayGameButtonPressed()
  {
    if (GameRoutine == null)
    {
      // get the parameters of the game
      NeighboursAliveLow = (int)UnderPopulationSlider.value;
      NeighboursAliveHigh = (int)OverPopulationSlider.value;
      NeighboursDeadLow = (int)BirthSlider.value;
      NeighboursDeadHigh = (int)OverBirthSlider.value;

      // set the input fields as inactive
      OverBirthSlider.enabled = false;
      BirthSlider.enabled = false;
      UnderPopulationSlider.enabled = false;
      OverPopulationSlider.enabled = false;
      
      NextLevelButton.interactable = Completed;

      // set completed to false
      Completed = false;

      // resize Playground area to fullsize
      Playground.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 0.0f);

      // hide play button, show stop button
      StartButton.gameObject.SetActive(false);
      StopButton.gameObject.SetActive(true);

      // start game routine
      GameRoutine = StartCoroutine(GameEngine());
    }
  }
  public void StopGameButtonPressed()
  {
    // SUPPOSE TO STOP THE GAME, CAN BE RESTARTED

    // set the sliders back to active
    if (MovesDone == 0)
    {
      OverBirthSlider.enabled = true;
      BirthSlider.enabled = true;
      UnderPopulationSlider.enabled = true;
      OverPopulationSlider.enabled = true;
    }

    NextLevelButton.interactable = Completed;

    // stop the game
    if (GameRoutine != null)
    {
      StopCoroutine(GameRoutine);
      GameRoutine = null;
    }
    // hide stop button, show play button
    StartButton.gameObject.SetActive(true);
    StopButton.gameObject.SetActive(false);

    // resize the Playground back to the right part
    Playground.GetComponent<RectTransform>().anchorMin = new Vector2(0.399f, 0.0f);
  }
  public void ResetGameButtonPressed()
  {
    // SUPPOSE TO RESET THE GAME

    // count down the number of moves
    MovesDone = 0;
    MovesText.text = MovesDone.ToString() + "/" + MovesToComplete.ToString();

    // reset game thingies
    StillPlayable = true;
    TimeAlive = new int[GameSize, GameSize, GameSize];

    StopGameButtonPressed();

    Completed = false;
    NextLevelButton.interactable = Completed;
    
    // hide the notes
    notes.SetActive(false);

    // reset the table
    AssignTableValues();
    RepopulateTheGame();
  }
  public void NextLevelButtonPressed()
  {
    if(NextLevelButton.interactable)
    {
      StopGameButtonPressed();

      LoadLevel();

      // generate the squares according to Neighbourhood
      GenerateThePlayingField();

      ResetGameButtonPressed();
    }
  }
  public void ExitButtonPressed()
  {
    // TODO: exit application - experiment on it!
    Application.Quit();
  }
  public void SpeedGameButtonPressed()
  {
    GameSpeed = (GameSpeed + 1) % GameSpeeds.Length;
    SetUpdateTime();
  }
  public void SpreadButtonToggle()
  {
    // spread the cubes for clearer vision up and down
    if (SpreadRoutine != null)
    {
      StopCoroutine(SpreadRoutine);
      SpreadRoutine = null;
    }
    spread = !spread; // negate spread
    if (spread)
        SplitButton.GetComponent<Image>().color = ButtonPressed;
    else
      SplitButton.GetComponent<Image>().color = ButtonReleased;
    SpreadRoutine = StartCoroutine(SpreadCubes());
  }
  public void RotateButtonToggle()
  {
    if (RotateRoutine != null)
    {
      StopCoroutine(RotateRoutine);
      RotateRoutine = null;
      RotateButton.GetComponent<Image>().color = ButtonReleased;
    }
    else 
    {
      RotateButton.GetComponent<Image>().color = ButtonPressed;
      RotateRoutine = StartCoroutine(RotateCamera());
    }
      
  }
  public void WaveButtonPressed()
  {
    waveSpread = !waveSpread;
    if(waveSpread)
      WaveButton.GetComponent<Image>().color = ButtonPressed;
    else
      WaveButton.GetComponent<Image>().color = ButtonReleased;
  }
  #endregion

  #region Sliders
  public void OnSliderChange()
  {
    // update all the texts, to keep consistency
    UnderPopulationText.text = UnderPopulationSlider.value.ToString();
    OverPopulationText.text = OverPopulationSlider.value.ToString();
    BirthText.text = BirthSlider.value.ToString();
    OverBirthText.text = OverBirthSlider.value.ToString();


    // set the boundaries
    UnderPopulationSlider.maxValue = OverPopulationSlider.value;
    OverPopulationSlider.minValue = UnderPopulationSlider.value;
    BirthSlider.maxValue = OverBirthSlider.value;
    OverBirthSlider.minValue = BirthSlider.value;
  }
  #endregion

  #region Speech
  private void InitSpeechControl()
  {
    // start the voice recognition
    actions.Add("split", SpreadButtonToggle);
    actions.Add("speed", SpeedGameButtonPressed);
    actions.Add("fast", SpeedGameButtonPressed);
    actions.Add("hello", SpreadButtonToggle);
    actions.Add("rotate", RotateButtonToggle);
    actions.Add("wave", WaveButtonPressed);
    actions.Add("play", PlayGameButtonPressed);
    actions.Add("game", PlayGameButtonPressed);
    // actions.Add("start", PlayGameButtonPressed);
    actions.Add("stop", StopGameButtonPressed);
    actions.Add("pause", StopGameButtonPressed);
    actions.Add("reset", ResetGameButtonPressed);
    actions.Add("next", NextLevelButtonPressed);
    actions.Add("exit", ExitButtonPressed);


    keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray(), ConfidenceLevel.Low);
    keywordRecognizer.OnPhraseRecognized += RecognizeSpeech;
    keywordRecognizer.Start();
  }
  private void RecognizeSpeech(PhraseRecognizedEventArgs speech)
  {
    StringBuilder builder = new StringBuilder();
    builder.AppendFormat("{0} ({1}){2}", speech.text, speech.confidence, Environment.NewLine);
    builder.AppendFormat("\tTimestamp: {0}{1}", speech.phraseStartTime, Environment.NewLine);
    builder.AppendFormat("\tDuration: {0} seconds{1}", speech.phraseDuration.TotalSeconds, Environment.NewLine);
    Debug.Log(builder.ToString());

    actions[speech.text].Invoke();
  }
  #endregion

  #region Coroutines
  IEnumerator GameEngine()
  {
    if (MovesDone == MovesToComplete)
    {
      // koncan nivo, nalozi naslednjega oziroma ne, naredi kr nekej
      SuccessText.gameObject.SetActive(true);
      FailureText.gameObject.SetActive(false);
      notes.SetActive(true);
      Debug.Log("Level is finished");

      StopGameButtonPressed();

      Completed = true;
      NextLevelButton.interactable = true;
    }
    while (StillPlayable && MovesDone < MovesToComplete && !Completed)
    {
      StillPlayable = false;
      // PlayGame();
      ShaderIn = new int[GameSize, GameSize, GameSize];
      ShaderOut = new int[GameSize, GameSize, GameSize];

      for (int i = 0; i < GameSize; i++)
        for (int j = 0; j < GameSize; j++)
          for (int k = 0; k < GameSize; k++)
          {
            ShaderIn[i, j, k] = GameTable[i, j, k] ? 1 : 0;
            ShaderOut[i, j, k] = GameTable[i, j, k] ? 1 : 0;
          }

      var kernel = compute.FindKernel("CSMain");
      var inBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));
      var outBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));
      var aliveBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));

      inBuffer.SetData(ShaderIn);
      outBuffer.SetData(ShaderOut);
      aliveBuffer.SetData(TimeAlive);

      compute.SetBuffer(kernel, "Input", inBuffer);
      compute.SetBuffer(kernel, "Result", outBuffer);
      compute.SetBuffer(kernel, "TimeAlive", aliveBuffer);

      compute.SetInt("GameSize", GameSize);
      compute.SetInt("OP", NeighboursAliveHigh);
      compute.SetInt("UP", NeighboursAliveLow);
      compute.SetInt("OB", NeighboursDeadHigh);
      compute.SetInt("UB", NeighboursDeadLow);

      groupSize = Mathf.CeilToInt(GameSize / (float)ShaderThreadSize);

      compute.Dispatch(kernel, groupSize, groupSize, groupSize);

      inBuffer.GetData(ShaderIn);
      outBuffer.GetData(ShaderOut);
      aliveBuffer.GetData(TimeAlive);
      
      inBuffer.Release();
      outBuffer.Release();
      aliveBuffer.Release();

      for (int i = 0; i < GameSize; i++)
      {
        for (int j = 0; j < GameSize; j++)
          for (int k = 0; k < GameSize; k++)
          {
            if (GameTable[i, j, k] != ShaderOut[i, j, k] > 0) { ReplaceCell(i, j, k, !GameTable[i, j, k], GameObjectsTable); }
            GameTable[i, j, k] = ShaderOut[i, j, k] > 0;
            if (GameTable[i, j, k])
            {
              StillPlayable = true;
              if (TimeAlive[i, j, k] < MovesToComplete / 2)
                cellColor = Color.Lerp(lowLife, medLife, TimeAlive[i, j, k] / (MovesToComplete / 2.0f));
              else
                cellColor = Color.Lerp(medLife, higLife, (TimeAlive[i, j, k] - MovesToComplete / 2) / (MovesToComplete / 2.0f));
              GameObjectsTable[i, j, k].GetComponent<Renderer>().material.color = cellColor;
            }
          }
        if (waveSpread)
          yield return new WaitForSeconds(0.05f);
      }
      
      // END play game

      MovesDone += 1;
      // update the Moves counter text
      MovesText.text = MovesDone.ToString() + "/" + MovesToComplete.ToString();

      if (MovesDone == MovesToComplete)
      {
        // koncan nivo, nalozi naslednjega oziroma ne, naredi kr nekej
        SuccessText.gameObject.SetActive(true);
        FailureText.gameObject.SetActive(false);
        notes.SetActive(true);
        Debug.Log("Level is finished");

        StopGameButtonPressed();

        Completed = true;
        NextLevelButton.interactable = true;
      }

      // wait for the next step of the game
      yield return new WaitForSeconds(UpdateTime);
    }

    if (!Completed)
    {
      // fill the last state, end game
      // PlayTheGame();

      // show the end game over the top
      SuccessText.gameObject.SetActive(false);
      FailureText.gameObject.SetActive(true);
      notes.SetActive(true);

      StopGameButtonPressed();
    }
  }
  IEnumerator RotateCamera()
  {
    while (true)
    {
      center.transform.Rotate(new Vector3(0, 1.0f, 0));
      yield return new WaitForSeconds(0.05f);
    }
  }
  IEnumerator SpreadCubes()
  {
    float goal = 0f;
    float change = 0.2f;
    if (spread)
    {
      goal = (realWidth - (CellGap * (GameSize - 1) * 2)) / (GameSize * 2) / 2.5f;
    }
    else
    {
      goal = 0f;
      change = -change;
    }

    while ((spread && CellGap < goal) || (!spread && CellGap > goal))
    {

      CellGap += change;
      CalculateWidthAndBeggining();

      for (int i = 0; i < GameSize; i++)
        for (int j = 0; j < GameSize; j++)
          for (int k = 0; k < GameSize; k++)
          {
            GameObjectsTable[i, j, k].transform.position = GetTablePosition(i, j, k);
            GameObjectsTable[i, j, k].transform.localScale = CellScale(GameObjectsTable[i, j, k]);
          }
      yield return new WaitForSeconds(0.0f);
    }
  }
  #endregion

  #region Load Functions
  private void LoadLevel()
  {
    if (GameObjectsTable != null)
      ClearGameObjectsTable();

    CurrentLevel = LH.GetLevel();
    GameSize = CurrentLevel.GameSize;
    MovesToComplete = CurrentLevel.MovesToComplete;

    // calculate width and beggining
    if(!spread)
      CellGap = 0;
    else
      CellGap = (realWidth - (CellGap * (GameSize - 1) * 2)) / (GameSize * 2) / 1.5f;

    CalculateWidthAndBeggining();

    GameRoutine = null;

    // initiate other level utilities
    GameTable = new bool[GameSize, GameSize, GameSize];
    NextGameTable = new bool[GameSize, GameSize, GameSize];
    GameObjectsTable = new GameObject[GameSize, GameSize, GameSize];
    TimeAlive = new int[GameSize, GameSize, GameSize];

    // copy level table into game table
    AssignTableValues();
  }
  #endregion

  #region Calculate Functions
  private void CalculateWidthAndBeggining()
  {
    // calculate cell width and displace the botleft for cellgap
    CellWidth = (realWidth - (CellGap * (GameSize - 1) * 2)) / (GameSize * 2);

    botLeftPoint = InitBotLeftPoint + new Vector3(CellGap, CellGap, CellGap);
  }
  private Vector3 GetTablePosition(int widthIndex, int heightIndex, int depthIndex)
  {

    float width = widthIndex * CellWidth + (widthIndex - 1) * CellGap + CellWidth / 2 + botLeftPoint.x;
    float height = heightIndex * CellWidth + (heightIndex - 1) * CellGap + CellWidth / 2 + botLeftPoint.y;
    float depth = depthIndex * CellWidth + (depthIndex - 1) * CellGap + CellWidth / 2 + botLeftPoint.z;

    return new Vector3(width, height, depth);
  }
  private void SetUpdateTime()
  {
    UpdateTime = 2.0f / GameSpeeds[GameSpeed];
    SpeedText.text = "x" + GameSpeeds[GameSpeed].ToString();
  }
  #endregion

  #region Game Util Functions
  private void RepopulateTheGame()
  {
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          ReplaceCell(i, j, k, GameTable[i, j, k], GameObjectsTable);
          GameObjectsTable[i, j, k].GetComponent<Renderer>().material.color = Color.white;
        }
  }
  void AssignTableValues()
  {
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          GameTable[i, j, k] = CurrentLevel.Table[i, j, k];
  }
  private void ClearGameObjectsTable()
  {
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          if (GameObjectsTable[i, j, k] != null)
            Destroy(GameObjectsTable[i, j, k]);
  }
  void GenerateThePlayingField()
  {
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          GameObjectsTable[i, j, k] = GenerateCell(i, j, k);
          ReplaceCell(i, j, k, GameTable[i, j, k], GameObjectsTable);
        }
  }
  private void ReplaceCell(int widthIndex, int heightIndex, int depthIndex, bool isFilled, GameObject[,,] table)
  {
    table[widthIndex, heightIndex, depthIndex].SetActive(isFilled);
  }
  private GameObject GenerateCell(int widthIndex, int heightIndex, int depthIndex)
  {
    GameObject cellItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //cellItem.AddComponent<AudioSource>();
    //cellItem.AddComponent<AudioAgent>();
    //cellItem.AddComponent<AudioAgent>().audioSource = cellItem.GetComponent<AudioSource>();
    //cellItem.AddComponent<AudioAgent>().position = new Vector3Int(widthIndex, heightIndex, depthIndex);

    Vector3 pos = GetTablePosition(widthIndex, heightIndex, depthIndex);
    Vector3 size = cellItem.GetComponentInChildren<Renderer>().bounds.size;

    cellItem.transform.position = pos;
    cellItem.transform.localScale = CellScale(cellItem);

    return cellItem;
  }
  private Vector3 CellScale(GameObject go)
  {
    //Vector3 size = go.GetComponentInChildren<Renderer>().bounds.size;
    Vector3 localSize = go.transform.localScale;

    return new Vector3(CellWidth, CellWidth, CellWidth);
  }
  public bool IsValidPosition(Vector3 position)
  {
    return IsValidAxis(position.x) && IsValidAxis(position.y) && IsValidAxis(position.z);
  }
  public bool IsValidAxis(float value)
  {
    return value < GameSize && value >= 0;
  }
  #endregion

  #region Legacy Functions
  void PlayTheGame()
  {
    ShaderIn = new int[GameSize, GameSize, GameSize];
    ShaderOut = new int[GameSize, GameSize, GameSize];

    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          ShaderIn[i, j, k] = GameTable[i, j, k] ? 1 : 0;
          ShaderOut[i, j, k] = GameTable[i, j, k] ? 1 : 0;
        }

    var kernel = compute.FindKernel("CSMain");
    var inBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));
    var outBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));
    var aliveBuffer = new ComputeBuffer((int)Mathf.Pow(GameSize, 3), sizeof(int));

    inBuffer.SetData(ShaderIn);
    outBuffer.SetData(ShaderOut);
    aliveBuffer.SetData(TimeAlive);

    compute.SetBuffer(kernel, "Input", inBuffer);
    compute.SetBuffer(kernel, "Result", outBuffer);
    compute.SetBuffer(kernel, "TimeAlive", aliveBuffer);

    compute.SetInt("GameSize", GameSize);
    compute.SetInt("OP", NeighboursAliveHigh);
    compute.SetInt("UP", NeighboursAliveLow);
    compute.SetInt("OB", NeighboursDeadHigh);
    compute.SetInt("UB", NeighboursDeadLow);

    groupSize = Mathf.CeilToInt(GameSize / (float)ShaderThreadSize);

    print(groupSize);

    compute.Dispatch(kernel, groupSize, groupSize, groupSize);

    inBuffer.GetData(ShaderIn);
    outBuffer.GetData(ShaderOut);
    aliveBuffer.GetData(TimeAlive);

    for (int i = 0; i < GameSize; i++)
    {
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          if (GameTable[i, j, k] != ShaderOut[i, j, k] > 0) { ReplaceCell(i, j, k, !GameTable[i, j, k], GameObjectsTable); }
          GameTable[i, j, k] = ShaderOut[i, j, k] > 0;
          if (GameTable[i, j, k])
          {
            StillPlayable = true;
            if (TimeAlive[i, j, k] < MovesToComplete / 2)
              cellColor = Color.Lerp(lowLife, medLife, TimeAlive[i, j, k] / (MovesToComplete / 2.0f));
            else
              cellColor = Color.Lerp(medLife, higLife, (TimeAlive[i, j, k] - MovesToComplete / 2) / (MovesToComplete / 2.0f));
            GameObjectsTable[i, j, k].GetComponent<Renderer>().material.color = cellColor;
          }
        }
    }

    inBuffer.Release();
    outBuffer.Release();
    aliveBuffer.Release();

    /*
    bool currentValue;

    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
        {
          currentValue = GetCellValue(i, j, k);
          if (currentValue)
            StillPlayable = true;
          NextGameTable[i, j, k] = currentValue;
          ReplaceCell(i, j, k, GameTable[i, j, k], GameObjectsTable);
          if (currentValue)
            TimeAlive[i, j, k] += 1;
          else
            TimeAlive[i, j, k] = 1;
          // color the cell item
          if (TimeAlive[i,j,k] < MovesToComplete / 2)
            cellColor = Color.Lerp(lowLife, medLife, TimeAlive[i, j, k] / (MovesToComplete / 2.0f));
          else
            cellColor = Color.Lerp(medLife, higLife, (TimeAlive[i, j, k] - MovesToComplete/2) / (MovesToComplete / 2.0f));
          GameObjectsTable[i, j, k].GetComponent<Renderer>().material.color = cellColor;
        }
    // rewrite the newGameTable into the GameTable
    for (int i = 0; i < GameSize; i++)
      for (int j = 0; j < GameSize; j++)
        for (int k = 0; k < GameSize; k++)
          GameTable[i, j, k] = NextGameTable[i, j, k];
    */
  }
  bool GetCellValue(int i, int j, int k)
  {
    // get the number of live neighbours
    int liveNeighbours = 0;
    int timesInLoop = 0;
    int neededNeighboursLow;
    int neededNeighboursHigh;

    if (GameTable[i, j, k])
    {
      neededNeighboursLow = NeighboursAliveLow; neededNeighboursHigh = NeighboursAliveHigh;
    }
    else
    {
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
  #endregion

}