using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphAnimation : MonoBehaviour
{

    [SerializeField] private UIDocument _mainUI;

    void Start()
    {
        
    }

    void Update()
    {
    }

    void AnimateScoreGraph(int[] bars, int[] newHeights, float delay)
    {
        for (int i = 0; i < bars.Length; i++)
        {
            StartCoroutine(AnimateBarHeight(bars[i], newHeights[i], 1f + delay * i));
        }
    }

    IEnumerator AnimateBarHeight(int barIndex, int newHeight, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateScoreGraph(barIndex, newHeight);
    }

    [ContextMenu("Update Score Graph")]
    void UpdateScoreGraph()
    {
        AnimateScoreGraph(
            new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, 
            new int[] {
            Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100), 
            Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100), 
            Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100), 
            Random.Range(0, 100)
            }, 
            0.5f
        );
    }
    
    void UpdateScoreGraph(int barIndex, int newHeight)
    {
        var scoreBars = _mainUI.rootVisualElement.Query<VisualElement>(className: "bar").ToList();
        for (int i = 0; i < scoreBars.Count; i++)
        {
            if (i == barIndex)
            {
                var parent = scoreBars[i].parent;
                parent.style.height = new StyleLength(new Length(newHeight, LengthUnit.Percent));
            }
        }
    }
}