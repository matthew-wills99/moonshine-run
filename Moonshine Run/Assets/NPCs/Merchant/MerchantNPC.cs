using UnityEngine;

public class MerchantNPC : Interactable
{
    [SerializeField] private MerchantUI merchantUI;

    private bool isUIOpen = false;
    private bool isUIReady = false;

    private void Update()
    {
        if(!isUIOpen) return;
        if(!isUIReady)
        {
            isUIReady = true;
            return;
        }

        if(Vector3.Distance(player.position, transform.position) > maxInteractDistance || InputManager.Interacting)
            CloseUI();
    }

    public override void InteractPress()
    {
        if(isUIOpen) CloseUI();
        else OpenUI();
        
        Debug.Log("Interacted with the Merchant.");
    }

    private void OpenUI()
    {
        merchantUI.gameObject.SetActive(true);
        isUIOpen = true;
    }

    private void CloseUI()
    {
        merchantUI.gameObject.SetActive(false);
        isUIOpen = false;
        isUIReady = false;
    }

    public override void InteractHold()
    {
        Debug.Log("Holding interact on the Merchant.");
    }
}
