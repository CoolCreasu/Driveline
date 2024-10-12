using UnityEngine;

public class Driveline : MonoBehaviour
{
    // Engine parameters
    [SerializeField] private float engineFriction = 100.0f; // Nm constant value for engine friction
    [SerializeField] private float engineInertia = 1.5f; // Engine inertia (kg*m^2)
    [SerializeField] private float engineTorqueRating = 300.0f; // Maximum torque the engine can produce (Nm)

    // Wheel parameters
    [SerializeField] private float wheelInertia = 1.156f; // Wheel inertia (kg*m^2)
    [SerializeField] private float wheelFriction = 100.0f; // Nm constant value for wheel friction

    // Clutch parameters
    [SerializeField] private float clutchMaxTorque = 50.0f; // Maximum torque the clutch can apply
    [SerializeField] private float clutchCoefficientStatic = 150.0f; // Static friction coefficient (Nm)
    [SerializeField] private float clutchCoefficientDynamic = 100.0f; // Dynamic friction coefficient (Nm)
    [SerializeField] private float clutchSlipThreshold = 0.1f; // Threshold to begin slipping

    // Clutch engagement value (0.0 to 1.0)
    [SerializeField] private float clutchEngagement = 1.0f; // Start fully engaged

    // Throttle parameter
    [SerializeField] private float throttle = 0.0f; // Throttle value (0.0 to 1.0)

    private float engineAngularVelocity = 0.0f; // Current engine angular velocity
    private float wheelAngularVelocity = 0.0f; // Current wheel angular velocity

    public float WheelAngularVelocity => wheelAngularVelocity;

    public bool inTestMode { get; set; } = false;

    public void SetInitialConditions(float initialEngineSpeed, float initialWheelSpeed, bool engageClutch)
    {
        // For tests
        engineAngularVelocity = initialEngineSpeed;
        wheelAngularVelocity = initialWheelSpeed;
        clutchEngagement = engageClutch ? 1.0f : 0.0f;
    }

    private void Update()
    {
        // Control for clutch engagement (for testing purposes)
        if (Input.GetKey(KeyCode.E)) // Press 'E' to engage the clutch
        {
            clutchEngagement = Mathf.Clamp(clutchEngagement + 1f * Time.deltaTime, 0.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.Q)) // Press 'Q' to disengage the clutch
        {
            clutchEngagement = Mathf.Clamp(clutchEngagement - 1f * Time.deltaTime, 0.0f, 1.0f);
        }

        // Control for throttle input (for testing purposes)
        if (Input.GetKey(KeyCode.W)) // Press 'W' to increase throttle
        {
            throttle = Mathf.Clamp(throttle + 1f * Time.deltaTime, 0.0f, 1.0f);
        }
        if (Input.GetKey(KeyCode.S)) // Press 'S' to decrease throttle
        {
            throttle = Mathf.Clamp(throttle - 1f * Time.deltaTime, 0.0f, 1.0f);
        }
    }

    public void FixedUpdate()
    {
        if (inTestMode == false)
        {
            OnFixedUpdate();
        }
    }

    public void OnFixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        // Calculate engine friction torque
        float maxEngineFrictionTorque = Mathf.Abs(engineAngularVelocity * engineInertia / deltaTime);
        float engineFrictionTorque = Mathf.Clamp(engineFriction * Mathf.Sign(engineAngularVelocity), -maxEngineFrictionTorque, maxEngineFrictionTorque);

        // Calculate produced torque based on throttle
        float producedTorque = engineTorqueRating * throttle; // Torque produced by engine based on throttle

        // Net torque on the engine
        float engineNetTorque = producedTorque - engineFrictionTorque; // Friction opposes motion
        engineAngularVelocity += (engineNetTorque / engineInertia) * deltaTime;

        // Calculate clutch torque
        float speedDifference = engineAngularVelocity - wheelAngularVelocity;
        float clutchTorque;

        // Determine whether the clutch is slipping or fully engaged
        if (Mathf.Abs(speedDifference) < clutchSlipThreshold)
        {
            // Clutch is engaged (static)
            clutchTorque = Mathf.Clamp(speedDifference * clutchCoefficientStatic * clutchEngagement, -clutchMaxTorque, clutchMaxTorque);
        }
        else
        {
            // Clutch is slipping (dynamic)
            clutchTorque = Mathf.Clamp(speedDifference * clutchCoefficientDynamic * clutchEngagement, -clutchMaxTorque, clutchMaxTorque);
        }

        // Calculate wheel friction torque
        float maxWheelFrictionTorque = Mathf.Abs(wheelAngularVelocity * wheelInertia / deltaTime);
        float wheelFrictionTorque = Mathf.Clamp(wheelFriction * Mathf.Sign(wheelAngularVelocity), -maxWheelFrictionTorque, maxWheelFrictionTorque);

        // Net torque on the wheel
        float wheelNetTorque = clutchTorque - wheelFrictionTorque;
        wheelAngularVelocity += (wheelNetTorque / wheelInertia) * deltaTime;
    }
}

