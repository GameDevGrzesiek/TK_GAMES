using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 m_curPos;
    Vector3 m_curRot;

    void Start()
    {
        m_curPos = new Vector3(0, 10, -40);
        m_curRot = new Vector3(12, 0, 0);
    }

    void Update()
    {
        this.transform.position = m_curPos;
        this.transform.eulerAngles = m_curRot;
    }

    public void MoveCam(float forward, float right)
    {
        m_curPos += transform.forward * forward + transform.right * right;
    }

    public void RotateCam(float yaw, float pitch)
    {
        m_curRot.x -= pitch;
        m_curRot.y += yaw;
    }
}
