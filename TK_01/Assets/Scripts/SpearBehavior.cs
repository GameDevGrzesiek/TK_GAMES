using UnityEngine;

public class SpearBehavior : MonoBehaviour
{
    Rigidbody m_rBody;

    void Start()
    {
        UpdateRigidBody();
    }

    void Awake()
    {
        UpdateRigidBody();
    }

    void OnEnable()
    {
        UpdateRigidBody();
        m_rBody.rotation = Quaternion.identity;
        m_rBody.velocity = Vector3.zero;
        m_rBody.angularVelocity = Vector3.zero;
        m_rBody.transform.rotation = Quaternion.identity;
    }

    void UpdateRigidBody()
    {
        if (!m_rBody)
            m_rBody = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (m_rBody)
            m_rBody.transform.forward = Vector3.Slerp(transform.forward, m_rBody.velocity.normalized, Time.deltaTime * 15);

        if (transform.position.y < 0)
            PoolManager.Instance.SpearPool.ReturnToPool(this);
    }

    public void Throw()
    {
        m_rBody.AddForce(transform.forward * 5000);
    }
}
