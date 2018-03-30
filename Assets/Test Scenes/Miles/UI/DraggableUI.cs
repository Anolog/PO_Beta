
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour {

    private Vector2 m_Offset;
    private RectTransform m_RectTransform;
    private Vector2 m_Helper;

    //Top Left anchor
    //private readonly Vector2 TLAnchor = new Vector2(0, 1);

    private void Start()
    {
        m_RectTransform = GetComponent<RectTransform>();
        if (m_RectTransform == null) m_RectTransform = gameObject.AddComponent<RectTransform>();

        EventTrigger trigger = GetComponent<EventTrigger>();
        if(trigger == null) trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
        beginDragEntry.eventID = EventTriggerType.BeginDrag;
        beginDragEntry.callback.AddListener((data) => { OnDrag(); });

        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback.AddListener((data) => { OnDragStay(); });

        trigger.triggers.Add(beginDragEntry);
        trigger.triggers.Add(dragEntry);
    }

    public void OnDrag()
    {
        m_Offset.x = m_RectTransform.anchoredPosition.x - Input.mousePosition.x;
        m_Offset.y = m_RectTransform.anchoredPosition.y - Input.mousePosition.y;
    }

    public void OnDragStay()
    {
        m_Helper.x = m_Offset.x + Input.mousePosition.x;
        m_Helper.y = m_Offset.y + Input.mousePosition.y;

        m_RectTransform.anchoredPosition = m_Helper;
    }
}
