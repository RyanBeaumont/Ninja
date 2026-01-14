using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.Cinemachine;
using UnityEngine.U2D.Animation;

[Serializable] public enum CameraAngle{standard, none, closeup, lowAngle, highAngle, behind, zoom, tilt, dodgeLeft, dodgeRight, jump, duck, wideBehind};
[Serializable] public class Dialog
{
    [TextArea] public string text;
    public string name = "Spartan Jack";
    public Transform character;
    public string pose;
    public CameraAngle cameraAngle;
    public string face;
}
public class DialogBox : MonoBehaviour
{
    public event Action OnDialogFinished;
    public TextMeshProUGUI textComponent;
    public List<Dialog> dialog = new List<Dialog>();
    GameObject cameraRig;
    CinemachineCamera cutsceneCamera;
    Animator cameraAnimator;
    Canvas canvas;
    public float textSpeed = 0.05f;
    Animator anim;
    Transform model;
    Transform player;
    public string choice = "";
    bool canAdvance = true;
    [SerializeField] TMP_Text nameText;
    [SerializeField] RectTransform yesButton;
    [SerializeField] RectTransform noButton;
    bool flipCam = false;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        textComponent.text = "";
        canvas.enabled = false;
        anim = GetComponent<Animator>();
        cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
        cameraAnimator = cameraRig.GetComponent<Animator>();
        cutsceneCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) && canvas.enabled)
        {
            if (textComponent.text == dialog[0].text)
                AdvanceDialog();
            else
            {
                StopAllCoroutines();
                textComponent.text = dialog[0].text;
            }
        }
    }

    public void BasicDialog(string[] messages)
    {
        dialog.Clear();
        foreach (string txt in messages)
        {
            dialog.Add(new Dialog { text = txt });
            StartDialog(dialog);
        }
    }

    public void StartDialog(List<Dialog> newDialog)
    {
        if (canvas.enabled) return;
        dialog = new List<Dialog>(newDialog);
        anim.SetBool("DialogActive", true);
        GameManager.Instance.SetGameplayState(GameplayState.Dialog);
        player.GetComponent<Animator>().Play("Idle");
        textComponent.text = "";
        flipCam = false;
        StartCoroutine(TypeLine());
        canvas.enabled = true;
        cutsceneCamera.Priority = 10;
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }

    public void ShowChoiceButtons(string option1 = "Yes", string option2 = "No")
    {
        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
        yesButton.GetComponentInChildren<TMP_Text>().text = option1;
        noButton.GetComponentInChildren<TMP_Text>().text = option2;
        canAdvance = false;
    }

    public void YesClicked()
    {
        choice = "Yes";
        canAdvance = true;
        AdvanceDialog();
    }

    public void NoClicked()
    {
        choice = "No";
        canAdvance = true;
        AdvanceDialog();
    }

    void SetPose(Transform target, string pose, CameraAngle cameraAngle, string face)
    {
        if(target == null) target = player;
        if(cameraRig == null)
        {
            cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
            cameraAnimator = cameraRig.GetComponent<Animator>();
            cutsceneCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
        }
        Animator anim = target.GetComponent<Animator>();
        if(anim != null && pose != ""){
            if(!anim.GetCurrentAnimatorStateInfo(0).IsName(pose)){
                anim.CrossFade(pose, 0.05f);
                var pulse = target.GetComponentInChildren<PulseToTheBeat>();
                if(pulse != null) pulse.Pulse();
            }
           
        }
        if(face != "")
        {
            FaceChanger f = target.GetComponentInChildren<FaceChanger>();
            if(f != null)
            {
                f.ChangeFace(face);
            }
        }
        cameraRig.transform.parent = target;
        cameraRig.transform.localRotation = Quaternion.identity;
        cameraRig.transform.localPosition = new Vector3(0f,0f,0f);
        if(target.tag == "Player")
        {
            cameraRig.transform.localPosition += new Vector3(0f,-0.4f,0f);
        }
        if(cameraAngle == CameraAngle.standard)
        {
            if(flipCam)cameraAnimator.Play("Camera_OTS_Left");
            else cameraAnimator.Play("Camera_OTS");
            flipCam = !flipCam;
        }
        else if(cameraAngle == CameraAngle.closeup) cameraAnimator.Play("Camera_Closeup");
        else if(cameraAngle == CameraAngle.behind) cameraAnimator.Play("Camera_Behind");
        else if(cameraAngle == CameraAngle.lowAngle) cameraAnimator.Play("Camera_LowAngle");
        else if(cameraAngle == CameraAngle.highAngle) cameraAnimator.Play("Camera_HighAngle");
        else if(cameraAngle == CameraAngle.zoom) cameraAnimator.Play("Camera_Zoom");
        else if(cameraAngle == CameraAngle.tilt) cameraAnimator.Play("Camera_Tilt");
    }
    

    void AdvanceDialog()
    {
        if(canAdvance == false) return;
        dialog.RemoveAt(0);
        textComponent.text = "";
        if (dialog.Count > 0)
        {
            canvas.enabled = true;
            StartCoroutine(TypeLine());
        }
        else
        {
            
            cutsceneCamera.Priority = 0;
            anim.SetBool("DialogActive", false);
            Invoke("DisableCanvas", 0.3f);
        }
    }

    void DisableCanvas()
    {
        canvas.enabled = false;
        player.GetComponentInChildren<Animator>().Play("Running");
        player.GetComponentInChildren<FaceChanger>().ChangeFace("Happy");
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        OnDialogFinished?.Invoke();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator TypeLine()
    {
        nameText.text = dialog[0].name;
        SetPose(dialog[0].character,dialog[0].pose, dialog[0].cameraAngle, dialog[0].face);
        foreach (char c in dialog[0].text.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
