using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using PA_DronePack; // for Drone Asset which I downloaded
using System;
public class DroneAgent : Agent
{
    private PA_DroneController dcoScript; // API�� ���� �����? �����ϱ� ���� PA_DroneController
    public DroneSetting area;
                // DroneSetting.cs���� ������ DroneSetting Ŭ������ �����? ������Ʈ area
    public GameObject [] goals;  // ?��?�� 개의 목적�?�? 배열�? ?��?��             // ������ ������Ʈ(DroneSetting.cs������ �����ϰ� ������. Why?
                                          // --> Drone Agent �� Area(Drone Setting) ��ο���? goal ������Ʈ�� ��ȣ�ۿ��� �ʿ��� �����?
    public GameObject [] obstacle;
    
    float preDist;                        // ����: ���� �����? ��ġ�� �������� �Ÿ� - ���� �����? ��ġ�� �������� �Ÿ�  
    float [] min;
    public Transform agentTrans;         // Drone
    public Transform [] goalTrans;  // ?��?�� 개의 목적�?�? 배열�? ????��
    public Transform [] ObstacleTrans;
    public int [] check;
    public Rigidbody agent_Rigidbody;    // Drone
    int j;
    float goalChange=0;
    int[] order;
    int GoalSequence=0; //목적지 인덱스 순서
    
    public override void Initialize() // origin: Unity.MLAgents --> Agent Class���� ���ǵǰ�, ���⼭�� overriding��
    {                                 // Agent�� ó�� Ȱ��ȭ�� �� �ѹ� ȣ��  

        dcoScript = gameObject.GetComponent<PA_DroneController>(); // Drone Agent�� �ִ� PA_DroneController ��ũ��Ʈ�� �ҷ�ȭ docScript ������ ����

        agentTrans = gameObject.transform;
        goalTrans = new Transform[goals.Length];  // 배열?�� ?��기�?? 목적�? 개수?�� 맞게 초기?��
        ObstacleTrans = new Transform[obstacle.Length];  

        check = new int[goals.Length]; 
        order = new int[goals.Length];
        for (int i = 0; i < goals.Length; i++)
        {
            goalTrans[i] = goals[i].transform;  // �? 목적�??�� Transform?�� 배열?�� ????��
            check[i]=0;
        }

        for (int i = 0; i < obstacle.Length; i++)
        {
            ObstacleTrans[i] = obstacle[i].transform;  // �? 목적�??�� Transform?�� 배열?�� ????��
        }

        agent_Rigidbody = gameObject.GetComponent<Rigidbody>();

        Academy.Instance.AgentPreStep += WaitTimeInference;  // C#���� �̺�Ʈ ó�� ����
    }

