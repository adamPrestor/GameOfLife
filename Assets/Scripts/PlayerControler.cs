using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{
  // self awarness
  public Vector3Int position = new Vector3Int();
  private Vector3 oldPosition;
  private Vector3 direction;
  private Vector3Int change;

  // table information
  private float CellWidth;
  private float CellGap;
  private int GameSize;

  public void Init(float cw, float cg)
  {
    CellWidth = cw;
    CellGap = cg;
    GameSize = GameManager.GameSize;
  }

  public void Update()
  {
    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      position += new Vector3Int(0, 0, 1);
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      position += new Vector3Int(0, 0, -1);
    }
    else if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      position += new Vector3Int(-1, 0, 0);
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      position += new Vector3Int(1, 0, 0);
    }
    else if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
    {
      position += new Vector3Int(0, -1, 0);
    }
    else if (Input.GetKeyDown(KeyCode.Space))
    {
      position += new Vector3Int(0, 1, 0);
    }
  }

  public void Move()
  {
    oldPosition = transform.position;
    transform.Translate(new Vector3(0, 0, CellWidth + CellGap));

    direction = transform.position - oldPosition;
    change = new Vector3Int(GetOrient(direction.x), GetOrient(direction.y), GetOrient(direction.z));
    position += change;

    // check the validity of the move
    if (!GameManager.IsValidPosition(position))
      position -= change;

    GameManager.TablePosition(transform.position);
  }

  private int GetOrient(float x)
  {
    if (x > 0)
      return 1;

    if (x < 0)
      return -1;

    return 0;
  }
  
}
