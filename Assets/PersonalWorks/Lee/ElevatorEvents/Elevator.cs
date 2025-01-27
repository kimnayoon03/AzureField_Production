using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;

enum MoveType
{
    Elevator,
    MovingObjects
};

[System.Serializable]
public enum ElevatorType
{   
    Auto = 0,
    Interaction = 1

};

public class Elevator : MonoBehaviour
{
/*
    엘레베이터 관련 오브젝트를 관리하는 스크립트 입니다. 해당 스크립트는 엘레베이터 뿐만 아니라
    움직이는 발판도 관리 할 수 있습니다.
    엘레베이터 오브젝트를 작동 시킬 때는 반드시 엘레베이터 감지 콜라이더에 ElevatorCollider을
    추가해주셔야 작동합니다.

*/
    [SerializeField] int StartPoint;
    [SerializeField] Transform[] Points;
    [SerializeField] public ElevatorType elevatorType;
    [SerializeField] MoveType moveType;
    
    static public Elevator instance;
    static public Elevator Instace{get{return instance;}}

    public float moveSpeed; 
    public bool Canmove = false;
    bool reverse;
    int i;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // 이전에 생성된 다른 인스턴스를 파괴합니다.
        }
        transform.position = Points[StartPoint].position;
        i = StartPoint;
    }

    private void Start() 
    {
        instance = this;
    }
    private void Update() 
    {

        if(moveType == MoveType.MovingObjects)
        { 
           MoveFloor();

        }
        else if(moveType == MoveType.Elevator)
        {
            MoveElevator();
        }
      
    }

   


    private void MoveFloor()
    {
        if(Vector3.Distance(transform.position, Points[i].position)< 0.01f)
        {
                
            i++;

            // i가 Points 배열의 길이를 초과하는 경우, 처음 위치로 되돌아가기
            if (i >= Points.Length)
            {
                i = 0;
            }
        }
        transform.position = Vector3.MoveTowards(transform.position,Points[i].position,
        moveSpeed * Time.deltaTime);
    }

    private void MoveElevator()
    {
        if(Vector3.Distance(transform.position, Points[i].position)< 0.01f)
            {
                Canmove = false;
                if(i == Points.Length - 1)
                {
                    reverse = true;
                    i--;
                    return;
                }
                else if(i==0)
                {
                    reverse = false;
                    i++;
                    return;
                }
                if(reverse)
                {
                    i++;
                }
                else
                {
                    i--;
                }
            }

            if(Canmove)
            {
                transform.position = Vector3.MoveTowards(transform.position,Points[i].position,
                moveSpeed * Time.deltaTime);
            }
        
    }

  

}



