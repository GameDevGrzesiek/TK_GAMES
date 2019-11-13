using UnityEngine;

public class MobFightComponent : MonoBehaviour
{
    public enum MobState
    {
        ToTarget,
        Throw,
        FromTarget
    }

    internal Vector3 StartPos = Vector3.zero;
    internal Vector3 TargetPos = Vector3.zero;

    private MobState m_state;

    void Start()
    {
        m_state = MobState.ToTarget;
    }

    void Update()
    {
        Vector3 curPos = transform.position;

        Vector3 shootTarget = new Vector3(TargetPos.x, GameManager.Instance.Target.transform.position.y, TargetPos.z);
        Vector3 dir = shootTarget - curPos;

        var hits = Physics.RaycastAll(curPos, dir, SceneSettings.ShootingRange, 1 << LayerMask.NameToLayer("Wall"));
        bool canThrow = false;
        if (hits != null && hits.Length > 0 && hits[0].collider != null)
            canThrow = true;

        switch (m_state)
        {
            case MobState.ToTarget:
            {
                curPos = Vector3.MoveTowards(curPos, TargetPos, Time.deltaTime);

                if (canThrow)
                    m_state = MobState.Throw;

                if (Vector3.Distance(curPos, TargetPos) < 2.0f)
                    m_state = MobState.FromTarget;
            } break;

            case MobState.Throw:
            {
                var spearGO = GameObject.Instantiate(GameManager.Instance.SpearPrefab);
                var spear = spearGO.GetComponent<SpearBehavior>();
                spear.transform.position = curPos + SceneSettings.ThrowingPoint;
                spear.transform.rotation = Quaternion.Euler(SceneSettings.ThrowingRotation);
                spear.Throw();
                m_state = MobState.FromTarget;
            } break;

            case MobState.FromTarget:
            {
                curPos = Vector3.MoveTowards(curPos, StartPos, Time.deltaTime);

                if (Vector3.Distance(curPos, StartPos) < 2.0f)
                    m_state = MobState.ToTarget;
            } break;
        };

        transform.position = curPos;
    }
}
