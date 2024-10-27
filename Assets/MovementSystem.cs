using System;
using UnityEngine;

namespace Confront.Physics
{
    public class MovementSystem
    {
        private CharacterController _controller;
        private MovementSettings _settings;
        private GroundSensor _groundSensor;

        private GroundSensorResult _groundSensorResult;

        private Vector3 _velocity;

        private float? _jumpForce = null;
        private float _jumpTimeout = 0f;

        private GroundState _previousGroundState;
        private GroundState _groundState;

        public Vector3 Velocity => _velocity;

        public MovementSystem(CharacterController controller, MovementSettings settings, GroundSensor groundSensor)
        {
            _controller = controller;
            _settings = settings;
            _groundSensor = groundSensor;
        }

        public void Jump(float jumpForce)
        {
            _jumpForce = jumpForce;
            _jumpTimeout = _settings._jumpTimeoutDelta;
        }

        private PlayerInput _input;

        public void Update(PlayerInput input)
        {
            _input = input;
            _jumpTimeout -= Time.deltaTime;
            _groundSensorResult = _groundSensor.CheckGround(_controller.transform.position, Vector2.down, _controller.slopeLimit);
            _previousGroundState = _groundState;
            _groundState = _groundSensorResult.GroundState;

            UpdateVelocity();
            ApplyVelocity();
        }

        private void UpdateVelocity()
        {
            switch (_groundState)
            {
                case GroundState.Grounded:
                    UpdateGroundedVelocity();
                    break;
                case GroundState.Abyss:
                    UpdateAbyssVelocity();
                    break;
                case GroundState.SteepSlope:
                    UpdateSteepSlopeVelocity();
                    break;
                case GroundState.InAir:
                    UpdateInAirVelocity();
                    break;
            }
        }

        private float CalculateDirection(float direction)
        {
            if (Mathf.Abs(direction) < 0.01f) return 0;
            return Mathf.Sign(direction);
        }

        private void UpdateGroundedVelocity()
        {
            if (_previousGroundState == GroundState.InAir)
            {
                _velocity.y = 0f;
            }

            // 入力に応じてx速度を更新する。
            var acceleration = _settings._acceleration;
            var deceleration = _settings._deceleration;
            var inputDirection = CalculateDirection(_input.LeftStick.x);

            var isInputZero = inputDirection == 0f;
            var isTurning = IsTurning(inputDirection);
            var velocitySign = Mathf.Sign(_velocity.x);

            float velocity = _velocity.magnitude * velocitySign;

            if (isInputZero)
            {
                velocity = Mathf.Lerp(velocity, 0f, deceleration * Time.deltaTime);
            }
            else if (isTurning)
            {
                velocity = Mathf.Lerp(velocity, 0f, _settings._turnDeceleration * Time.deltaTime);
            }
            else
            {
                velocity = Mathf.Lerp(velocity, _settings._maxSpeed * inputDirection, acceleration * Time.deltaTime);
            }

            var groundNormal = _groundSensorResult.GroundNormal;
            _velocity = Vector3.ProjectOnPlane(new Vector3(velocity, 0f), groundNormal).normalized * Mathf.Abs(velocity);
        }

        private bool IsTurning(float inputDirection)
        {
            return (inputDirection > 0.1f && _velocity.x < -0.1f) || (inputDirection < -0.1f && _velocity.x > 0.1f);
        }

        private void UpdateAbyssVelocity()
        {
            // ななめ移動（崖から滑落する。）
            // もし、x軸方向の速度が滑落側と反対方向の場合、x軸方向の速度をゼロにする。
            var fallDirection = Mathf.Sign(_groundSensorResult.GroundNormal.x);
            var velocityDirection = Mathf.Sign(_velocity.x);
            var acceleration = _settings._abyssGravity;

            var velocity = _velocity.magnitude;
            if (fallDirection != velocityDirection)
            {
                _velocity.x = 0f;
            }

            _velocity += new Vector3(fallDirection, -1f) * acceleration * Time.deltaTime;
        }

        private void UpdateSteepSlopeVelocity()
        {
            // 斜面の角度に合わせて滑落する。
            // もし、前フレームで落下していた場合、落下速度を維持する。
            var groundNormal = _groundSensorResult.GroundNormal;
            var downhillDirection = Vector3.Cross(Vector3.Cross(Vector3.up, groundNormal), groundNormal).normalized;
            var acceleration = _settings._slopeAcceleration;

            if (_previousGroundState != GroundState.SteepSlope)
            {
                var _fallSpeed = _velocity.y;
                _velocity = downhillDirection * _fallSpeed;
            }

            var velocity = _velocity.magnitude;
            velocity += acceleration * Time.deltaTime;
            _velocity = downhillDirection * velocity;
        }

        private void UpdateInAirVelocity()
        {
            // 空中での移動。
            // 重力を適用する。
            // 入力に応じて横移動を行う。
            var acceleration = _settings._inAirAcceleration;
            var deceleration = _settings._inAirDeceleration;
            var maxSpeed = _settings._inAirMaxSpeed;
            var gravity = _settings._gravity;
            var isInputZero = Mathf.Abs(_input.LeftStick.x) < 0.01f;

            if (isInputZero)
            {
                _velocity.x = Mathf.MoveTowards(_velocity.x, 0f, deceleration * Time.deltaTime);
            }
            else
            {
                var inputDirection = Mathf.Sign(_input.LeftStick.x);
                _velocity.x = Mathf.MoveTowards(_velocity.x, maxSpeed * inputDirection, acceleration * Time.deltaTime);
            }

            _velocity.y -= gravity * Time.deltaTime;
        }

        private void ApplyVelocity()
        {
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}