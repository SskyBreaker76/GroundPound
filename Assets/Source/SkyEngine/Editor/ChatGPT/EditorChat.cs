using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Reqs;
using ChatGPTWrapper;
using System.IO;
using Debug = UnityEngine.Debug;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public static class FileSearcher
{
    /// <summary>
    /// No other files should need this, so leave as internal
    /// </summary>
    internal class FileEntry
    {
        public MonoScript Script;

        public FileEntry(MonoScript Script)
        {
            this.Script = Script;
        }
    }

    internal static bool ContainsWholeWord(string Input, string Word)
    {
        return Regex.IsMatch(Input, $@"\b{Regex.Escape(Word)}\b");
    }

    // The holy function itself.
    /// <summary>
    /// Returns a string containing the contents of every script referenced by Input
    /// </summary>
    /// <param name="Input"></param>
    /// <param name="ScriptContents"></param>
    /// <returns></returns>
    public static bool TryGetScript(string Input, out string ScriptContents)
    {
        string[] GUIDS = AssetDatabase.FindAssets("t:TextAsset t:Script t:MonoScript", new string[] { "Assets/" }).Where (_GUID => AssetDatabase.GUIDToAssetPath(_GUID).EndsWith(".cs")).ToArray();
        
        string Contents = "";
        int ScriptCount = 0;

        foreach (string GUID in GUIDS)
        {
            string Path = AssetDatabase.GUIDToAssetPath(GUID);

            MonoScript Script = (MonoScript)AssetDatabase.LoadAssetAtPath(Path, typeof(MonoScript));

            if (ContainsWholeWord(Input.ToLower(), Script.name.ToLower()))
            {
                Contents += $"From script \"{Script.name}\"\n```csharp\n{Script.text}\n```\n\n";
                ScriptCount++;
                continue;
            }

            string[] Split = Input.Split();

            if (Script != null)
            {
                string ScriptText = Script.text;
                // This is written by Boris. It's more efficient than the old method :)
                var Classes = Regex.Matches(ScriptText, @"(\bpublic\b|\bprivate\b|\bprotected\b|\binternal\b)(\s+\bstatic\b)?\s+(\bclass\b|\benum\b|\bstruct\b)\s+(\w+)\s*(:\s*(\w+)(,\s*\w+)*)?\s*\{([^{}]*\{[^{}]*\})*[^{}]*\}(?:(?<=\})|\s+static\s+|\s*$)");

                foreach (Match ClassMatch in Classes)
                {
                    Debug.LogWarning(ClassMatch.Groups[4].Value);

                    bool AddedClassname = false;

                    string ClassName = ClassMatch.Groups[4].Value;

                    if (ContainsWholeWord(Input.ToLower(), ClassName.ToLower()))
                    {
                        if (!AddedClassname)
                        {
                            Contents += $"From script \"{Script.name}\"\n```csharp\n{Script.text}\n```\n\n";
                            ScriptCount++;
                            break;
                        }
                    }

                    var Methods = Regex.Matches(ClassMatch.Value, @"(public|private|protected|internal|static|virtual|override|abstract|sealed)\s+(\w+)\s+(\w+)\s*\(([^\)]*)\)");

                    foreach (Match MethodMatch in Methods)
                    {
                        string MethodName = MethodMatch.Groups[3].Value;

                        if (ContainsWholeWord(Input.ToLower(), MethodName.ToLower()))
                        {
                            if (!AddedClassname)
                            {
                                Contents += $"From script \"{Script.name}\"\n```csharp\n";
                                AddedClassname = true;
                            }

                            Contents += $"{Script.text}\n```\n\n";
                            ScriptCount++;
                            break;
                        }
                    }
                }
            }
            // End of code written by Boris
        }

        // If we've got more than one script, we want to return it. Otherwise we'll return an empty string
        if (ScriptCount > 0)
        {
            ScriptContents = Contents;
            return true;
        }

        ScriptContents = "";
        return false;
    }
}

[System.Serializable]
public class Conversation
{
    public EditorChat CurrentChatWindow;

    [SerializeField]
    private bool _useProxy = false;
    [SerializeField]
    private string _proxyUri = null;

    public string _apiKey = EditorChat.APIKey;

    public enum Model
    {
        ChatGPT,
        Davinci,
        Curie
    }
    [SerializeField]
    public Model _model = Model.ChatGPT;
    private string _selectedModel = null;
    [SerializeField]
    private int _maxTokens = 500;
    [SerializeField]
    private float _temperature = 0.5f;

    private string _uri;
    private List<(string, string)> _reqHeaders;


