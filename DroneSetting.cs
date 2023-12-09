using UnityEngine;
using Unity.MLAgents;
using System.Diagnostics;
using System;
using System.Security.Cryptography.X509Certificates;
 
public class DroneSetting : MonoBehaviour
{
    public GameObject DroneAgent; // ��� ������Ʈ ������Ʈ
    public GameObject [] Goals;       // ������ ������Ʈ
    public GameObject [] Obstacle;   // ��ֹ� ������Ʈ

    Vector3 areaInitPos;          // Area ������Ʈ�� �ʱ� ��ġ(�������� �ʱ� ��ġ�� ����� �� ���� ��ġ�� �����)
    Vector3 droneInitPos;         // �� ���Ǽҵ尡 ����Ǹ�, ����� ��ġ�� �ʱ� ������ �����ϸ�, �� ��, ����� �ʱ� ��ġ ������ ������
    Quaternion droneInitRot;      // �� ���Ǽҵ尡 ����Ǹ�, ����� ȸ������ �ʱ� ������ �����ϸ�, �� ��, ����� �ʱ� ȸ���� ������ ������

    EnvironmentParameters m_ResetParams; // �н��� �ʿ��� ���ڵ��� ����

    public Transform AreaTrans;  // Area ������Ʈ�� ��ġ, ȸ�� ������ �����ϱ� ���� Area ������Ʈ�� Transform ���۷����� ����
    public Transform DroneTrans; // Drone ������Ʈ�� ��ġ, ȸ�� ������ �����ϱ� ���� Area ������Ʈ�� Transform ���۷����� ����
    public Transform [] GoalTrans;  // ������(Goal) ������Ʈ�� ��ġ, ȸ�� ������ �����ϱ� ���� Area ������Ʈ�� Transform ���۷����� ����
    public Transform [] ObstacleTrans;

    private Rigidbody DroneAgent_Rigidbody; // Drone ������Ʈ�� ������ RigidBody ������Ʈ�� �ҷ��� ������Ʈ�� ���� ���� �ӵ��� ����

    void Start() // ����Ƽ �����Լ�, DroneSetting.cs �� ����� ��, ó�� �ѹ��� ����
    {
        UnityEngine.Debug.Log("start함수");
        UnityEngine.Debug.Log(m_ResetParams);

        AreaTrans = gameObject.transform;   // ��ũ��Ʈ�� ����Ǵ� ������Ʈ(��ũ��Ʈ�� ����Ǵ� ���� ������Ʈ ��ü)�� Transform ������ �ε�
                                            // gameObject�� Unity���� ����� �Ӽ�(i.e., Area object)
        DroneTrans = DroneAgent.transform;  // ��� ������Ʈ�� Transform ������ �ε�
        GoalTrans = new Transform[Goals.Length];
        for (int i = 0; i < Goals.Length; i++)
        {
            //UnityEngine.Debug.Log(Goals[i]);
            GoalTrans[i] = Goals[i].transform;
        }
        
        ObstacleTrans = new Transform[Obstacle.Length];
        for (int i = 0; i < Obstacle.Length; i++)
        {
            //UnityEngine.Debug.Log(Obstacle[i]);
            ObstacleTrans[i] = Obstacle[i].transform;
        }
        

        areaInitPos = AreaTrans.position;     // Area ��ġ ���� ����(--Trans�� ���� ������)
        droneInitPos = DroneTrans.position;   // Drone ��ġ ���� ����(--Trans�� ���� ������)
        droneInitRot = DroneTrans.rotation;   // Drone ȸ�� ���� ����(--Trans�� ���� ������)

        DroneAgent_Rigidbody = DroneAgent.GetComponent<Rigidbody>();  // DroneAgent�� RigidBody ������Ʈ ����
    }

    public void AreaSetting()  // ȯ���� ���� ������ �� ��� ��Ȳ�� �ʱ�ȭ
    {
      
        DroneAgent_Rigidbody.velocity = Vector3.zero; // ����� �ӵ��� 0���� �ʱ�ȭ
        DroneAgent_Rigidbody.angularVelocity = Vector3.zero;  // ����� ���ӵ��� 0���� �ʱ�ȭ

        DroneTrans.position = droneInitPos;     // �ʱ⿡ ������ ����� �ʱ���ġ�� �ʱ�ȭ
        DroneTrans.rotation = droneInitRot;     // �ʱ⿡ ������ ����� �ʱ�ȸ�� ������ �ʱ�ȭ
        for (int i = 0; i < Goals.Length; i++)
        {
            
            GoalTrans[i].position = areaInitPos + new Vector3(UnityEngine.Random.Range(-7f, 7f), UnityEngine.Random.Range(-7f, 7f), UnityEngine.Random.Range(7f, 7f));
            // 占쏙옙占쏙옙占쏙옙 占쏙옙치 占십깍옙화 X, Y, Z 占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙 -5~5 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙占싹곤옙 占쏙옙占쏙옙占쏙옙
        }

        float max = Vector3.Magnitude(GoalTrans[0].position- DroneTrans.position);
        int maxIndex=0;
        float min = Vector3.Magnitude(GoalTrans[0].position- DroneTrans.position);
        int minIndex=0;
        for (int i = 0; i < Goals.Length; i++)
        { 
            if(max<Vector3.Magnitude(GoalTrans[i].position- DroneTrans.position))
            {
                max=Vector3.Magnitude(GoalTrans[i].position- DroneTrans.position);
                maxIndex=i;
            }    
            if(min>Vector3.Magnitude(GoalTrans[i].position- DroneTrans.position))
            {
                min=Vector3.Magnitude(GoalTrans[i].position- DroneTrans.position);
                minIndex=i;
            }  
        }
        
        
        float minX = Math.Min(GoalTrans[minIndex].position.x, GoalTrans[maxIndex].position.x); // X 占쏙옙 占쌍소곤옙
        float maxX = Math.Max(GoalTrans[minIndex].position.x, GoalTrans[maxIndex].position.x);  // X 占쏙옙 占쌍대값
        float minY = Math.Min(GoalTrans[minIndex].position.y, GoalTrans[maxIndex].position.y); // Y 占쏙옙 占쌍소곤옙
        float maxY = Math.Max(GoalTrans[minIndex].position.y, GoalTrans[maxIndex].position.y);  // Y 占쏙옙 占쌍대값
        float minZ = Math.Min(GoalTrans[minIndex].position.z, GoalTrans[maxIndex].position.z); // Z 占쏙옙 占쌍소곤옙
        float maxZ = Math.Max(GoalTrans[minIndex].position.z, GoalTrans[maxIndex].position.z);  // Z 占쏙옙 占쌍대값

        // 장애물의 위치 설정
        for (int i = 0; i < Obstacle.Length; i++)
        {
            ObstacleTrans[i].position = new Vector3(UnityEngine.Random.Range(minX+1, maxX-1), UnityEngine.Random.Range(minY+1, maxY-1), UnityEngine.Random.Range(minZ+1, maxZ-1));
            if(Vector3.Magnitude(ObstacleTrans[i].position- DroneTrans.position)<=1.5)
            {
                i--;
            }
        }

    }
}
