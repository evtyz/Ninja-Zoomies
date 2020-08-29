using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverlayController : MonoBehaviour
{
    public Sprite character;
    public Sprite doubleJumpIcon;
    public Sprite extraSpeedIcon;
    public Sprite lowGravityIcon;
    private Sprite[] sprites;

    private Image characterRenderer;
    private Image powerupRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        characterRenderer = gameObject.transform.GetChild(0).GetComponent<Image>();
        characterRenderer.sprite = character;

        powerupRenderer = gameObject.transform.GetChild(1).GetComponent<Image>();
        powerupRenderer.sprite = null;
        powerupRenderer.enabled = false;

        sprites = new Sprite[] {null, lowGravityIcon, extraSpeedIcon, doubleJumpIcon};
    }

    public void setPowerup(int index)
    {
        if (index == 0)
        {
            powerupRenderer.enabled = false;
            return;
        }
        powerupRenderer.enabled = true;
        powerupRenderer.sprite = sprites[index];
    }

    public void kill()
    {
        gameObject.SetActive(false);
    }
}
