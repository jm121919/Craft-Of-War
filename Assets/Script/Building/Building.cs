using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System;

public class UnitList : List<Unit>
{
    public Action<Unit> OnAddUnit;
    public Action<int> OnRemoveUnit;

    public new void Add(Unit unit)
    {
        base.Add(unit);
        OnAddUnit?.Invoke(unit);
    }

    public new void RemoveAt(int index)
    {
        base.RemoveAt(index);
        OnRemoveUnit?.Invoke(index);
    }
}
public abstract class Building : MonoBehaviourPunCallbacks, IHitAble, IPunObservable
{
    public Image hpCan; 

    PhotonView pv;
    //커스텀 자료구조
    public UnitList spawnList;
    public List<IEnumerator> unitCoolTimeCos;

    public IEnumerator unitProductManagerCo;
    public int index;
    protected int hp;
    public int Hp 
    {
        get => hp;
        set
        {
            hp = value;
            if (hp <= 0)
            {
                this.gameObject.GetComponent<Collider>().enabled = false;
                GameManager.Instance.buildingObjectPool.ReturnPool(this.gameObject, this.index);
                Die();
            }
            hpCan.fillAmount = (float)hp/(float)maxHp;
        }
    }

    public int maxHp;
    public int priority;
    public int Priority { get => priority; set => priority = value; }

    public int cost;


    public override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(unitProductManagerCo);
        pv.RPC("BuildingInitialize", RpcTarget.AllBuffered);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        StopCoroutine(unitProductManagerCo);
    }
    public virtual void Awake()
    {
        pv = GetComponent<PhotonView>();
        pv.RPC("BuildLayer", RpcTarget.AllBuffered);

        spawnList = new UnitList();
        spawnList.OnAddUnit += (Unit unit) => UIMatch();
        spawnList.OnRemoveUnit += (int index) => UIMatch();
        unitCoolTimeCos = new List<IEnumerator>();

        unitProductManagerCo = UnitProductManagerCo();

        priority = 5;
    }



    IEnumerator UnitProductManagerCo()
    {
        while (true)
        {
            if (unitCoolTimeCos.Count > 0)
            {
                IEnumerator currentCo = unitCoolTimeCos[0];
                yield return StartCoroutine(currentCo);
                unitCoolTimeCos.RemoveAt(0);
                spawnList.RemoveAt(0);
            }
            yield return null;
        }
    }

    public string buildingName;
    public abstract  void Die();
    public abstract void Hit();

    public virtual void Hit(IAttackAble attacker)
    {
        Hp -= attacker.Atk;
    }

    public void UIMatch()
    {
        for (int i = 0; i < SlotManager.Instance.unitProductProgressFaceSlots.Count; i++)
        {
            if(GameManager.Instance.rtsController.SelectBuilding != null)
            {
                if(this == GameManager.Instance.rtsController.SelectBuilding.GetComponent<Building>())
                {
                    if (i < spawnList.Count)
                    {
                        SlotManager.Instance.unitProductProgressFaceSlots[i].SetActive(true);
                        SlotManager.Instance.unitProductProgressFaceSlots[i].GetComponent<Image>().sprite = spawnList[i].faceSprite;
                    }
                    else
                    {
                        SlotManager.Instance.unitProductProgressFaceSlots[i].SetActive(false);
                        SlotManager.Instance.unitProductProgressFaceSlots[i].GetComponent<Image>().sprite = null;
                    }
                }
            }
        }
    }

    [PunRPC]
    public void BuildLayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (pv.IsMine)
                this.gameObject.layer = 12;
            else
                this.gameObject.layer = 13;
        }
        else
        {
            if (pv.IsMine)
                this.gameObject.layer = 13;
            else
                this.gameObject.layer = 12;
        }
    }


    [PunRPC]
    public void RPCSetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    [PunRPC]
    public void BuildingInitialize()
    {
        this.maxHp = 300;
        this.hp = maxHp;
        this.gameObject.GetComponent<Collider>().enabled = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Hp);
        }
        else
        {
            this.Hp = (int)stream.ReceiveNext();
        }
    }
}
