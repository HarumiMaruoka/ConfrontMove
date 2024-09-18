using System;
using UnityEngine;

public class VelocityView : MonoBehaviour
{
    [SerializeField]
    private SamplePlayer _player;
    [SerializeField]
    private TMPro.TextMeshProUGUI _velocityText;

    private void Update()
    {
        var velocity = _player.Velocity;
        _velocityText.text = $"Velocity: {velocity.x:F2}, {velocity.y:F2}";
    }
}