/*
using UnityEditor;
using UnityEngine;

public class Driveline : MonoBehaviour
{
    // suspected values from real life
    // sedan
    // flywheel inertia approx: 1.2 kg*m²
    // motor inertia approx: 0.4 kg*m²
    // total inertia: 1.6 kg*m²

    // sedan
    // flywheel inertia approx: 1.5 kg*m²
    // motor inertia approx: 0.5 kg*m²
    // total inertia: 2.0 kg*m²

    // muscle car
    // flywheel inertia approx: 1.8 kg*m²
    // crankshaft inertia approx: 0.6 kg*m²
    // total inertia: 2.4 kg*m²

    // electric/hybrid
    // motor inertia: 0.7 kg*m²
    // no flywheel: /
    // total inertia: 0.7 kg*m²

    // cabrio
    // flywheel inertia approx: 1.0 kg*m²
    // crankshaft inertia approx: 0.3 kg*m²
    // total inertia: 1.3 kg*m²

    [Header("Driveline Properties")]
    [SerializeField] private float generatedMotorTorque = 0.0f;
    [SerializeField] private float motorInertia = 0.05f;
    [SerializeField] private float flywheelInertia = 0.1f;
    [SerializeField] private float gearRatio = 2.0f;

    [SerializeField] private float flywheelFriction = 0.05f;
    [SerializeField] private float flywheelMinimalFrictionTorque = 0.01f;

    // motor
    private float motorAngularVelocity = 0.0f;

    // gears
    private float inputGearAngularVelocity = 0.0f;
    private float outputGearAngularVelocity = 0.0f;

    // flywheel
    private float flywheelAngularVelocity = 0.0f;

    [SerializeField] private float motorMomentum = 0.0f;
    [SerializeField] private float drivingGearMomentum = 0.0f;
    [SerializeField] private float drivenGearMomentum = 0.0f;
    [SerializeField] private float flywheelMomentum = 0.0f;

    [SerializeField] private float motorPower = 0.0f;
    [SerializeField] private float drivingGearPower = 0.0f;
    [SerializeField] private float drivenGearPower = 0.0f;
    [SerializeField] private float flywheelPower = 0.0f;

    private void FixedUpdate()
    {
        float dt = Time.deltaTime;

        float motorTotalInertia = motorInertia + (flywheelInertia / (gearRatio * gearRatio));
        float flywheelTotalInertia = flywheelInertia + (motorInertia * (gearRatio * gearRatio));

        // === forward propagation: motor -> gears -> flywheel ===

        float motorTorque = generatedMotorTorque;
        float motorAngularAcceleration = motorTorque / motorTotalInertia;
        motorAngularVelocity = motorAngularVelocity + motorAngularAcceleration * dt;

        float inputGearTorque = generatedMotorTorque;
        float inputGearAngularAcceleration = inputGearTorque / motorTotalInertia;
        inputGearAngularVelocity = inputGearAngularVelocity + inputGearAngularAcceleration * dt;

        float outputGearTorque = inputGearTorque * gearRatio;
        float outputGearAngularAcceleration = outputGearTorque / flywheelTotalInertia;
        outputGearAngularVelocity = outputGearAngularVelocity + outputGearAngularAcceleration * dt;

        float flywheelTorque = outputGearTorque;
        float flywheelAngularAcceleration = flywheelTorque / flywheelTotalInertia;
        flywheelAngularVelocity = flywheelAngularVelocity + flywheelAngularAcceleration * dt;

        // === backward propagation: flywheel -> gears -> motor

        // flywheel friction scales with angular velocity and has a minimal friction torque
        float flywheelFrictionTorque = flywheelFriction * -flywheelAngularVelocity;
        flywheelFrictionTorque = (Mathf.Abs(flywheelFrictionTorque) > flywheelMinimalFrictionTorque) 
            ? (flywheelFrictionTorque) // regular friction
            : (flywheelMinimalFrictionTorque * Mathf.Sign(flywheelFrictionTorque)); // minimum friction value
        float flywheelFrictionTorqueLimit = -flywheelAngularVelocity * flywheelTotalInertia / dt;
        flywheelFrictionTorque = (flywheelFrictionTorque == 0.0f) ? (0.0f) : (flywheelFrictionTorque > 0) ? ((flywheelFrictionTorque >= flywheelFrictionTorqueLimit) ? (flywheelFrictionTorqueLimit) : (flywheelFrictionTorque)) : ((flywheelFrictionTorque <= flywheelFrictionTorqueLimit) ? (flywheelFrictionTorqueLimit) : (flywheelFrictionTorque));

        flywheelTorque = flywheelFrictionTorque;
        flywheelAngularAcceleration = flywheelTorque / flywheelTotalInertia;
        flywheelAngularVelocity = flywheelAngularVelocity + flywheelAngularAcceleration * dt;

        outputGearTorque = flywheelTorque;
        outputGearAngularAcceleration = outputGearTorque / flywheelTotalInertia;
        outputGearAngularVelocity = outputGearAngularVelocity + outputGearAngularAcceleration * dt;

        inputGearTorque = outputGearTorque / gearRatio;
        inputGearAngularAcceleration = inputGearTorque / motorTotalInertia;
        inputGearAngularVelocity = inputGearAngularVelocity + inputGearAngularAcceleration * dt;

        motorTorque = inputGearTorque;
        motorAngularAcceleration = motorTorque / motorTotalInertia;
        motorAngularVelocity = motorAngularVelocity + motorAngularAcceleration * dt;

        // === update momentum values ===

        motorMomentum = motorTotalInertia * motorAngularVelocity;
        drivingGearMomentum = motorTotalInertia * inputGearAngularVelocity;
        drivenGearMomentum = flywheelTotalInertia * outputGearAngularVelocity;
        flywheelMomentum = flywheelTotalInertia * flywheelAngularVelocity;

        // === update power values

        motorPower = motorTorque * motorAngularVelocity;
        drivingGearPower = inputGearTorque * inputGearAngularVelocity;
        drivenGearPower = outputGearTorque * outputGearAngularVelocity;
        flywheelPower = flywheelTorque * flywheelAngularVelocity;
    }
}
*/