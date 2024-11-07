using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    public int id; 
    private Animator animator;  
    private GameManager gameManager;  
    private BoxCollider cubeCollider; 
    public GameObject hiddenImg; 
    private Texture2D hiddenImageTexture; 

    public bool isShowing; 

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        cubeCollider = GetComponentInChildren<BoxCollider>();

        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("No GameManager found! Please ensure there is a GameManager with the correct tag.");
        }

        Transform cubeTransform = transform.Find("Cube");
        if (cubeTransform == null)
        {
            Debug.LogError("Cube not found as a child of Token. Please ensure there is a Cube child.");
            return;
        }

        hiddenImg = cubeTransform.Find("HiddenImg")?.gameObject;
        if (hiddenImg == null)
        {
            Debug.LogError("HiddenImg not found as a child of Cube. Please ensure it exists.");
            return;
        }

        AssignHiddenImage();
    }

    private void AssignHiddenImage()
    {
        hiddenImageTexture = hiddenImg.GetComponent<Renderer>().material.mainTexture as Texture2D;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == cubeCollider)
                {
                    OnTokenClicked();
                }
            }
        }
    }

    private void OnTokenClicked()
    {
 
        ShowToken();

        gameManager.OnTokenClicked(this); 
    }

    public void ShowToken()
    {
        animator.SetTrigger("ShowToken");
        isShowing = true;
        animator.SetBool("Showing", true);
    }

    public void HideToken()
    {
        animator.SetTrigger("HideToken");
        isShowing = false;
        animator.SetBool("Showing", false);
    }

    public bool CompareToken(Token otherToken)
    {
        return hiddenImageTexture == otherToken.hiddenImageTexture;
    }
}
