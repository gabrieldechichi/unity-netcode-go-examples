using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClientDriven.Common
{
    [ExecuteAlways]
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 arm;

        private void LateUpdate()
        {
            if (target != null)
            {
                transform.position = target.position - arm;
            }
        }
    }

}
