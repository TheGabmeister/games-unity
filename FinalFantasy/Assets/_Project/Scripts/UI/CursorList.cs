using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class CursorList : MonoBehaviour
{
    [SerializeField] RectTransform contentParent;
    [SerializeField] GameObject itemTemplate; // must have a Selectable (Button)
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] TextMeshProUGUI cursorText; // the "▶" cursor indicator

    readonly List<GameObject> items = new();
    int selectedIndex = -1;

    public event Action<int> OnItemSelected;
    public event Action<int> OnItemConfirmed;

    public int SelectedIndex => selectedIndex;
    public int Count => items.Count;

    public void SetItems(string[] labels)
    {
        Clear();

        for (int i = 0; i < labels.Length; i++)
        {
            var item = Instantiate(itemTemplate, contentParent);
            item.SetActive(true);

            var text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = labels[i];

            int index = i; // capture for closure
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ConfirmItem(index));
            }

            items.Add(item);
        }

        // Wire explicit navigation (wrap around)
        for (int i = 0; i < items.Count; i++)
        {
            var sel = items[i].GetComponent<Selectable>();
            if (sel == null) continue;

            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = items[(i - 1 + items.Count) % items.Count].GetComponent<Selectable>();
            nav.selectOnDown = items[(i + 1) % items.Count].GetComponent<Selectable>();
            sel.navigation = nav;
        }

        // Select first
        if (items.Count > 0)
            SelectItem(0);

        if (itemTemplate != null) itemTemplate.SetActive(false);
    }

    public void SelectItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        selectedIndex = index;

        var sel = items[index].GetComponent<Selectable>();
        if (sel != null)
            EventSystem.current?.SetSelectedGameObject(sel.gameObject);

        OnItemSelected?.Invoke(index);

        // Scroll to keep visible
        if (scrollRect != null && items.Count > 0)
        {
            float normalized = 1f - (float)index / Mathf.Max(1, items.Count - 1);
            scrollRect.verticalNormalizedPosition = normalized;
        }
    }

    void ConfirmItem(int index)
    {
        selectedIndex = index;
        GameManager.Instance?.Audio?.PlaySFX(SoundEffect.Confirm);
        OnItemConfirmed?.Invoke(index);
    }

    public void Clear()
    {
        foreach (var item in items)
        {
            if (item != null) Destroy(item);
        }
        items.Clear();
        selectedIndex = -1;
    }
}
