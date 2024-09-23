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

        private float _xSpeed;
        private Vector2 _xSpeed2;

        private float _ySpeed;

        private float _slopeSpeed;
        private Vector2 _slopeSpeed2;

        private Vector3 _velocity;

        private float? _jumpForce = null;
        private float _jumpTimeout = 0f;

        public Vector2 Velocity => new Vector2(_velocity.x, _velocity.y);

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

        public void Update(float xInput)
        {
            _jumpTimeout -= Time.deltaTime;
            _groundSensorResult = _groundSensor.CheckGround(_controller.transform.position, Vector2.down, _controller.slopeLimit);

            CaluculateHorizontal(xInput);
            CalculateVertical();
            CaluculateSlope();
            ApplyVelocity();
        }

        private void CaluculateHorizontal(float xInput)
        {
            var acceleration = _settings._acceleration;
            var deceleration = _settings._deceleration;
            var turnDeceleration = _settings._turnDeceleration;
            var maxSpeed = _settings._maxSpeed;

            var isGrounded = _groundSensorResult.IsGrounded;
            var isAbyss = _groundSensorResult.IsAbyss;
            var isOverSlope = _groundSensorResult.IsOverSlope;

            var isInputZero = Mathf.Abs(xInput) < 0.1f;
            var inputDir = Mathf.Abs(xInput) < 0.1f ? 0f : Mathf.Sign(xInput);
            var isTurnning = xInput > 0.1f && _xSpeed < -0.1f || xInput < -0.1f && _xSpeed > 0.1f;

            if (isTurnning)
            {
                _xSpeed = Mathf.MoveTowards(_xSpeed, 0f, turnDeceleration * Time.deltaTime);
            }
            if (isInputZero || isOverSlope)
            {
                _xSpeed = Mathf.MoveTowards(_xSpeed, 0f, deceleration * Time.deltaTime);
            }
            else
            {
                _xSpeed = Mathf.MoveTowards(_xSpeed, inputDir, acceleration * Time.deltaTime);
            }

            var normal = _groundSensorResult.GroundNormal;
            _slopeSpeed2 = Vector3.Cross(Vector3.Cross(Vector3.up, normal), normal).normalized;
            if (isGrounded && !isOverSlope && !isAbyss)
            {
                var groundNormal = _groundSensorResult.GroundNormal;
                _xSpeed2 = Vector3.ProjectOnPlane(new Vector3(_xSpeed, 0f), groundNormal).normalized * Mathf.Abs(_xSpeed) * maxSpeed;
            }
            else
            {
                _xSpeed2 = new Vector2(_xSpeed * maxSpeed, 0f);
            }
        }

        private bool _lastGrounded;

        private void CalculateVertical()
        {
            var isGrounded = _groundSensorResult.IsGrounded;
            var isOverSlope = _groundSensorResult.IsOverSlope;

            if (_jumpForce != null)
            {
                _ySpeed = _jumpForce.Value;
                _jumpForce = null;
            }
            else if (_lastGrounded && !isGrounded && _jumpTimeout < 0f)
            {
                _ySpeed = -_slopeSpeed;
            }
            else if (!isGrounded)
            {
                _ySpeed -= _settings._gravity * Time.deltaTime;
            }

            _lastGrounded = isGrounded;
        }

        private void CaluculateSlope()
        {
            if (_velocity.y > 0f) return;

            var isGrounded = _groundSensorResult.IsGrounded;
            var isOverSlope = _groundSensorResult.IsOverSlope;
            var isAbyss = _groundSensorResult.IsAbyss;

            var normal = _groundSensorResult.GroundNormal;
            var downhillVector = Vector3.Cross(Vector3.Cross(Vector3.up, normal), normal).normalized;

            if (isGrounded && isOverSlope || isAbyss)
            {
                _slopeSpeed += _settings._slopeAcceleration * Time.deltaTime;
                _slopeSpeed = Mathf.Clamp(_slopeSpeed, _settings._slopeMinSpeed, _settings._slopeMaxSpeed);
            }
            else
            {
                _slopeSpeed = 0f;
            }
        }

        private void ApplyVelocity()
        {
            _velocity = _xSpeed2 + new Vector2(0f, _ySpeed) + _slopeSpeed2 * _slopeSpeed;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}