    public override void CollectObservations(VectorSensor sensor) 
    {                                                            
        
        UnityEngine.Debug.Log(GoalSequence+"collect"+j);
        sensor.AddObservation(agentTrans.position - goalTrans[GoalSequence].position);
        
        sensor.AddObservation(agent_Rigidbody.velocity);  

        // ���ӵ� ����
        sensor.AddObservation(agent_Rigidbody.angularVelocity); // �����? ���ӵ� ����

        for (int i = 0; i < obstacle.Length; i++)
        {
            //sensor.AddObservation(agentTrans.position - ObstacleTrans[i].position);
        }
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers) {
        AddReward(-0.01f);  
        var actions = actionBuffers.ContinuousActions; 

        float moveX = Mathf.Clamp(actions[0], -1, 1f);  
        float moveY = Mathf.Clamp(actions[1], -1, 1f);
        float moveZ = Mathf.Clamp(actions[2], -1, 1f);

       
        dcoScript.DriveInput(moveX); 
        dcoScript.StrafeInput(moveY); 
        dcoScript.LiftInput(moveZ);   

        float [] distance_obstacle = new float[obstacle.Length];
        float distance = Vector3.Magnitude(goalTrans[GoalSequence].position - agentTrans.position);
        
        UnityEngine.Debug.Log(distance+"change??");
        for (int i = 0; i < obstacle.Length; i++)
        {
            distance_obstacle[i] = Vector3.Magnitude(ObstacleTrans[i].position- agentTrans.position); 
            distance = Vector3.Magnitude(goalTrans[GoalSequence].position - agentTrans.position);

            if(distance_obstacle[i] <= 1.0f)
            {
                SetReward(-1f);     //          -1    ް  
                EndEpisode();       //   Ǽҵ带       

            }
            else if(distance_obstacle[i]<=1.5f & distance>distance_obstacle[i]){ // 가까이 접근했을 때, 단 장애물과 목적지 사이의 거리가 가까울 때는 X
                if(min[i]>distance_obstacle[i])
                    min[i] = distance_obstacle[i]; // 제일 가까워졌을 때의 값 : min
                else
                {
                    float reward = distance_obstacle[i] - min[i]; // 장애물로 부터 멀어진 만큼 보상
                    AddReward(reward); // 거리가 멀어질 때 보상을 주어, 장애물로 부터 멀어지게 만듦, 가가이 접근못함..!
                }
            }
            
        }


        distance = Vector3.Magnitude(goalTrans[GoalSequence].position - agentTrans.position);
        if (distance <= 0.5f)  // 목적지 도착 확인
            {
                SetReward(1.0f); // 도착 시, 보상을 준다.
                check[GoalSequence]=1; // 도착한 목적지의 번호에 맞게 check 배열을 1로 바꾸기
                float closeGoal = 100; // 가까운 목적지 디폴트 값
                for(int i=0;i<goals.Length;i++)//도착하지 않은 나머지 목적지 중, 현재 도착한 목적지와 가장 가까운 목적지를 찾는다.
                {
                    if(check[i]!=1&& Vector3.Magnitude(goalTrans[GoalSequence].position - goalTrans[i].position) < closeGoal)
                    {
                        closeGoal=Vector3.Magnitude(goalTrans[GoalSequence].position - goalTrans[i].position);
                        GoalSequence=i; // 가장 가까운 목적지의 index를 goalsequence로 바꿔줌
                    }
                }
                int c=0;
                for (; c < goals.Length; c++) // 목적지에 모두 도착했는지 확인 하는 부분, for문을 탈출했을 때의 c 값이 목적지의 개수가 된다.
                {
                    if (check[c] != 1)
                    {   
                        break;
                    }   
                }
                if(c==(goals.Length)){ // 모든 목적지에 도착했다면 에피소드 종료
                    SetReward(1.0f);    
                    EndEpisode();     
                }
                else{
                    // 다음 목적지와 드론의 거리 차에 의한 보상
                    preDist = Vector3.Magnitude(goalTrans[GoalSequence].position - agentTrans.position); 
    
                }      
                
            }
            else if (distance > 15f) 
            {
                SetReward(-1f);      
                EndEpisode();             
            }
            else  
            {
                float reward = preDist - distance;  
                                               
                Debug.Log(distance+","+reward);                               
               
                AddReward(reward);
                preDist = distance;
            }
        
    }

    public override void OnEpisodeBegin()
    {
        j=0;                                        // ���Ǽҵ尡 ���۵� �� ȯ���� �ʱ�ȭ�ϴ� �Լ�
        goalChange=0;
        GoalSequence=0;
        area.AreaSetting(); //환경 세팅  
        float [] index = new float[goals.Length];// 목적지와 드론 사이의 거리 배열
        min = new float[obstacle.Length]; // 장애물과 드론 사이의 거리 최소값


        for (int i = 0; i < goals.Length; i++)
        {
            check[i]=0; // 드론이 목적지의 도착했는지 판단하기 위한 배열
            index[i] = Vector3.Magnitude(goalTrans[i].position - agentTrans.position);//목적지와 드론 사이의 거리를 담는 배열 
        }
        
        Array.Sort(index); // 오름차순으로 정렬한다.
        
        for (int i = 0; i < goals.Length; i++) // 각 목적지와 드론 사이의 거리 순서를 매긴다.
        {
            for(int ch=0;ch<goals.Length;ch++)
            {
                if(index[ch]==Vector3.Magnitude(goalTrans[i].position - agentTrans.position))
                    {
                        order[ch]=i; // order 배열 => 드론과 목적지의 거리가 가까운 순서대로 목적지의 index가 차례대로 들어가게 됨
                        UnityEngine.Debug.Log(order[ch]+"..");
                        break;
                    }
            } 
        }
        GoalSequence = order[0];
        for (int i = 0; i < obstacle.Length; i++) // 장애물 사이와 드론 사이의 거리 최소값을 설정하기 위함
        {
            min[i] = Vector3.Magnitude(ObstacleTrans[i].position- agentTrans.position); 
        }
        
        preDist = Vector3.Magnitude(goalTrans[GoalSequence].position - agentTrans.position); //목적지와 드론 사이의 거리를 담아둔다.  
    }
  
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        continuousActionsOut[2] = Input.GetAxis("Mouse ScrollWheel");
    }

    public float DecisionWaitingTime = 5f; // ��ٸ�? �ִ� �ð�? ������?
    float m_currentTime = 0f;              // ���� �ð��� 0���� �ʱ�ȭ 

    public void WaitTimeInference(int action)
    {
        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_currentTime >= DecisionWaitingTime)
            {
                m_currentTime = 0f;
                RequestDecision();
            }
            else
            {
                m_currentTime += Time.fixedDeltaTime;
            }
        }
    }
}
