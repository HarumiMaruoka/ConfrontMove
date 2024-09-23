using System;
using UnityEngine;

namespace Confront.Physics
{
    [CreateAssetMenu(fileName = "GroundSensor", menuName = "Confront/Physics/GroundSensor")]
    public class GroundSensor : ScriptableObject
    {
        [SerializeField]
        private LayerMask _groundLayer;

        [SerializeField]
        public float _groundCheckSphereRadius = 0.15f;
        [SerializeField]
        private Vector2 _groundCheckSphereOffset = new Vector2(0f, 1f);
        [SerializeField]
        private float _groundCheckDistance = 0.87f;

        [SerializeField]
        private Vector2 _abyssCheckSphereOffset = new Vector2(0f, 1f);
        [SerializeField]
        private float _abyssCheckSphereRadius = 0.1f;
        [SerializeField]
        private float _abyssCheckDistance = 1.2f;

        public GroundSensorResult CheckGround(Vector2 position, Vector2 direction, float slopeLimit)
        {
            var result = new GroundSensorResult();

            var groundCheckSpherePosition = position + _groundCheckSphereOffset;
            var abyssCheckSpherePosition = position + _abyssCheckSphereOffset;

            // 足元にレイを飛ばして、ヒットした場合には、地面にいると判定する。
            result.IsGrounded = UnityEngine.Physics.SphereCast(groundCheckSpherePosition, _groundCheckSphereRadius, direction, out var groundHit, _groundCheckDistance, _groundLayer);
            // 足元に小さなレイを飛ばして、ヒットしなかった場合には、崖にいると判定する。
            result.IsAbyss = !UnityEngine.Physics.SphereCast(abyssCheckSpherePosition, _abyssCheckSphereRadius, direction, out var abyssHit, _abyssCheckDistance, _groundLayer);

            if (result.IsGrounded)
            {
                result.GroundNormal = groundHit.normal;
                result.GroundPoint = groundHit.point;
                result.IsOverSlope = Vector3.Angle(Vector3.up, groundHit.normal) > slopeLimit;
            }

            return result;
        }

        public void DrawGizmos(Vector2 position, Vector2 direction, float slopeLimit)
        {
            var result = CheckGround(position, direction, slopeLimit);

            if (result.IsGrounded) Gizmos.color = new Color(1, 0, 0, 0.5f);
            else Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawSphere(position + _groundCheckSphereOffset + direction * _groundCheckDistance, _groundCheckSphereRadius);

            if (result.IsAbyss) Gizmos.color = new Color(0, 1, 1, 0.5f);
            else Gizmos.color = new Color(1, 0, 1, 0.5f);
            Gizmos.DrawSphere(position + _abyssCheckSphereOffset + direction * _abyssCheckDistance, _abyssCheckSphereRadius);
        }
    }

    public struct GroundSensorResult
    {
        public bool IsGrounded;
        public bool IsAbyss;
        public bool IsOverSlope;
        public Vector2 GroundNormal;
        public Vector2 GroundPoint;
    }
}