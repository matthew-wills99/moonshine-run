using UnityEngine;

public class RainBarrel : Interactable
{
    private float waterGainPerSecond = 0.5f;
    private float maximumWaterCapacity = 100f;

    private float timer = 0f;

    public float waterVolume { get; private set; } = 0f;

    public override void InteractHold()
    {
        Debug.Log($"Interact hold on rain barrel ({transform.position.x}, {transform.position.y})");
    }

    public override void InteractPress()
    {
        //Debug.Log($"Interact press on rain barrel ({transform.position.x}, {transform.position.y})");
        Debug.Log($"Water barrel has ({waterVolume} / {maximumWaterCapacity}) water.");
    }

    private void Update()
    {
        if(World.Instance.isRaining)
        {
            FillRainBarrel();
            Debug.Log("Raining");
        }
    }

    private void FillRainBarrel()
    {
        timer += Time.deltaTime;
        if(timer >= 1f && waterVolume < maximumWaterCapacity)
        {
            timer = 0f;
            waterVolume += waterGainPerSecond;
        }

        if(waterVolume > maximumWaterCapacity) waterVolume = maximumWaterCapacity;
    }

}
