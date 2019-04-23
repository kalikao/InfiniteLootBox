using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
/// <summary>
/// Chest behaviour.
/// </summary>
/// <remarks>
/// Flying into the scene. Floating in the air. 
/// Lid opening, falling, scaling down and particles after click.
/// </remarks>
public class ChestBehaviour : MonoBehaviour
{
    Transform Lid;
    Quaternion LidClosedQuat;
    Quaternion LidOpenedQuat;
    bool isOpening = false;
    float LidLerpStage = 0;
    Vector3 endingScale;
    float fallingDistance = 0;
    Vector3 initialPos;
    Vector3 targetPos;
    float chestMoveStage = 0; //[0,1]
    float openSpeed;
    private ParticleSystem mParticleSys;
    void Start()
    {
        openSpeed = GameController.Settings.ChestOpeningSpeed;

        mParticleSys = GetComponent<ParticleSystem>();
        var mpsm = mParticleSys.main;
        mpsm.maxParticles = GameController.Settings.OpenParticlesMax;
        var mpse = mParticleSys.emission;
        mpse.rateOverTime = GameController.Settings.OpenParticlesRate;
        
        endingScale = transform.localScale;
        Lid = transform.Find("Armature").Find("gBottom").Find("gTop");
        LidClosedQuat = Lid.transform.localRotation;
        var eu = LidClosedQuat.eulerAngles;
        LidOpenedQuat = Quaternion.Euler(eu.x, eu.y, eu.z - 140);

        float camNearClip = Camera.main.nearClipPlane;
        initialPos = new Vector3(-0.25f, 0.13f, camNearClip + 20f); //left-bottom quadrant
        targetPos = new Vector3(0.25f, 0.25f, camNearClip + 10f); //left-bottom quadrant
    }
    bool spawnedNew = false;
    void Update()
    {
        var cam = Camera.main;
        if (chestMoveStage < 1)
            chestMoveStage += Time.deltaTime * 1.1f;
        else
            chestMoveStage = 1;
        var chestViewportPos = Vector3.LerpUnclamped(initialPos, targetPos, EasingFunction.EaseOutElastic(0, 1, chestMoveStage));
        Vector3 newPosition = cam.ViewportToWorldPoint(chestViewportPos);
        newPosition.y += 0.3f * Mathf.Sin(0.57f * Time.time) - fallingDistance;
        newPosition.x += 0.13f * Mathf.Sin(0.21f * Time.time);
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(3f * Mathf.Sin(1.91f * Time.time),
                                                -13 + 15f * Mathf.Sin(0.9f * Time.time),
                                                     3f * Mathf.Sin(2.21f * Time.time));

        if (wasClicked)
        {
            if (!spawnedNew)
            {
                spawnedNew = true;
                GameController.instance.spawnNewChest(GameController.Settings.ChestSpawnDelay);
            }
            endingScale *= (1f - 0.05f * openSpeed);
            transform.localScale = endingScale;
            fallingDistance += ( 0.5f * openSpeed) * Time.deltaTime;

        }
        if (isOpening)
        {
            LidLerpStage += Time.deltaTime * 2f * openSpeed;
            Lid.transform.localRotation = Quaternion.Lerp(LidClosedQuat, LidOpenedQuat, LidLerpStage);
            if (LidLerpStage > 1) isOpening = false;
        }
        else
        {//isClosing
            LidLerpStage -= Time.deltaTime * 0.3f * openSpeed;
            if (LidLerpStage > 0)
                Lid.transform.localRotation = Quaternion.Lerp(LidClosedQuat, LidOpenedQuat, LidLerpStage);
            else
                LidLerpStage = 0;
        }
    }
    private bool wasClicked = false;
    void OnMouseDown()
    {
        // GameController.instance.audioSrc.pitch = Random.Range(0.92f, 1.15f);
        // GameController.instance.audioSrc0.pitch = Random.Range(0.93f, 1f);
        GameController.instance.audioSrc0.pitch = Random.Range(0.98f, 1.01f)*GameController.Settings.AudioSpeed;
        GameController.instance.audioSrc0.Play();
        GameController.instance.audioSrc1.Play();
        if (!wasClicked)
        {
            if (GameController.Settings.OpenParticlesMax > 0) mParticleSys.Play();
            var go = GameController.instance.getNextLoot();
            go.transform.Translate(this.transform.position, Space.World);
            go.transform.Rotate(this.transform.rotation.eulerAngles, Space.World);
            Destroy(gameObject,GameController.Settings.ChestDestroyDelay);
        }
        wasClicked = true;
        isOpening = true;
    }

}
