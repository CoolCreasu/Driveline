using System.Collections;
using UnityEngine;

public class DrivelineTest : MonoBehaviour
{
    [SerializeField] private Driveline driveline; // Reference to the Driveline script
    private float targetAngularVelocity = 100f; // Target angular velocity to stop at

    private void Start()
    {
        driveline.inTestMode = true;
        StartCoroutine(PerformTest());
    }

    private IEnumerator PerformTest()
    {
        // Scenario A: Clutch engaged
        driveline.SetInitialConditions(1000f, 1000f, true);
        yield return StartCoroutine(RunSimulation());

        // Reset for Scenario B
        driveline.SetInitialConditions(1000f, 1000f, false);
        yield return StartCoroutine(RunSimulation());
    }

    private IEnumerator RunSimulation()
    {
        float timeElapsed = 0f;
        float deltaTime = Time.fixedDeltaTime;

        while (driveline.WheelAngularVelocity > targetAngularVelocity)
        {
            driveline.OnFixedUpdate();
            timeElapsed += deltaTime;
            yield return new WaitForFixedUpdate(); // Wait for the next fixed update
        }

        // Log the result directly after the simulation
        Debug.Log($"Time taken to reach {targetAngularVelocity} rad/s: {timeElapsed:F2} seconds");
    }
}