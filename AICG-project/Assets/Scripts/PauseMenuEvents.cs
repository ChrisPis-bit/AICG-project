using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenuEvents : MonoBehaviour
{
    private UIDocument _document;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        _document.rootVisualElement.transform.position = Vector3.one * 10000;
    }
}
