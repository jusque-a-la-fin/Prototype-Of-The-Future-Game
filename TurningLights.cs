using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script implements working of the turning lights of the car
public class TurningLights : MonoBehaviour
{
    // Buttons(pointers) activating turning lights: backend 
    public SC_ClickTracker[] turnButtons = new SC_ClickTracker[4];

    // Buttons(pointers) activating turning lights: frontend 
    public GameObject[] turningButtons = new GameObject[4];

    // Materials of the turning lights at the rear, the front, on the sides of the car.
    // Being attached to the turning lights meshes, these materials implement blinking
    public Material[] turnLights = new Material[2];

    // Flag variables that allows to call a coroutine only one time after pressing
    // a turning light button. Without using these variable-flags a few coroutines,
    // disturbing each other, will work at the same time
    bool switchOnRightTurnLight = false;
    bool switchOnLeftTurnLight = false;

    // Flag variables that indicate whether turning lights are activated or not
    bool leftTurningLightIsEnabled = false;
    bool rightTurningLightIsEnabled = false;

    // A coroutine animating the left turning light
    Coroutine lastRoutine1;

    // A coroutine animating the right turning light
    Coroutine lastRoutine2;

    // Reflections of turning lights that fall onto the ground
    public Light[] turningLightSpots;

    // Provide the access to the information about the behaviour of the wheels
    public Driving_VAZ driving_vaz;

    // Defines if any turn has been performed or not
    private bool detectorOfTurns = false;

    // Start is called before the first frame update
    private void Start()
    {
        // Switching on the button-pointers activating the turning lights: frontend
        turningButtons[0].SetActive(true);
        turningButtons[2].SetActive(true);

        // Switching off the button-pointers disabling the turning lights: frontend
        turningButtons[1].SetActive(false);
        turningButtons[3].SetActive(false);
    }

    // FixedUpdate is called 40 times per second
    private void FixedUpdate()
    {
        // If the button-pointer activating the left turning lights is pressed
        if (SC_MobileControls.instance.GetMobileButton("TurnLeft"))
        {
            // If the button-pointer disabling the right turning lights is ready to be pressed
            if (turningButtons[3].activeSelf == true)
            {
                // Switch off the right turning lights that are working
                // simultaneosly with the left turning lights 
                SwitchOffTheRightTurningLight();
            }

            // Change the value of the flag variable to call the coroutine
            // which implement working of the left turning buttons
            switchOnLeftTurnLight = true;

            // Change the value of the flag variable that indicate
            // the left turning lights are activated
            leftTurningLightIsEnabled = true;

            // Make this button(activating left turning lights):backend unpressed
            // because only unpressed buttons are able to be pressed again
            turnButtons[0].ChangeHoldingStatus();

            // Switching off the button-pointer activating the left turning lights: frontend
            turningButtons[0].SetActive(false);

            // Switching on the button-pointer disabling the left turning lights: frontend
            turningButtons[1].SetActive(true);
        }

        // If the button-pointer disabling the left turning lights is pressed 
        if (SC_MobileControls.instance.GetMobileButton("TurnLeftGreen"))
        {
            // Switch off the left turning lights
            SwitchOffTheLeftTurningLight();
        }

        // If the button-pointer activating the right turning lights is pressed
        if (SC_MobileControls.instance.GetMobileButton("TurnRight"))
        {
            // If the button-pointer disabling the left turning lights is ready to be pressed
            if (turningButtons[1].activeSelf == true)
            {
                // Switch off the left turning lights that are working
                // simultaneosly with the right turning lights 
                SwitchOffTheLeftTurningLight();
            }

            // Change the value of the flag variable to call the coroutine
            // which implement working of the right turning buttons
            switchOnRightTurnLight = true;

            // Change the value of the flag variable that indicate
            // the right turning lights are activated
            rightTurningLightIsEnabled = true;

            // Make this button(activating the right turning lights):backend unpressed
            // because only unpressed buttons are able to be pressed again
            turnButtons[2].ChangeHoldingStatus();

            // Switching off the button-pointer activating the right turning lights: frontend
            turningButtons[2].SetActive(false);

            // Switching on the button-pointer disabling the left turning lights: frontend
            turningButtons[3].SetActive(true);
        }

        // If the button-pointer disabling the right turning lights is pressed 
        if (SC_MobileControls.instance.GetMobileButton("TurnRightGreen"))
        {
            // Switch off the right turning lights
            SwitchOffTheRightTurningLight();
        }

       
        if (switchOnLeftTurnLight == true)
        {
            // Call the coroutine which implement working of the left turning buttons 
            lastRoutine1 = StartCoroutine(EnableLeftTurningLight());
        }

      
        if (switchOnRightTurnLight == true)
        {
            // Call the coroutine which implement working of the right turning buttons
            lastRoutine2 = StartCoroutine(EnableRightTurningLight());
        }

        // If the car is turning
        if (driving_vaz.carAxle[1].rightWheel.steerAngle != 0f)
        {
            detectorOfTurns = true;
        }
       
        // If the car is going straight
        if(driving_vaz.carAxle[1].rightWheel.steerAngle == 0f && detectorOfTurns == true)
        {
            // If the left turning lights are working
            if (leftTurningLightIsEnabled == true)
            {
                // Switch off the left turning lights
                SwitchOffTheLeftTurningLight();
            }

            // If the right turning lights are working
            if (rightTurningLightIsEnabled == true)
            {
                // Switch off the right turning lights
                SwitchOffTheRightTurningLight();
            }

            detectorOfTurns = false;
        }
    }

