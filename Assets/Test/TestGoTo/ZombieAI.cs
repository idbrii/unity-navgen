using idbrii.navgen.runtime;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;

public class ZombieAI : MonoBehaviour
{
    //Debug
    PathDebuger pathDebuger;

    //PathMove
    bool NavVaild { get { return NavLinkGenerator_Runtime.HasNavMesh; } }
    NavMeshAgent navMeshAgent;
    NavMeshPath tempPath;

    bool ChasingState = false; 

    //==================================公开==================================
    public bool SetChasing;
    [SerializeField]
    Vector3 VaildPos;
    [SerializeField]
    Vector3 unVaildPos;
    [SerializeField]
    Vector3 currentTargetPos;
    [SerializeField]
    Vector3 _UpdateTargetPos;
    [SerializeField]
    float lastOutsideTick;
    public Vector3 UpdateTargetPos
    {
        get { return _UpdateTargetPos; }
        set
        {
            if (_UpdateTargetPos != value && IsPosChange(_UpdateTargetPos, value))
            {
                if (NavVaild && SetChasing)
                {
                    //Debug.Log("目标位置改变,触发重新计算Path");
                    ReGoto(value);
                }
            }
            _UpdateTargetPos = value;
            //else
            //    Debug.Log("Pass不动点");
        }
    }
    void Awake()
    {
        //Debugger
        if (pathDebuger == null)
            pathDebuger = gameObject.AddComponent<PathDebuger>();

        if (navMeshAgent == null)
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.enabled = true;

        tempPath = new NavMeshPath();

        //注册路网更新
        NavLinkGenerator_Runtime.RebuildNavMesh_CallBack += OnNavMeshRebuild;
    }

    void Update()
    {
        //状态检测 更新
        if (navMeshAgent.isStopped)
        {
            ChasingState = false;
            Debug.Log("停止:" + navMeshAgent.isStopped);
        }

        //外转内不转(要让内部启动)
        if (SetChasing && !ChasingState && (_UpdateTargetPos != unVaildPos))
            //Debug.Log("外转内不转:"+ SetChasing + !ChasingState + (_UpdateTargetPos.ToString() + unVaildPos));
            ReGoto(_UpdateTargetPos);
        

        //内转外不转(要让内部停止)
        if (!SetChasing && ChasingState)
            EndGoTo();
    }
    void OnNavMeshRebuild()
    {
        //Debug.Log("监听到路网更新:"+ navMeshAgent.isPathStale);
        //路网有效, 在追击中
        if (ChasingState && NavVaild)
        {
            EndGoTo();
            SetChasing = true;
            //StartGoto(UpdateTargetPos);//重新计算路径追击
        }
    }

    bool IsPosChange(Vector3 old, Vector3 newpos)
    {
        //if (Vector3.Distance(transform.position, newpos) > 30)
        //    return false;

        var dis = Vector3.Distance(old, newpos);
        bool changed = dis > 1f;
        //Debug.Log("IsPosChange:"+ changed+"|"+ dis);
        return true;
    }

    void ReGoto(Vector3 thisPos)
    {
        if (!NavVaild)
        {
            Debug.LogError("路网未初始化,StartGoto Pass:" + NavVaild);
            return;
        }

        if (thisPos == unVaildPos)
        {
            Debug.LogError("Pass无效点");
            return;
        }

        if (navMeshAgent.isOnOffMeshLink)
            return;//在桥上时不重新计算

        //计算路径
        bool recalCulate = navMeshAgent.CalculatePath(thisPos, tempPath);

        //目标在路网之外
        if (!recalCulate)
        {
            float thistime = Time.time;
            if (thistime - lastOutsideTick < 1)
            {
                //Debug.Log("快速退出");
                return;
            }
            else
            {
                lastOutsideTick = thistime;
                //Debug.Log("网格外Tick:" + thistime);
            }
        }
        bool resetPath = navMeshAgent.SetPath(tempPath);
        Debug.Log("Tick");

        //规划路径失败
        if (!recalCulate || !resetPath)
        {
            //Debug.Log("计算失败");
            //重计算
            if (NavMesh.SamplePosition(thisPos, out NavMeshHit nearstPoint, 10, NavMesh.AllAreas))
            {
                //使用最近点
                thisPos = nearstPoint.position;
                Debug.Log("使用最近点:" + thisPos);
            }
            else
            {
                //没有最近点
                //有有效点
                if (VaildPos != Vector3.zero)
                {
                    thisPos = VaildPos;
                    if (VaildPos == currentTargetPos)
                    {
                        Debug.Log("忽略重复的有效点");
                        return;
                    }
                    else
                        Debug.Log("使用有效点:" + thisPos + recalCulate);
                    //Debug.Log(navMeshAgent.destination.ToString() + VaildPos);
                    //if (navMeshAgent.pathEndPosition == VaildPos)
                    //    Debug.Log("有效点早已被设定");
                }
                else {
                    //没有有效点
                    unVaildPos = thisPos;//记录这个无效点(没有最近点, 没有有效点)
                    EndGoTo();//停止
                    Debug.LogError("停止: 没有最近点,没有有效点");
                    return;
                }
            }

            //重规划(最近点 或 有效点)
            recalCulate = navMeshAgent.CalculatePath(thisPos, tempPath);
            resetPath = navMeshAgent.SetPath(tempPath);
        }

        //成功(最近点 或 有效点)
        if (recalCulate && resetPath)
        {
            currentTargetPos = VaildPos;
            VaildPos = thisPos;//记录有效
            pathDebuger.DebugPath(tempPath);
            ChasingState = true;
        }
        //失败
        else
        {
            unVaildPos = thisPos;//记录无效
            EndGoTo();
            Debug.LogError("修正点设置失败: " + recalCulate + "|" + resetPath + "|" + navMeshAgent.pathStatus);
            Debug.DrawLine(thisPos, new Vector3(0, 100, 0), Color.red, 3);
        }
    }

    void EndGoTo() {
        ChasingState = false;
        navMeshAgent.ResetPath();
        pathDebuger.OnDestroy();
    }

}
