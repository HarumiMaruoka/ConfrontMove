using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Confront.Physics
{
    public class MovementSystem
    {
        private readonly CharacterController _controller;
        private readonly GroundSensor _groundSensor;

        private MovementSettings _settings;
        private Vector3 _velocity;

        public Vector3 Velocity => _velocity;

        public MovementSystem(CharacterController controller, MovementSettings settings, GroundSensor groundSensor)
        {
            _controller = controller;
            _settings = settings;
            _groundSensor = groundSensor;
        }

        public void Update(float xInput)
        {
            _jumpTimeout -= Time.deltaTime;

            CaluculateHorizontal(xInput);
            CalculateVertical();
            CaluculateSlope();
            ApplyVelocity();
        }

        private float? _jumpForce = null;
        private float _jumpTimeout = 0f;

        public void Jump(float jumpForce)
        {
            _jumpForce = jumpForce;
            _jumpTimeout = _settings._jumpTimeoutDelta;
        }

        private float CalculateDirection(float x) => Mathf.Abs(x) < 0.1f ? 0f : x > 0 ? 1f : -1f;

        private void CaluculateHorizontal(float xInput)
        {
            var acceleration = _settings._acceleration;
            var deceleration = _settings._deceleration;
            var maxSpeed = _settings._maxSpeed * Mathf.Sign(xInput);

            var isInputZero = Mathf.Abs(xInput) < 0.1f;

            if (isInputZero)
            {
                _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, deceleration * Time.deltaTime);
            }
            else
            {
                _velocity.x = Mathf.MoveTowards(_velocity.x, maxSpeed, acceleration * Time.deltaTime);
            }

            var groundCheckResult = _groundSensor.CheckGround(_controller.transform.position, Vector2.down);
            var angle = Vector2.Angle(Vector2.up, groundCheckResult.GroundNormal);
            if (groundCheckResult.IsGrounded && !groundCheckResult.IsAbyss && angle <= _controller.slopeLimit && _jumpTimeout <= 0f)
            {
                _velocity = Vector3.ProjectOnPlane(_velocity, groundCheckResult.GroundNormal);
            }
        }

        private void CalculateVertical()
        {
            var groundCheckResult = _groundSensor.CheckGround(_controller.transform.position, Vector2.down);
            var angle = Vector2.Angle(Vector2.up, groundCheckResult.GroundNormal);
            var isGrounded = groundCheckResult.IsGrounded && !groundCheckResult.IsAbyss && angle <= _controller.slopeLimit;

            if (_jumpForce != null)
            {
                _velocity.y = _jumpForce.Value;
                _jumpForce = null;
            }
            else if (!isGrounded)
            {
                _velocity.y -= _settings._gravity * Time.deltaTime;
            }
        }

        private void CaluculateSlope()
        {
            var slopeAcceleration = _settings._slopeAcceleration;
            var groundCheckResult = _groundSensor.CheckGround(_controller.transform.position, Vector2.down);
            var isSlope = false;
            var downhillVector = Vector3.zero;

            if (groundCheckResult.IsGrounded)
            {
                var angle = Vector2.Angle(Vector2.up, groundCheckResult.GroundNormal);

                if (angle > _controller.slopeLimit || groundCheckResult.IsAbyss)
                {
                    isSlope = true;
                    var normal = groundCheckResult.GroundNormal;
                    downhillVector = Vector3.Cross(Vector3.Cross(Vector3.up, normal), normal).normalized;
                }

                var maxSpeed = _settings._slopeMaxSpeed;

                if (isSlope)
                {
                    // 斜面の向きと進行方向が逆の場合、速度を0にする。
                    if (Mathf.Sign(downhillVector.x) != Mathf.Sign(_velocity.x)) _velocity.x = 0f;
                    if (_velocity.y > 0f) _velocity.y = 0f;

                    _velocity = Vector3.MoveTowards(_velocity, downhillVector * maxSpeed, slopeAcceleration * Time.deltaTime);
                }
            }
        }

        private void ApplyVelocity()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}