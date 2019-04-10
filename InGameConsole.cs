using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameConsole : MonoBehaviour
{
    struct Log
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }

    static readonly Dictionary<LogType, string> logTypeColors = new Dictionary<LogType, string>()
    {
        { LogType.Assert, "white" },
        { LogType.Error, "red" },
        { LogType.Exception, "red" },
        { LogType.Log, "white" },
        { LogType.Warning, "yellow" },
    };

    
    /// <summary>
    /// The hotkey to show and hide the console window.
    /// </summary>
    public KeyCode toggleKey = KeyCode.BackQuote;
    
    /// <summary>
    /// Is the console visible at the start.
    /// </summary>
    public bool activeAtStart = false;


    List<Log> logs = new List<Log>();
    public void ClearLogs() { logs.Clear(); }

    uint duplicates = 0;
    bool isAtBottom = true;


    /// <summary>
    /// put a canvas element in which the console will be rendered. canvas take up all the screen if left empty.
    /// </summary>
    public GameObject consoleCanvas;

    public int scrollbarWidth = 10;
    public int margin = 20;


    GameObject scrollview;
    GameObject scrollBar;
    GameObject handle;
    GameObject consoleText;

    Text m_Text;
    Scrollbar m_Scrollbar;
    ScrollRect m_ScrollRect;


    // Start is called before the first frame update
    void Start()
    {
        RectTransform m_RectTransform;

        if(consoleCanvas == null)
        {
            consoleCanvas = new GameObject("console", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            consoleCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        scrollview = new GameObject("scrollView", typeof(CanvasRenderer), typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollview.transform.SetParent(consoleCanvas.transform);

        m_RectTransform = scrollview.GetComponent<RectTransform>();
        m_RectTransform.sizeDelta = new Vector2(-(margin*2), -(margin * 2));
        m_RectTransform.anchorMin = new Vector2(0, 0);
        m_RectTransform.anchorMax = new Vector2(1, 1);
        m_RectTransform.anchoredPosition = new Vector2(0, 0);

        m_ScrollRect = scrollview.GetComponent<ScrollRect>();
        m_ScrollRect.horizontal = false;
        m_ScrollRect.inertia = false;
        m_ScrollRect.movementType = ScrollRect.MovementType.Clamped;

        scrollview.GetComponent<Image>().color = Color.black;


        scrollBar = new GameObject("scrollBar", typeof(CanvasRenderer), typeof(Scrollbar));
        scrollBar.transform.SetParent(consoleCanvas.transform);

        m_RectTransform = scrollBar.GetComponent<RectTransform>();
        m_RectTransform.sizeDelta = new Vector2(scrollbarWidth, -(margin * 2));
        m_RectTransform.anchorMin = new Vector2(1, 0);
        m_RectTransform.anchorMax = new Vector2(1, 1);
        m_RectTransform.anchoredPosition = new Vector2(-(margin + scrollbarWidth), 0);
        m_RectTransform.pivot = new Vector2(0.5f, 0.5f);


        handle = new GameObject("handle", typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(scrollBar.transform);

        m_RectTransform = handle.GetComponent<RectTransform>();
        m_RectTransform.sizeDelta = new Vector2(scrollbarWidth, 0);
        m_RectTransform.anchoredPosition = new Vector2(0, 0);
        m_RectTransform.pivot = new Vector2(0.5f, 0.5f);

        m_Scrollbar = scrollBar.GetComponent<Scrollbar>();
        m_Scrollbar.targetGraphic = handle.GetComponent<Image>();
        m_Scrollbar.handleRect = handle.GetComponent<RectTransform>();
        m_Scrollbar.direction = Scrollbar.Direction.BottomToTop;

        m_ScrollRect.verticalScrollbar = m_Scrollbar;
        m_ScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;


        consoleText = new GameObject("text", typeof(CanvasRenderer), typeof(Text), typeof(ContentSizeFitter));
        consoleText.transform.SetParent(scrollview.transform);
        scrollview.GetComponent<ScrollRect>().content = m_RectTransform;

        consoleText.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        m_RectTransform = consoleText.GetComponent<RectTransform>();
        m_RectTransform.sizeDelta = new Vector2(-50, 0);
        m_RectTransform.anchorMin = new Vector2(0, 0);
        m_RectTransform.anchorMax = new Vector2(1, 1);
        m_RectTransform.anchoredPosition = new Vector2(-15, 0);
        m_RectTransform.pivot = new Vector2(0.5f, 1.0f);

        scrollview.GetComponent<ScrollRect>().content = m_RectTransform;

        m_Text = consoleText.GetComponent<Text>();
        m_Text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

        m_Text.text = "";

        if (!activeAtStart)
            consoleCanvas.SetActive(false);
    }


    /// <summary>
    /// Records a log from the log callback.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="stackTrace">Trace of where the message came from.</param>
    /// <param name="type">Type of message (error, exception, warning, assert).</param>
    void HandleLog(string message, string stackTrace, LogType type)
    {
        logs.Add(new Log()
        {
            message = message,
            stackTrace = stackTrace,
            type = type,
        });
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            consoleCanvas.SetActive(!consoleCanvas.activeInHierarchy);
        }

        if (consoleCanvas.activeInHierarchy)
        {
            isAtBottom = (m_Scrollbar.value == 0);

            m_Text.text = "";
            duplicates = 0;

            for (int index = 0; index < logs.Count; index++)
            {
                var log = logs[index];

                if (index+1 < logs.Count && log.message == logs[index+1].message)
                {

                    duplicates += 1;
                    continue;
                }
                else
                {
                    if (duplicates > 0)
                    {
                        m_Text.text += "<color=" + logTypeColors[log.type] + ">" + log.message + "</color>" + " <color=green>(" + (duplicates + 1).ToString() + ")</color>" + "\n";
                        duplicates = 0;
                        continue;
                    }
                }
                m_Text.text += "<color=" + logTypeColors[log.type] + ">" + log.message + "</color> \n";
            }
            if (isAtBottom)
                m_ScrollRect.verticalNormalizedPosition = 0;
        }
    }
}
