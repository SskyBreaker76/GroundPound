using UnityEngine;
using System.Threading.Tasks;

namespace SkySoft.LevelManagement
{
    public enum TriggerMode
    {
        Internal,
        External
    }

    [RequireComponent(typeof(BoxCollider), typeof(LineRenderer)), AddComponentMenu("SkyEngine/Level Management/Load Trigger")]
    public class LoadTrigger : MonoBehaviour
    {
        public TriggerMode TriggerMode;
        public static string ComingFrom = "";
        private BoxCollider Collider => GetComponent<BoxCollider>();
        private LineRenderer Line => GetComponent<LineRenderer>();

        public int Width;
        public Transform Label;
        public Transform TextParent;
        public float TextHeight;
        [SceneReference]
        public string Level;
        public string Display;
        public FadeColour FadeColour = FadeColour.Black;
        private bool IsLoading = false;

        public Vector3 m_CameraPosition = new Vector3(0, 5, 15);

        public Vector3 CameraPosition => transform.position + (transform.forward * m_CameraPosition.z) + (transform.right * m_CameraPosition.x) + (transform.up * m_CameraPosition.y);
        [SerializeField] private Vector3 m_StartPosition = new Vector3(0, 0, 3);
        public Vector3 StartPosition => transform.position + ((transform.right * m_StartPosition.x) + (transform.up * m_StartPosition.y) + (transform.forward * m_StartPosition.z));

        public float GizmosScale = 1;

        private void OnDrawGizmos()
        {
            SkyEngine.Gizmos.Colour = Color.red;
           SkyEngine.Gizmos.DrawWireSphere(CameraPosition, 1);

            SkyEngine.Gizmos.Matrix = Matrix4x4.TRS(StartPosition, transform.rotation, Vector3.one * GizmosScale);
           SkyEngine.Gizmos.DrawWireSphere(Vector3.zero, 0.5f);
            SkyEngine.Gizmos.Colour = Color.yellow * new Color(1, 1, 1, 0.3f);
           SkyEngine.Gizmos.DrawMesh(SkyEngine.Properties.HumanoidGizmos, SkyEngine.Properties.PositionOffset, Quaternion.Euler(SkyEngine.Properties.RotationOffset), Vector3.one * SkyEngine.Properties.Scale);
        }

        private void OnValidate()
        {
            Collider.size = new Vector3(Width, 100, 1);
            Collider.isTrigger = true;

            Line.SetPositions(new Vector3[]
            {
                new Vector3(-Width / 2, 0, 0),
                new Vector3(Width / 2, 0, 0)
            });

            IsOnValidate = true;
            Update();
        }

        private void UpdateLabel()
        {
            UnityEngine.UI.Text Txt;

            if (Txt = Label.GetComponent<UnityEngine.UI.Text>())
            {
                try
                {
                    Txt.text = SkyEngine.Levels.GetDisplayName(Level, !IsOnValidate);

                    if (Txt.text == "")
                        Txt.text = "Invalid Level";
                }
                catch
                {
                    Txt.text = "Invalid Level";
                }

                if (TextParent)
                {
                    if (SkyEngine.PlayerEntity)
                    {
                        Vector3 LabelPosition = transform.GetChild(0).InverseTransformPoint(SkyEngine.PlayerEntity.transform.position);
                        LabelPosition.x = Mathf.Clamp(LabelPosition.x, -Width / 2, Width / 2);
                        LabelPosition.y = TextHeight;
                        LabelPosition.z = 0;
                        TextParent.localPosition = LabelPosition;
                    }
                    else
                    {
                        TextParent.localPosition = new Vector3(0, TextHeight, 0);
                    }
                }
            }

            IsOnValidate = false;
        }

        private bool IsOnValidate = false;
        private void Update()
        {
            UpdateLabel();

            try
            {
                if (Label)
                {
                    Label.LookAt(Camera.main.transform);
                }
            }
            catch { }
        }

        private void Awake()
        {
            IsDoingLoad = false;
        }

        private Entities.Entity T;

        public void SetInteractingEntity(Entities.Entity Entity)
        {
            T = Entity;
        }

        public void TriggerLoad(int Delay = 2000)
        {                
            if (!IsLoading)
            {
                StartLoad(T, Delay);
            }
        }

        public void LoadWithoutEntity(int Delay = 2000)
        {
            if (!IsLoading)
            {
                StartLoad(null, Delay);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerMode == TriggerMode.Internal)
            {
                if (!IsLoading)
                {
                    if (other.GetComponent<Entities.Entity>() && other.GetComponent<Entities.Entity>() == SkyEngine.PlayerEntity)
                    {
                        StartLoad(other.GetComponent<Entities.Entity>());
                    }
                }
            }
        }

        public static bool IsDoingLoad = false;

        public bool MoveLeader = true;

        private async void StartLoad(Entities.Entity Leader, int Delay = 2000)
        {
            transform.GetChild(0).gameObject.SetActive(false);

            IsDoingLoad = true;
            Delay = Mathf.Clamp(Delay, 1, 10000);

            ComingFrom = SkyEngine.Levels.GetShortKey(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            IsLoading = true;

            if (Leader)
            {
                Camera.main.transform.position = CameraPosition;

                if (MoveLeader)
                {
                    Leader.transform.rotation = transform.rotation;
                    Leader.Move(new Vector3(0, 0, 1));

                    await Task.Delay(Delay);
                }
            }

            if (!string.IsNullOrEmpty(Level))
                LevelManager.LoadLevel(Level, FadeColour, () => { }, SkyEngine.LoadedFile);
        }

        public static LoadTrigger GetLoadTrigger(string TargetLevel)
        {
            foreach (LoadTrigger Trigger in FindObjectsOfType<LoadTrigger>()) 
            {
                if (Trigger.Level.ToLower() == TargetLevel.ToLower())
                {
                    return Trigger;
                }
            }

            return null;
        }
    }
}