    // The coroutine which implement working of the left turning buttons
    private IEnumerator EnableLeftTurningLight()
    {
        switchOnLeftTurnLight = false;

        // Make the material of the left turning lights capable of emitting light
        turnLights[0].EnableKeyword("_EMISSION");

        // Enable reflections of the left turning lights that fall onto the ground
        for (int i = 0; i < 3; ++i)
        {
            turningLightSpots[i].intensity = 10f;
        }
        
        // Wait 0.3 seconds
        yield return new WaitForSeconds(0.3f);

        // Make the material of the left turning lights incapable of emitting light
        turnLights[0].DisableKeyword("_EMISSION");

        // Disable reflections of the left turning lights that fall onto the ground
        for (int i = 0; i < 3; ++i)
        {
            turningLightSpots[i].intensity = 0f;
        }

        // wait 0.3 seconds
        yield return new WaitForSeconds(0.3f);
  
        switchOnLeftTurnLight = true;
    }

    // The coroutine which implement working of the right turning buttons
    private IEnumerator EnableRightTurningLight()
    {

        switchOnRightTurnLight = false;

        // Make the material of the right turning lights capable of emitting light
        turnLights[1].EnableKeyword("_EMISSION");

        // Enable reflections of the right turning lights that fall onto the ground
        for (int i = 3; i < 6; ++i)
        {
            turningLightSpots[i].intensity = 10f;
        }

        // Waits 0.3 seconds
        yield return new WaitForSeconds(0.3f);

        // Make the material of the right turning lights incapable of emitting light
        turnLights[1].DisableKeyword("_EMISSION");

        // Disable reflections of the right turning lights that fall onto the ground
        for (int i = 3; i < 6; ++i)
        {
            turningLightSpots[i].intensity = 0f;
        }

        // Waits 0.3 seconds
        yield return new WaitForSeconds(0.3f);
        
        switchOnRightTurnLight = true;
    }

    // Disable the left turning lights
    private void SwitchOffTheLeftTurningLight()
    {
        leftTurningLightIsEnabled = false;

        // Make this button(disabling the left turning lights):backend unpressed
        // because only unpressed buttons are able to be pressed again
        turnButtons[1].ChangeHoldingStatus();

        // Stop the coroutine which implement working of the left turning buttons
        StopCoroutine(lastRoutine1);

        // Switching off the button-pointer disabling the left turning lights: frontend
        turningButtons[1].SetActive(false);

        // Switching on the button-pointer activating the left turning lights: frontend
        turningButtons[0].SetActive(true);

        // Make the material of the left turning lights incapable of emitting light
        turnLights[0].DisableKeyword("_EMISSION");
    }

    // Disable the right turning lights
    private void SwitchOffTheRightTurningLight()
    {
        rightTurningLightIsEnabled = false;

        // Make this button(disabling the right turning lights):backend unpressed
        // because only unpressed buttons are able to be pressed again
        turnButtons[3].ChangeHoldingStatus();

        // Stop the coroutine which implement working of the right turning buttons
        StopCoroutine(lastRoutine2);

        // Switching off the button-pointer disabling the right turning lights: frontend
        turningButtons[3].SetActive(false);

        // Switching on the button-pointer activating the right turning lights: frontend
        turningButtons[2].SetActive(true);

        // Make the material of the right turning lights incapable of emitting light
        turnLights[1].DisableKeyword("_EMISSION");
    }
}
