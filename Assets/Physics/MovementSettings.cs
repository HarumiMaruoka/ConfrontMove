using System;
using UnityEngine;

namespace Confront.Physics
{
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "Confront/Physics/MovementSettings")]
    public class MovementSettings : ScriptableObject
    {
        public float _maxSpeed = 10f;
        public float _acceleration = 10f;
        public float _deceleration = 10f;
        public float _gravity = 20f;
        public float _slopeAcceleration = 10f;
        public float _slopeMaxSpeed = 10f;
        public float _jumpTimeoutDelta = 0.2f;
    }
}