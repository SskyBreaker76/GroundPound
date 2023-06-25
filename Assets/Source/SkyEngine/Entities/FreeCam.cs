using SkySoft;
using SkySoft.IO;
using SkySoft.Steam;
using SkySoft.UI;
using Steamworks;
using Steamworks.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FreeCam : MonoBehaviour
{
    private bool Pivot => (Mouse.current != null && Mouse.current.middleButton.isPressed) || (Gamepad.current != null && Gamepad.current.leftTrigger.isPressed);
    private bool Reset => (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) || (Gamepad.current != null && Gamepad.current.rightStickButton.isPressed);
    private bool Moveup => (Keyboard.current != null && Keyboard.current.eKey.isPressed) || (Gamepad.current != null && Gamepad.current.rightShoulder.isPressed);
    private bool Movedown => (Keyboard.current != null && Keyboard.current.qKey.isPressed) || (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);

    public static bool Enabled;
    private bool RestrictiveMode = true;
    [SerializeField] private bool m_IsEnabled;
    [SerializeField] private CharacterController m_Char;
    [SerializeField] private Rigidbody m_RigidBody;
    [SerializeField] private Collider m_Collider;
    public float MoveSpeed = 5;
    public float SpeedMultiplier => ConfigManager.GetOption("PhotoMode.Speed", 1);
    public float FastMoveSpeed = 8;
    public float LookSpeed = 45;
    private Vector3 LookCoord;
    public Canvas HUD;
    public Canvas Config;
    public Text InformationDump;
    public CommandMenu BaseMenu;

    private Camera Cam => GetComponent<Camera>();

    private void Update()
    {
        Enabled = m_IsEnabled;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            m_IsEnabled = !m_IsEnabled;

            if (m_IsEnabled)
            {
                BaseMenu.enabled = true;
                RestrictiveMode = !Keyboard.current.rKey.isPressed;
            }

            HUD.targetDisplay = m_IsEnabled ? 1 : 0;
            Config.targetDisplay = m_IsEnabled ? 0 : 1;
        }

        // m_Collider.enabled = Enabled;
        m_Char.enabled = Enabled;

        if (Enabled)
        {
            Cam.fieldOfView = Mathf.MoveTowards(Cam.fieldOfView, ConfigManager.GetOption("PhotoMode.FieldOfView", 70), Time.deltaTime * 90);

            InformationDump.text = 
                $"<b>Pitch</b>:\t{transform.localEulerAngles.x.ToString("0.0")}\n" +
                $"<b>Yaw</b>:\t{transform.localEulerAngles.y.ToString("0.0")}\n" +
                $"<b>Roll</b>:\t{transform.localEulerAngles.z.ToString("0.0")}\n" +
                $"\n" +
                $"<b>FOV</b>:\t\t{Cam.fieldOfView.ToString("0.0")}";

            Vector2 MouseInput = SkyEngine.Input.Gameplay.LookAround.ReadValue<Vector2>();


            if (!Pivot)
            {
                LookCoord.x += MouseInput.x * Time.unscaledDeltaTime * LookSpeed;
                LookCoord.y += MouseInput.y * Time.unscaledDeltaTime * LookSpeed;
            }
            else
            {
                if (Reset)
                {
                    LookCoord.z = 0;
                }

                LookCoord.z += MouseInput.x * Time.unscaledDeltaTime * LookSpeed;
            }

            LookCoord.y = Mathf.Clamp(LookCoord.y, -80, 90);
            transform.rotation = Quaternion.Euler(new Vector3(LookCoord.y, LookCoord.x, LookCoord.z));

            Vector3 V = Vector3.zero;

            float S = Reset ? FastMoveSpeed : MoveSpeed;

            Vector2 Input = SkyEngine.Input.Gameplay.Move.ReadValue<Vector2>();

            V += transform.forward * S * Input.y;
            V += transform.right * S * Input.x;
            V += transform.up * S * (Moveup ? 1 : 0);
            V -= transform.up * S * (Movedown ? 1 : 0);

            m_Char.Move(V * Time.unscaledDeltaTime);

            if (RestrictiveMode)
            {
                Vector3 LPos = SkyEngine.PlayerEntity.transform.InverseTransformPoint(transform.position);
                LPos = Vector3.ClampMagnitude(LPos, 10);
                transform.position = SkyEngine.PlayerEntity.transform.TransformPoint(LPos);
            }

            if (HUD)
            {
                if (Keyboard.current.f2Key.wasPressedThisFrame)
                {
                    CaptureScreenshot(() => { });
                }
            }
        }
        else
        {
            Cam.fieldOfView = 60;
        }
    }

    private Action Complete;

    private async void CaptureScreenshot(Action Complete)
    {
        this.Complete = Complete;
        Config.targetDisplay = 1;
        await Task.Delay(250);
        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Erinheim/Screenshots/");

        string Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Erinheim/Screenshots/{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}_{DateTime.Now.Ticks}.png";

        ScreenCapture.CaptureScreenshot(Path);

        Config.targetDisplay = 0;
        Complete();

        Screenshot? S = SteamScreenshots.AddScreenshot(Path, "", Screen.width, Screen.height);
    
        if (S.HasValue)
        {
            S.Value.SetLocation(SkyEngine.Levels.GetDisplayName(SceneManager.GetActiveScene().buildIndex));
            S.Value.TagUser(Steamhook.LocalUserID);

            if (SkyEngine.IsOnline)
            {
                foreach (Friend F in Steamhook.CurrentLobby.Members)
                {
                    if (!F.IsMe)
                        S.Value.TagUser(F.Id);
                }
            }
        }
    }
}