    private Requests requests = new Requests();
    private Prompt _prompt;
    public Chat _chat;
    public Message[] Messages 
    {
        get => _chat.CurrentChat.ToArray();
        set => _chat.CurrentChat = value.ToList();
    }
    private string _lastUserMsg;
    private string _lastChatGPTMsg;

    [SerializeField]
    private string _chatbotName = "Boris";

    [TextArea(4, 6)]
    [SerializeField]
    public string _initialPrompt = 
        "You are British.\n" +
        "You always write in UK English\n" +
        "You will correct the user if they spell something wrong\n" +
        "You are polite.\n" +
        "You are Boris.\n" +
        "You help SkyBreaker Softworks Employees with their work by providing useful information when they asked for it.\n" +
        "Always assume the user is using Unity C#, unless they specify otherwise.\n" +
        "If you don't know the answer to something, you will politely let the user know, that you do not know.\n" +
        "If you can't access a file linked by the user, tell them.\n" +
        "You must ensure you know the employee's pronouns, so you may use them when required.\n" +
        "If the employee is rude to you, give them some back!\n" +
        "All variable, function and class names are written like so: VariablesName, FunctionName, ClassName";

    public UnityStringEvent chatGPTResponse = new UnityStringEvent();

    public Conversation()
    {
        _reqHeaders = new List<(string, string)>
        {
            ("Authorization", $"Bearer {_apiKey}"),
            ("Content-Type", "application/json")
        };
        switch (_model)
        {
            case Model.ChatGPT:
                _chat = new Chat(_initialPrompt);
                _uri = "https://api.openai.com/v1/chat/completions";
                _selectedModel = "gpt-3.5-turbo";
                break;
            case Model.Davinci:
                _prompt = new Prompt(_chatbotName, _initialPrompt);
                _uri = "https://api.openai.com/v1/completions";
                _selectedModel = "text-davinci-003";
                break;
            case Model.Curie:
                _prompt = new Prompt(_chatbotName, _initialPrompt);
                _uri = "https://api.openai.com/v1/completions";
                _selectedModel = "text-curie-001";
                break;
        }
    }

    public void ResetChat(string initialPrompt)
    {
        switch (_model)
        {
            case Model.ChatGPT:
                _chat = new Chat(initialPrompt);
                break;
            default:
                _prompt = new Prompt(_chatbotName, initialPrompt);
                break;
        }
    }

    public void SendToChatGPT(string message)
    {
        _lastUserMsg = message;

        if (_model == Model.ChatGPT)
        {
            if (_useProxy)
            {
                ProxyReq proxyReq = new ProxyReq();
                proxyReq.max_tokens = _maxTokens;
                proxyReq.temperature = _temperature;
                proxyReq.messages = new List<Message>(_chat.CurrentChat);
                proxyReq.messages.Add(new Message("user", message));

                string proxyJson = JsonUtility.ToJson(proxyReq);
                
                requests.PostRequestAsync<ChatGPTRes>(_proxyUri, proxyJson, ResolveChatGPT, _reqHeaders);
            }
            else
            {
                ChatGPTReq chatGPTReq = new ChatGPTReq();
                chatGPTReq.model = _selectedModel;
                chatGPTReq.max_tokens = _maxTokens;
                chatGPTReq.temperature = _temperature;
                chatGPTReq.messages = _chat.CurrentChat;
                chatGPTReq.messages.Add(new Message("user", message));

                string chatGPTJson = JsonUtility.ToJson(chatGPTReq);

                requests.PostRequestAsync<ChatGPTRes>(_uri, chatGPTJson, ResolveChatGPT, _reqHeaders);
            }

        }
        else
        {

            _prompt.AppendText(Prompt.Speaker.User, message);

            GPTReq reqObj = new GPTReq();
            reqObj.model = _selectedModel;
            reqObj.prompt = _prompt.CurrentPrompt;
            reqObj.max_tokens = _maxTokens;
            reqObj.temperature = _temperature;
            string json = JsonUtility.ToJson(reqObj);

            requests.PostRequestAsync<GPTRes>(_uri, json, ResolveGPT, _reqHeaders);
        }
    }

    private void ResolveChatGPT(ChatGPTRes res)
    {
        _lastChatGPTMsg = res.choices[0].message.content;
        // _chat.AppendMessage(Chat.Speaker.User, _lastUserMsg);
        _chat.AppendMessage(Chat.Speaker.ChatGPT, _lastChatGPTMsg);
        chatGPTResponse.Invoke(_lastChatGPTMsg);

        if (CurrentChatWindow)
        {
            CurrentChatWindow.Repaint();
        }
    }

