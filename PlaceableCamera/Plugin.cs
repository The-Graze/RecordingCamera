using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RecordingCamera
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        GameObject CamMan;
        void Update()
        {
            if (CamMan == null)
            {
                CamMan = new GameObject("CameraMan");
                CamMan.AddComponent<CameraScript>();
                DontDestroyOnLoad(CamMan);
                Destroy(this);
            }
        }
        void OnDestroy()
        {
            if (CamMan == null)
            {
                CamMan = new GameObject("CameraMan");
                CamMan.AddComponent<CameraScript>();
                DontDestroyOnLoad(CamMan);
            }
        }
    }
    class CameraScript : MonoBehaviour
    {
        Camera cam;
        Camera MainCam;

        PlayerControllerB me;

        Renderer HemlRend;

        GameObject Arms;

       LODGroup lodg;

        bool ThirdP;

        bool Dropped = true;
            
        bool Behind;

        bool CopyRottation;

        bool hide;

        public float MoveSpeed = 1;

        public float lerpingSpeed = 2f; 

        Vector3 targetPosition;

        Vector3 Perspective()
        {
            if (Behind)
            {
                return new Vector3(0, 0, -2.5f);
            }
            else
            {
                return new Vector3(0, 0, 2.5f);
            }
        }
        Quaternion rotPerspective()
        {
            if (Behind)
            {
                return Quaternion.Euler(0, 0, 0);
            }
            else
            {
                return Quaternion.Euler(0, 180, 0);
            }
        }

        void Update()
        {
            try
            {
                if (cam == null)
                {
                    cam = new GameObject("ThridPersonCam").AddComponent<Camera>();
                    HemlRend = GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/ScavengerHelmet").GetComponent<Renderer>();
                }
                else
                {
                    cam.enabled = ThirdP;
                    HemlRend.forceRenderingOff = ThirdP;
                    if (Arms != null) { Arms.SetActive(!ThirdP); }
                    if (me == null)
                    {
                        foreach (PlayerControllerB playerControllerB in FindObjectsOfType<PlayerControllerB>())
                        {
                            if (playerControllerB.IsOwner && playerControllerB.isPlayerControlled)
                            {
                                me = playerControllerB;
                                MainCam = me.gameplayCamera;
                                cam.cullingMask = MainCam.cullingMask;
                                targetPosition = MainCam.transform.position;
                                Arms = me.cameraContainerTransform.parent.GetChild(1).gameObject;
                                lodg = me.transform.GetChild(0).GetComponent<LODGroup>();
                            }
                        }
                    }
                    else if (!me.inTerminalMenu || !me.isTypingChat)
                    {
                        if (me.isPlayerDead)
                        {
                            ThirdP = false;
                            hide = false;
                        }
                        if (Keyboard.current.endKey.wasPressedThisFrame)
                        {
                            ThirdP = !ThirdP;
                            if (ThirdP)
                            {
                                lodg.ForceLOD(1);
                            }
                            else
                            {
                                lodg.enabled = false;
                                lodg.enabled = true;
                            }
                        }
                        if (Keyboard.current.cKey.wasPressedThisFrame)
                        {
                            Dropped = !Dropped;
                            SwitchCamState();
                        }
                        if (Dropped)
                        {
                            #region movekeys 
                            if (Keyboard.current.uKey.wasPressedThisFrame) { targetPosition += cam.transform.forward * MoveSpeed; }
                            if (Keyboard.current.jKey.wasPressedThisFrame) { targetPosition -= cam.transform.forward * MoveSpeed; }
                            if (Keyboard.current.kKey.wasPressedThisFrame) { targetPosition += cam.transform.right * MoveSpeed; }
                            if (Keyboard.current.hKey.wasPressedThisFrame) { targetPosition -= cam.transform.right * MoveSpeed; }
                            if (Keyboard.current.iKey.wasPressedThisFrame) { targetPosition += cam.transform.up * MoveSpeed; }
                            if (Keyboard.current.yKey.wasPressedThisFrame) { targetPosition -= cam.transform.up * MoveSpeed; }
                            #endregion
                            cam.transform.position = Vector3.Lerp(cam.transform.position, targetPosition, lerpingSpeed * Time.deltaTime);
                            if (CopyRottation)
                            {
                                cam.transform.rotation = MainCam.transform.rotation;
                            }
                        }
                        if (Keyboard.current.pKey.wasPressedThisFrame)
                        {
                            if (Dropped)
                            {
                                CopyRottation = !CopyRottation;
                                SwitchCamState();
                            }
                            else
                            {
                                Behind = !Behind;
                                SwitchCamState();
                            }
                        }
                        if (Keyboard.current.homeKey.wasPressedThisFrame)
                        {
                            hide = !hide;
                            me.playerHudUIContainer.GetComponent<RectTransform>().localPosition = Hide();
                        }
                    }
                }
            }
            catch
            {
                //Stopping errors untill player exists
            }
        }
        Vector3 Hide()
        {
            if (hide)
            {
                return new Vector3 (0, 5555, 0);
            }
            else
            {
                return Vector3.zero;
            }
        }

        void SwitchCamState()
        {
            if (Dropped)
            {
                cam.transform.parent = null;
                targetPosition = cam.transform.position;
            }
            else
            {
                cam.transform.SetParent(MainCam.transform, false);
                cam.transform.localRotation = rotPerspective();
                cam.transform.localPosition = Perspective();
            }
        }
    }
}