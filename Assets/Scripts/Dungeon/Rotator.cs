﻿using UnityEngine;

namespace Dungeon
{
    public class Rotator : MonoBehaviour {
        [SerializeField]
        float rotateSpeed;

        new Transform transform;

        void Start() {
            transform = GetComponent<Transform>();
        }

        void Update() {
            transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }
    }
}