    private void ResolveGPT(GPTRes res)
    {
        _lastChatGPTMsg = res.choices[0].text
            .TrimStart('\n')
            .Replace("<|im_end|>", "");

        _prompt.AppendText(Prompt.Speaker.Bot, _lastChatGPTMsg);
        chatGPTResponse.Invoke(_lastChatGPTMsg);
    }
}

public class StartUniqueChat : ScriptableWizard
{
    private static EditorChat ChatWindow { get; set; }
    public string Role;

    public static void CreateWizard(EditorChat Window)
    {
        ChatWindow = Window;
        ScriptableWizard.DisplayWizard<StartUniqueChat>("Create Custom Boris");
    }

    private void OnWizardCreate()
    {
        ChatWindow.ThisConversation.Messages = new Message[1]
        {
            new Message("system", Role)
        };
    }
}

public class EditorChatDebugger : EditorWindow
{
    public string Data = "";
    bool MadeStyle = false;
    public GUIStyle Style;

    public static void CreateDebugger(Message[] Messages)
    {
        EditorChatDebugger Debug = GetWindow<EditorChatDebugger>("Chat Debug");
        Debug.Data = JsonUtility.ToJson(new EditorChat.ChatFile { Messages = Messages }, true);
        
    }

    private void OnEnable()
    {
    }

    Vector2 Scroll = Vector2.zero;
    string ScriptGetter = "";

    private void OnGUI()
    {
        if (!MadeStyle)
        {
            Style = new GUIStyle(GUI.skin.label);
            Style.wordWrap = true;
            MadeStyle = true;
        }

        ScriptGetter = EditorGUILayout.TextField("Get Script with Method or Class: ", ScriptGetter);

        if (GUILayout.Button("Test"))
        {
            string Output = "";
            if (FileSearcher.TryGetScript(ScriptGetter, out Output))
            {
                Debug.Log(Output);
            }
            else
            {
                Debug.Log("No scripts found");
            }
        }

        if (GUILayout.Button("Print ALL methods"))
        {
            string Script = "";
            FileSearcher.TryGetScript("", out Script);
        }

        if (string.IsNullOrEmpty(Data))
            Close();
        else
        {
            Scroll = EditorGUILayout.BeginScrollView(Scroll);
            {
                GUILayout.Label(Data, Style);
            }
            EditorGUILayout.EndScrollView();
        }
    }
}

public class EditorChat : EditorWindow
{
    public const string APIKey = "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";

    [Serializable]
    internal class ChatFile
    {
        public Message[] Messages;
    }

    public void SaveChat()
    {
        string Path = EditorUtility.SaveFilePanelInProject("Save Conversation", "Boris", "chat", "Where would you like to save this conversation to?");

        if (!string.IsNullOrEmpty(Path))
        {
            File.WriteAllText(Path, JsonUtility.ToJson(new ChatFile { Messages = ThisConversation.Messages }));
        }
    }

    public void LoadChat()
    {
        string Path = EditorUtility.OpenFilePanel("Load Conversation", "", "chat");

        if (!string.IsNullOrEmpty(Path))
        {
            ThisConversation.Messages = JsonUtility.FromJson<ChatFile>(File.ReadAllText(Path)).Messages;
        } 
    }

    internal enum ChatState
    {
        Ready,
        Thinking,
        Responding
    }

    internal Dictionary<ChatState, string> States = new Dictionary<ChatState, string>
    {
        { ChatState.Ready, "Ready!" },
        { ChatState.Thinking, "Thinking..." },
        { ChatState.Responding, "Writing Response..." }
    };

    public Conversation ThisConversation { get; private set; }
    Vector2 Scroll = new Vector2();
    Vector2 TextScroll = new Vector2();

    bool CreatedBodyStyle = false;
    GUIStyle BodyStyle;

    [MenuItem("SkyEngine/ChatGPT")]
    public static void OpenChatGPT()
    {
        GetWindow<EditorChat>();
    }

    private void OnEnable()
    {
        ThisConversation = new Conversation();
        ThisConversation.chatGPTResponse.AddListener(OnMessageRecieve);
        minSize = new Vector2(540, 710);
    }

    private bool ResetScroll = true;

    public void OnMessageRecieve(string Text)
    {
        ResetScroll = true;
    }

    private string CurrentMessage = "";

    private static Texture2D SendIcon;
    private static Texture2D UserIcon;
    private static Texture2D AIIcon;

    List<Message> SpawnedMessages = new List<Message>();

