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

    public Vector2 Velocity => _movementSystem.Velocity;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _movementSystem = new MovementSystem(_controller, _movementSettings, _groundSensor);
    }

    private void Update()
    {
        var xInput = Input.GetAxisRaw("Horizontal");
        _movementSystem.Update(xInput);

        var jumpForce = 10f;
        var jumpInput = Input.GetButtonDown("Jump");
        if (jumpInput)
            _movementSystem.Jump(jumpForce);
    }

    private void OnDrawGizmos()
    {
        if (_groundSensor) _groundSensor.DrawGizmos(transform.position, Vector2.down);
    }
    private GUIStyle labelStyle;
    private void OnGUI()
    {
        // GUIStyleÇÃèâä˙âª
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle();
            labelStyle.fontSize = 64;
            labelStyle.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(10, 10, 200, 20), $"Velocity: {Velocity.x:F2}, {Velocity.y:F2}", labelStyle);
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

        var serializedObject = new UnityEditor.SerializedObject(player._movementSettings);

        if (_showMovementSettings = UnityEditor.EditorGUILayout.Foldout(_showMovementSettings, "Show Movement Settings"))
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

        if (_showGroundSensor = UnityEditor.EditorGUILayout.Foldout(_showGroundSensor, "Show Ground Sensor"))
        {
            serializedObject = new UnityEditor.SerializedObject(player._groundSensor);
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

#endif
