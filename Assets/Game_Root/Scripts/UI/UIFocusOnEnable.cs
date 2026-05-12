using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIFocusOnEnable : MonoBehaviour {
    [SerializeField] private GameObject target;

    private void OnEnable() {
        StartCoroutine(SetFocus());
    }

    private IEnumerator SetFocus() {
        yield return null;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }
}