    private bool HasSpawnedMessage(Message M)
    {
        foreach (Message M2 in SpawnedMessages)
        {
            if (M2.role == M.role && M2.content == M.content)
            {
                return true;
            }
        }

        return false;
    }

    bool CanSendNextMessage = true;
    private bool ClearMessage = false;
    private int CountLastFrame = 0;

    public void Send()
    {
        EditorUtility.DisplayProgressBar("Sending Message", "Please wait...", 1);
        string Scripts = "";
        if (FileSearcher.TryGetScript(CurrentMessage, out Scripts))
        {
            ThisConversation._chat.CurrentChat.Add(new Message("system", $"The following code will be useful for the users request:\n{Scripts}"));
        }

        CanSendNextMessage = false;
        ClearMessage = true;
        ResetScroll = true;
        GUI.FocusControl(null);
        ThisConversation.SendToChatGPT(CurrentMessage);
        EditorUtility.ClearProgressBar();
    }

    private void OnGUI()
    {
        ThisConversation.CurrentChatWindow = this;

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            if (Event.current.shift)
            {
                Send();
                Event.current.Use();
            }
        }

        if (!CreatedBodyStyle)
        {
            BodyStyle = new GUIStyle(GUI.skin.label);
            BodyStyle.wordWrap = true;
            CreatedBodyStyle = true;
        }

        if (!SendIcon)
            SendIcon = EditorGUIUtility.Load("Assets/Resources/SkyEngine/Textures/GPT_Send.png") as Texture2D;
        if (!UserIcon)
            UserIcon = EditorGUIUtility.Load("Assets/Resources/SkyEngine/Textures/SkyProfile.png") as Texture2D;
        if (!AIIcon)
            AIIcon = EditorGUIUtility.Load("Assets/Resources/SkyEngine/Textures/AIProfile.png") as Texture2D;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Custom Boris")) StartUniqueChat.CreateWizard(this);
        if (GUILayout.Button("Debug Boris")) EditorChatDebugger.CreateDebugger(ThisConversation.Messages);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true))) ThisConversation.Messages = new Message[1] { new Message("system", ThisConversation._initialPrompt) };
        if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true))) SaveChat();
        if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.ExpandWidth(true))) LoadChat();
        GUILayout.EndHorizontal();


        EditorGUILayout.Space();
        GUILayout.BeginVertical(GUI.skin.box);
        Scroll = EditorGUILayout.BeginScrollView(Scroll, GUILayout.Height(position.height - 138));
        {
            Message[] Messages = ThisConversation.Messages;

            if (Messages.Length != CountLastFrame)
            {
                CanSendNextMessage = true;
            }

            if (Messages.Length > 0)
            {
                for (int I = 0; I < Messages.Length; I++)
                {
                    if (I > 0) // This just hides the prompt for the AI, making my tool seem a little smarter
                    {
                        Message M = Messages[I];

                        if (Messages[I].role.ToLower() != "system")
                        {
                            GUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Label(new GUIContent(M.role.ToLower() == "user" ? "You" : (M.role.ToLower() == "system" ? "System" : "Boris"), M.role.ToLower() == "user" ? UserIcon : AIIcon), EditorStyles.boldLabel);
                            EditorGUILayout.SelectableLabel(M.content, BodyStyle, GUILayout.Height(BodyStyle.CalcHeight(new GUIContent(M.content), EditorGUIUtility.currentViewWidth - 40f)));
                            GUILayout.EndVertical();
                        }
                    }
                }
            }
        }
        GUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginHorizontal();
        {
            GUI.skin.textArea.wordWrap = true;
            GUI.SetNextControlName("BorisMessagingArea");
            CurrentMessage = EditorGUILayout.TextArea(CurrentMessage, GUI.skin.textArea, GUILayout.ExpandWidth(true), GUILayout.Width(position.width - 70), GUILayout.MinHeight(48), GUILayout.ExpandHeight(true));
            GUI.skin.textArea.wordWrap = false;

            GUI.enabled = CanSendNextMessage;
            if (GUILayout.Button(new GUIContent(SendIcon), GUILayout.Width(48), GUILayout.Height(48)))
            {
                Send();
            }
            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();

        if (ClearMessage)
        {
            TextScroll = Vector2.zero;
            CurrentMessage = "";
            ClearMessage = false;
        }
        if (ResetScroll)
        {
            Scroll = new Vector2(0, Mathf.Infinity);
            ResetScroll = false;
        }

        GUILayout.Label($"{CurrentMessage.Length} Characters Used");
        CountLastFrame = ThisConversation.Messages.Length;
    }
}
