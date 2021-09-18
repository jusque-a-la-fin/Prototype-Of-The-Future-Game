using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

// Implements working of the parking brake
public class ParkingBrake : MonoBehaviour
{
    // A button to activate the parking brake: frontend
    public GameObject parkingBrakeOn;

    // A button to disable the parking brake: frontend
    public GameObject parkingBrakeOff;

    // A button to activate/disable the parking brake: backend
    public SC_ClickTracker[] parkingBrakeButtons = new SC_ClickTracker[2];

    // Indicates whether the parking brake is working or not
    bool parkingBrakeEnable = false;
    
    // Braking force
    float parkingBrakeForce = 10000.0f;

    // Access to the wheels which have to be stopped
    public Driving_VAZ vehicle;

    // Start is called before the first frame update
    private void Start()
    {
        // Switch on the button to activate the parking brake: frontend
        parkingBrakeOn.SetActive(true);

        // Switch off the button to disable the parking brake: frontend
        parkingBrakeOff.SetActive(false);
    }

    // FixedUpdate is called 40 times per second
    private void FixedUpdate()
    {
        // If the button activating the parking brake is pressed
        if (SC_MobileControls.instance.GetMobileButton("ParkingBrakeOn"))
        {
            // Switching off the button activating the parking brake: frontend
            parkingBrakeOn.SetActive(false);

            // Switching on the button disabling the parking brake: frontend
            parkingBrakeOff.SetActive(true);

            // Indicates the parking brake is working now
            parkingBrakeEnable = true;

            // Make this button(activating the parking brake):backend unpressed
            // because only unpressed buttons are able to be pressed again
            parkingBrakeButtons[0].ChangeHoldingStatus();           
        }

        // If the button disabling the parking brake is pressed
        if (SC_MobileControls.instance.GetMobileButton("ParkingBrakeOff"))
        {
            // Switching on the button activating the parking brake: frontend
            parkingBrakeOn.SetActive(true);

            // Switching off the button disabling the parking brake: frontend
            parkingBrakeOff.SetActive(false);

            // Indicates the parking brake isn't working now
            parkingBrakeEnable = false;

            // Make this button(disabling the parking brake):backend unpressed
            // because only unpressed buttons are able to be pressed again
            parkingBrakeButtons[1].ChangeHoldingStatus();

            // Disable the parking brake
            ApplyParkingBrake(parkingBrakeEnable);
        }

        if(parkingBrakeEnable)
        {
            // Activate the parking brake
            ApplyParkingBrake(parkingBrakeEnable);
        }
    }

    // Activates/Disables the parking brake
    public void ApplyParkingBrake(bool enable)
    {
        
        if (enable == true)
        {
            // Lock the back right wheel(wheelcollider)
            vehicle.carAxle[0].rightWheel.brakeTorque = parkingBrakeForce;

            // Lock the back left wheel(wheelcollider)
            vehicle.carAxle[0].leftWheel.brakeTorque = parkingBrakeForce;
        }
        else
        {
            // Unlock the back right wheel(wheelcollider)
            vehicle.carAxle[0].rightWheel.brakeTorque = 0;

            // Unlock the back left wheel(wheelcollider)
            vehicle.carAxle[0].leftWheel.brakeTorque = 0;
        }
    }
}
