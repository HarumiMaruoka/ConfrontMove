using Confront.Physics;
using System;
using Unity.Android.Types;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SamplePlayer : MonoBehaviour
{
    public MovementSettings _movementSettings;
    public GroundSensor _groundSensor;

    private CharacterController _controller;
    private MovementSystem _movementSystem;

    public float _jumpForce = 10f;

    public Vector2 Velocity => _movementSystem.Velocity;
    public GroundSensorResult GroundSensorResult => _groundSensor.CheckGround(transform.position, Vector2.down, _controller.slopeLimit);
    public Vector2 DownhillVector
    {
        get
        {
            var normal = GroundSensorResult.GroundNormal;
            return Vector3.Cross(Vector3.Cross(Vector3.up, normal), normal).normalized;
        }
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _movementSystem = new MovementSystem(_controller, _movementSettings, _groundSensor);
    }

    private void Update()
    {
        var xInput = Input.GetAxisRaw("Horizontal");
        _movementSystem.Update(xInput);

        if (Input.GetButtonDown("Jump")) _movementSystem.Jump(_jumpForce);
    }

    private void OnDrawGizmos()
    {
        if (_controller == null) _controller = GetComponent<CharacterController>();
        if (_groundSensor) _groundSensor.DrawGizmos(transform.position, Vector2.down, _controller.slopeLimit);
    }

    private GUIStyle _labelStyle;
    private GUIStyle _buttonStyle;

    private void OnGUI()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 40;
            _labelStyle.normal.textColor = Color.white;
        }
        GUILayout.Label($"Velocity: {Velocity.x: 000.00;-000.00; 000.00}, {Velocity.y: 000.00;-000.00; 000.00}", _labelStyle);
        GUILayout.Label($"DownhillVector: {DownhillVector.x:00.00}, {DownhillVector.y:00.00}", _labelStyle);

        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 40;
        }
        if (GUILayout.Button("Speed 0.1", _buttonStyle)) Time.timeScale = 0.1f;
        if (GUILayout.Button("Speed 0.5", _buttonStyle)) Time.timeScale = 0.5f;
        if (GUILayout.Button("Speed 1", _buttonStyle)) Time.timeScale = 1;
        if (GUILayout.Button("Speed 2", _buttonStyle)) Time.timeScale = 2;
    }
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(SamplePlayer))]
public class SamplePlayerEditor : UnityEditor.Editor
{
    bool _showMovementSettings = true;
    bool _showGroundSensor = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var player = target as SamplePlayer;
        if (player == null) return;

        if (player._movementSettings != null)
        {
            var serializedObject = new UnityEditor.SerializedObject(player._movementSettings);

            if (_showMovementSettings = UnityEditor.EditorGUILayout.Foldout(_showMovementSettings, "Movement Settings"))
            {
                serializedObject.Update();
                var iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    UnityEditor.EditorGUILayout.PropertyField(iterator);
                }
                serializedObject.ApplyModifiedProperties();
            }
        }

        if (player._groundSensor != null)
        {
            if (_showGroundSensor = UnityEditor.EditorGUILayout.Foldout(_showGroundSensor, "Ground Sensor"))
            {
                var serializedObject = new UnityEditor.SerializedObject(player._groundSensor);
                serializedObject.Update();
                var iterator = serializedObject.GetIterator();
                iterator.NextVisible(true);
                while (iterator.NextVisible(false))
                {
                    UnityEditor.EditorGUILayout.PropertyField(iterator);
                }
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

#endif
