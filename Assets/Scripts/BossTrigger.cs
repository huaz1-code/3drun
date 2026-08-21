using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Tooltip("Boss对象（可选，自动查找）")]
    public GameObject bossObject;

    [Tooltip("Boss血条管理器")]
    public BossHealthBarManager bossHealthBarManager;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            TriggerBossBattle();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (hasTriggered) return;

        if (other.gameObject.CompareTag("Player"))
        {
            TriggerBossBattle();
        }
    }

    private void TriggerBossBattle()
    {
        hasTriggered = true;
        Debug.Log("触发了boss战！");

        DestroyRoad();
        DestroyNPCs();
        ActivateBoss();
    }

    private void DestroyRoad()
    {
        GameObject road = GameObject.Find("road");
        if (road != null)
        {
            Destroy(road);
            Debug.Log("销毁了 road 对象");
        }
        else
        {
            Debug.LogWarning("没有找到 road 对象");
        }
    }

    private void DestroyNPCs()
    {
        GameObject npc = GameObject.Find("NPC");
        if (npc != null)
        {
            Destroy(npc);
            Debug.Log("销毁了 NPC 对象");
        }

        GameObject npcWalk = GameObject.Find("NPCWalk");
        if (npcWalk != null)
        {
            Destroy(npcWalk);
            Debug.Log("销毁了 NPCWalk 对象");
        }

        GameObject[] npcsByTag = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject n in npcsByTag)
        {
            Destroy(n);
        }
        Debug.Log($"通过标签销毁了 {npcsByTag.Length} 个 NPC 对象");
    }

    private void ActivateBoss()
    {
        if (bossObject == null)
        {
            bossObject = GameObject.Find("Boss");
        }

        if (bossObject != null)
        {
            bossObject.SetActive(true);
            Boss bossComponent = bossObject.GetComponent<Boss>();
            if (bossComponent != null)
            {
                bossComponent.StartChasing();
            }
            else
            {
                Debug.LogWarning("Boss对象上没有Boss组件！");
            }

            if (bossHealthBarManager != null)
            {
                bossHealthBarManager.SetBossObject(bossObject);
                bossHealthBarManager.Initialize();
                bossHealthBarManager.ShowBossHealthBar();
            }
            else
            {
                Debug.LogWarning("BossTrigger: 未设置Boss血条管理器！");
            }
        }
        else
        {
            Debug.LogWarning("没有找到Boss对象！请在场景中创建名为'Boss'的对象。");
        }
    }
}
