using UnityEngine;

enum RainState
{
    Raining,
    WaitingForCooldown,
    CanStartRaining
}

public class World : MonoBehaviour
{
    public static World Instance { get; private set; }

        // in game day night cycle is 1 hour

    // |--------------------------------- RAIN ---------------------------------

    public bool isRaining { get; private set; }

    [SerializeField] private ParticleSystem rainParticles;

    [SerializeField] [Min(0)] private float minRainLength = 300f; // 5 minutes
    [SerializeField] [Min(0)] private float maxRainLength = 600f; // 10 minutes

    [SerializeField] [Min(0)] private float minRainDelay = 300f; // 5 minutes
    [SerializeField] [Min(0)] private float maxRainDelay = 1200f; // 20 minutes

    [SerializeField] [Range(0, 1)] private float rainChancePerCheck = 0.05f; // 5% chance to start raining every minute
    [SerializeField] [Min(0)] private float rainCheckFrequency = 60f; // 1 minute
    [SerializeField] [Min(0)] private float rainCheckTimeout = 1800f; // 30 minutes

    private RainState rainState = RainState.WaitingForCooldown;

    private float rainStopTime = 0f; // time rain ends
    private float rainCooldownEnd = 0f; // time when we can start trying to rain
    private float nextRainCheck = 0f; // next time we check for rain chance
    private float rainForceStartTime = 0f; // time when we decide that we waited too long and need to force a rain cycle
    
    // --------------------------------- RAIN ---------------------------------|

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);

         // start the game off as if we just stopped raining i guess, i dont want it to rain instantly
        rainCooldownEnd = Time.time + Random.Range(minRainDelay, maxRainDelay);
        rainParticles.Stop();
        Debug.Log($"Starting rain cycle, waiting for cooldown: {rainCooldownEnd - Time.time} seconds");
    }


    /*Rain

    I want it to rain periodically, there should be a minimum and maximum amount of time it rains for, it should choose randomly between these 2 values
    
    There should be a minimum and maximum amount of time that can pass before it starts raining, with a % chance for it to start raining every minute
    */
    private void UpdateRain()
    {
        switch(rainState)
        {
            case RainState.Raining:
                if(Time.time >= rainStopTime) StopRain();
                break;

            case RainState.WaitingForCooldown:
                if(Time.time >= rainCooldownEnd)
                {
                    rainForceStartTime = Time.time + rainCheckTimeout;
                    rainState = RainState.CanStartRaining;
                    Debug.Log($"Starting rain checks.");
                }
                break;

            case RainState.CanStartRaining:
                if(Time.time >= rainForceStartTime) StartRain();
                else if(Time.time >= nextRainCheck)
                {
                    if(Random.value <= rainChancePerCheck) StartRain();
                    else
                    {
                        nextRainCheck = Time.time + rainCheckFrequency;
                        Debug.Log($"Failed rain check, trying again in {nextRainCheck - Time.time} seconds");
                    }
                }
                break;
        }

        // if RainState.Raining
            // check if current time >= rainStopTime.
                // pick random cooldown time
                // save rainCooldownEnd = current + cooldown
                // set rain state to WaitingForCooldown

        // if RainState.WaitingForCooldown
            // check if current time >= rainCooldownEnd
                // rainForceStartTime = current time + rainCheckTimeout
                // set rain state to CanStartRaining

        // if RainState.CanStartRaining
            // check if current time >= rainForceStartTime
                // set rain state to Raining
                // set rainStopTime to random time
            // check if current time >= nextRainCheck
                // try to rain
                    // if success
                        // set rain state to Raining
                        // set rainStopTime to random time
                    // if fail
                        // set nextRainCheck = current time + rainCheckFrequency
    }

    private void StartRain()
    {
        rainStopTime = Time.time + Random.Range(minRainLength, maxRainLength);
        rainState = RainState.Raining;
        isRaining = true;
        rainParticles.Play();
        Debug.Log($"Starting rain. It will last {rainStopTime - Time.time} seconds.");
    }

    private void StopRain()
    {
        rainCooldownEnd = Time.time + Random.Range(minRainDelay, maxRainDelay);
        rainState = RainState.WaitingForCooldown;
        isRaining = false;
        rainParticles.Stop();
        Debug.Log($"Starting rain cooldown. (cooldown is {rainCooldownEnd - Time.time} seconds.)");
    }

    private void Update()
    {
        UpdateRain();
    }
}
