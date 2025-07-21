using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] protected float maxInteractDistance;
    [SerializeField] protected Transform player;
    private SpriteRenderer sr;
    private Color startColour;
    private bool changed = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if(!player) player = transform;
    }

    void OnMouseOver()
    {
        if(!InputManager.BuildMode && Vector3.Distance(player.position, transform.position) <= maxInteractDistance && !changed)
        {
            startColour = sr.material.color;
            sr.material.color = Color.green;
            changed = true;
        }
        if(changed && (Vector3.Distance(player.position, transform.position) > maxInteractDistance || InputManager.BuildMode))
        {
            sr.material.color = startColour;
            changed = false;
        }

        if(!InputManager.BuildMode && Vector3.Distance(player.position, transform.position) <= maxInteractDistance)
        {
            if(InputManager.InteractHolding)
            {
                InteractHold();
            }
            else if(InputManager.Interacting)
            {
                InteractPress();
            }
        }
    }

    void OnMouseExit()
    {
        if(changed)
        {
            sr.material.color = startColour;
            changed = false;
        }
    }

    public abstract void InteractPress();

    public abstract void InteractHold();
}
