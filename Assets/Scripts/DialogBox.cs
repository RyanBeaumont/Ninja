using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.Cinemachine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[Serializable] public enum CameraAngle{standard, none, closeup, lowAngle, highAngle, behind, zoom, tilt, dodgeLeft, dodgeRight, jump, duck, wideBehind, ground, super, lockOn, counter, knifeView};
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
    [SerializeField] RectTransform nameBox;
    [SerializeField] RectTransform yesButton;
    [SerializeField] RectTransform noButton;
    int sfxTimer = 0;
    public Image image;
    bool flipCam = false;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        textComponent.text = "";
        canvas.enabled = false;
        anim = GetComponentInChildren<Animator>();
        // Use the GameManager camera rig instead of creating a separate one here.
        // DialogBox.SetPose will call GameManager.Instance.GetCamera when needed.
        cameraRig = null;
        cameraAnimator = null;
        cutsceneCamera = null;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    void Update()
    {
        if ((Input.GetButtonDown("Interact") || Input.GetButtonDown("Jump")) && canvas.enabled)
        {
            if(dialog.Count == 0) return;
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
        player.GetComponentInChildren<Animator>().Play("Idle");
        textComponent.text = "";
        flipCam = false;
        StartCoroutine(TypeLine());
        canvas.enabled = true;
        cutsceneCamera.Priority = 10;
        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var partyLeader = YourParty.instance.partyMembers[0];
        print($"Party Leader: {partyLeader}");
        if(partyLeader == "Spartan Jack") image.color = new Color32(250, 222, 159,255);
        if(partyLeader == "Thad") image.color = new Color32(245, 144, 129,255);
        if(partyLeader.Equals("Storm")) image.color = new Color32(251, 129, 255,255);
        if(partyLeader == "Stretch") image.color = new Color32(255, 252, 165,255);
        if(partyLeader == "Torch") image.color = new Color32(255, 177, 96,255);

        
    }

    public void ShowChoiceButtons(string option1 = "Yes", string option2 = "No")
    {
        yesButton.gameObject.SetActive(true);
        yesButton.GetComponent<Button>().Select();
        yesButton.GetComponent<Button>().interactable = true;
        noButton.GetComponent<Button>().interactable = true;
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
        yesButton.GetComponent<Button>().interactable = false;
        noButton.GetComponent<Button>().interactable = false;
    }

    public void NoClicked()
    {
        choice = "No";
        yesButton.GetComponent<Button>().interactable = false;
        noButton.GetComponent<Button>().interactable = false;
        canAdvance = true;
        AdvanceDialog();
    }


    void SetPose(Transform target, string pose, CameraAngle cameraAngle, string face)
    {
        if(target == null) target = player;
        cameraRig = GameManager.Instance.GetCamera(out cameraAnimator,out cutsceneCamera);
        Animator anim = target.GetComponentInChildren<Animator>();
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
        if(target.GetComponentInChildren<DefaultPose>() != null)
        {
            cameraRig.transform.localPosition = new Vector3(0f, target.GetComponentInChildren<DefaultPose>().heightOffset, 0f);
        }
        
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
        else if(cameraAngle == CameraAngle.ground) cameraAnimator.Play("Camera_Ground");
        else if(cameraAngle == CameraAngle.super) cameraAnimator.Play("Camera_Super");
        else if(cameraAngle == CameraAngle.lockOn) cameraAnimator.Play("Camera_LockOn");
        else if(cameraAngle == CameraAngle.counter) cameraAnimator.Play("Camera_Counter");
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
            anim.SetBool("DialogActive", false);
            Invoke("DisableCanvas", 0.3f);
            Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        }
    }

    void DisableCanvas()
    {
        GameManager.Instance.DestroyCamera();
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        canvas.enabled = false;
        player.GetComponentInChildren<Animator>().Play("Running");
        player.GetComponentInChildren<FaceChanger>().ChangeFace("Happy");
        OnDialogFinished?.Invoke();
        
    }

    private IEnumerator TypeLine()
    {
        
        if(dialog[0].name != "") nameBox.gameObject.SetActive(true);
        else nameBox.gameObject.SetActive(false);
        nameText.text = dialog[0].name;
        SetPose(dialog[0].character,dialog[0].pose, dialog[0].cameraAngle, dialog[0].face);
        foreach (char c in dialog[0].text.ToCharArray())
        {
            textComponent.text += c;
            sfxTimer --;
            if(sfxTimer <= 0){AudioManager.Instance.PlaySoundEffect("click",UnityEngine.Random.Range(0.8f,1.2f));sfxTimer = 3;}
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
