using Godot;

namespace DungeonDelve.Level.Common.Interactables;

public partial class TorchFlicker : Node3D
{
    [Export] public NodePath LightPath = "OmniLight3D";

    [Export] public float BaseEnergy = 4.0f;
    [Export] public float FlickerAmount = 1.2f;
    [Export] public float FlickerSpeed = 15.0f;

    [Export] public Color BaseColor = new Color(1.0f, 0.55f, 0.15f);
    [Export] public Color FlickerColor = new Color(1.0f, 0.35f, 0.05f);

    private Light3D _light;
    private FastNoiseLite _noise;
    private float _noiseOffset;

    public override void _Ready()
    {
        _light = GetNode<Light3D>(LightPath);

        _noise = new FastNoiseLite();
        _noise.Seed = (int)GD.Randi();
        _noise.Frequency = 1.0f;

        _noiseOffset = (float)GD.RandRange(0.0, 1000.0);
    }

    public override void _Process(double delta)
    {
        _noiseOffset += (float)delta * FlickerSpeed;

        float n = _noise.GetNoise1D(_noiseOffset);

        _light.LightEnergy = BaseEnergy + n * FlickerAmount;

        float t = (n + 1.0f) / 2.0f; 
        _light.LightColor = BaseColor.Lerp(FlickerColor, t * 0.5f);
    }
}