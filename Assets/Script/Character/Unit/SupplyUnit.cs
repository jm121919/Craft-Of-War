using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SupplyUnit : Unit
{
    //행동트리
    SequenceNode btRootNode;
    ////////
    float miningCool = 3f;
    public Vector3 mineTf = Vector3.zero;//시작, 클릭될때 자원클릭되면 rts컨트롤러해서 벡터를 넣어줌
    public Vector3 nexusTf = Vector3.zero;//끝


    private bool isMineClicked;
    public bool IsMineClicked
    {
        get { return isMineClicked; }
        set { isMineClicked = value; }
    }
    public new void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        StartCoroutine(miningCo());
    }

    public new void Update()
    {
        base.Update();
        //btRootNode.Evaluate();
    }

    //채굴루틴 행동트리
    public void MiningBTInit()
    {
        btRootNode = new SequenceNode();

        ActionNode miningReadyCheck = new ActionNode();
        ActionNode miningAction = new ActionNode();
        ActionNode goNexus = new ActionNode();
        btRootNode.Add(miningReadyCheck);
        btRootNode.Add(miningAction);
        
        //액션 구현부
        miningReadyCheck.action = () =>
        {
            if (IsMineClicked)
            {
                return BTNode.State.SUCCESS;
            }
            return BTNode.State.FAIL;
        };

        miningAction.action = () =>
        {
            float curCool = miningCool;

            Collider[] cols = Physics.OverlapSphere(mineTf, 100f);
            //주변 넥서스 찾기, 범위를 넓게 줘야함
            foreach (var targetBuilding in cols)
            {
                if (targetBuilding.TryGetComponent(out NexusBuilding nexusBuilding))
                {
                    nexusTf = nexusBuilding.transform.position;
                }
            }

            agent.SetDestination(mineTf);

            while (curCool > 0)
            {
                curCool -= Time.fixedDeltaTime;
            }


            return BTNode.State.SUCCESS;
        };

        goNexus.action = () =>
        {
            agent.SetDestination(nexusTf);

            while (agent.remainingDistance < 5f)
            {

            }

            return BTNode.State.SUCCESS;
        };
    }

    public class WaitForClickedTarget : CustomYieldInstruction
    {
        public SupplyUnit supplyUnit;
        public WaitForClickedTarget(SupplyUnit supplyUnit)
        {
            this.supplyUnit = supplyUnit;
        }
        public override bool keepWaiting
        {
            get
            {
                return !supplyUnit.IsMineClicked;
            }
        }
    }

    public IEnumerator miningCo()
    {
        float curCool = miningCool;

        while (true)
        {
            yield return null;
            //자원옮기면서 순환하는 코드
            
            //클릭될때 불변수가 바뀌고 껏다 켜주는 역할
            yield return new WaitForClickedTarget(this);

            Collider[] cols = Physics.OverlapSphere(mineTf, 100f);
            //주변 넥서스 찾기, 범위를 넓게 줘야함
            foreach (var targetBuilding in cols)
            {
                if (targetBuilding.TryGetComponent(out NexusBuilding nexusBuilding))
                {
                    nexusTf = nexusBuilding.transform.position;
                }
            }

            agent.SetDestination(mineTf);

            while (true)
            {
                Debug.LogWarning("코루틴 첫 while 도착");
                //리소스에서 도착했을때
                if (Vector3.Distance(transform.position, mineTf) <= 5f)
                {
                    Debug.LogWarning(nexusTf);
                    curCool -= Time.fixedDeltaTime;
                    if (curCool <= 0)
                    {
                        agent.SetDestination(nexusTf);
                        curCool = miningCool;
                    }
                }

                //넥서스에 도착했을때
                if (Vector3.Distance(transform.position, nexusTf) <= 8f)
                {
                    agent.SetDestination(mineTf);
                    GameManager.Instance.Tree += 10;
                    //디버깅용
                }
                yield return null;
            }

        }
    }

    public override void InitStats()
    {
        throw new NotImplementedException();
    }

    public override void Attack(IHitAble target)
    {
        throw new NotImplementedException();
    }

    public override void Hit(IAttackAble attacker)
    {
        throw new NotImplementedException();
    }

    public override void Die()
    {
        throw new NotImplementedException();
    }
}

