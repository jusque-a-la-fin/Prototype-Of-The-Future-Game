using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script implements working of the front lights of the car
public class FrontLights : MonoBehaviour
{
    // Reflections of front lights that fall onto the ground
    public Light[] frontLights;

    // A button to activate front lights: frontend
    public GameObject frontLightsOn;

    // A button to turn off front frontLights: frontend
    public GameObject frontLightsOff;

    // Buttons activating front lights: backend
    public SC_ClickTracker[] frontLightsButtons = new SC_ClickTracker[2];

    // Materials that make front lights look turned on 
    public Material[] frontLightsMaterials = new Material[3];

    // Start is called before the first frame update
    private void Start()
    {
        // Switch off the front lights.
        // Initially they are turned on in the scene of the project
        foreach (Light light in frontLights)
        {
            light.intensity = 0;
        }

        // Switching on the button activating front lights: frontend
        frontLightsOn.SetActive(true);

        // Switching off the button disabling front lights: frontend
        frontLightsOff.SetActive(false);
    }

    // FixedUpdate is called 40 times per second
    private void FixedUpdate()
    {
        // If the button activating front lights is pressed
        if (SC_MobileControls.instance.GetMobileButton("frontLightsOn"))
        {
            // Switching on the button disabling front lights: frontend
            frontLightsOff.SetActive(true);

            // Switching off the button activating front lights: frontend
            frontLightsOn.SetActive(false);

            // Make the frontLights look turned on
            frontLightsMaterials[0].color = new Color(1.0f, 0.94f, 0.0f, 1.0f);

            frontLightsMaterials[1].EnableKeyword("_EMISSION");
            frontLightsMaterials[1].SetColor("_EmissionColor", new Color(1f, 1f, 0f, 1f));

            frontLightsMaterials[2].EnableKeyword("_EMISSION");
            frontLightsMaterials[2].SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f));

            // Make this button(activating front lights):backend unpressed
            // because only unpressed buttons are able to be pressed again
            frontLightsButtons[1].ChangeHoldingStatus();

            // Switch on the front lights.
            foreach (Light light in frontLights)
            {
                light.intensity = 8f;
            }
        }

        // If the button disabling front lights is pressed
        if (SC_MobileControls.instance.GetMobileButton("frontLightsOff"))
        {
            // Switching on the button activating front lights: frontend
            frontLightsOn.SetActive(true);

            // Switching off the button disabling front lights: frontend
            frontLightsOff.SetActive(false);

            // Make the front lights look turned off
            frontLightsMaterials[0].color = Color.white;

            frontLightsMaterials[1].DisableKeyword("_EMISSION");
            frontLightsMaterials[1].color = new Color(1f, 1f, 1f, 1f);

            frontLightsMaterials[2].DisableKeyword("_EMISSION");
            frontLightsMaterials[2].color = new Color(1f, 1f, 1f, 1f);

            // Make this button(disabling front lights):backend unpressed
            // because only unpressed buttons are able to be pressed again
            frontLightsButtons[0].ChangeHoldingStatus();

            // Switch off the front lights.
            foreach (Light light in frontLights)
            {
                light.intensity = 0;
            }
        }
    }
}
