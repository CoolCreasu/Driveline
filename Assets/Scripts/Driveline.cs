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