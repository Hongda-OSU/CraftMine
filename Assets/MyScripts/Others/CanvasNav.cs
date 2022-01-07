using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasNav : MonoBehaviour
{
    public List<Button> buttons;
    int buttonIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (buttons != null)
        {
            //If there are, select the first one
            buttons[0].GetComponent<Outline>().enabled = true;
            buttons[0].Select();
            buttonIndex = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Tab) && buttons.Count > 1)
		{
			//If there are, check if either shift key is being pressed
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			{
				//If shift is pressed, move up on the list - or, if at the top of the list, move to the bottom
				if (buttonIndex <= 0) 
				{
					buttonIndex = buttons.Count;
				}
				buttonIndex--;
				buttons[buttonIndex].GetComponent<Outline>().enabled = true;
                buttons[buttonIndex].Select();
            }
			else
			{
				//if shift is not pressed, move down on the list - or, if at the bottom, move to the top
				if (buttonIndex + 1 >= buttons.Count) // last button
                {
					buttonIndex = -1;
				}
				buttonIndex++; // change to first button
				buttons[buttonIndex].GetComponent<Outline>().enabled = true;
                buttons[buttonIndex].Select();
}

            if (buttonIndex == 0) // first button
            {
                buttons[buttons.Count - 1].GetComponent<Outline>().enabled = false; // disable last button
            }
            else
            {
                buttons[buttonIndex - 1].GetComponent<Outline>().enabled = false;
            }
        }
	}
}
