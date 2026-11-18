using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Ratolí sobre una casella de l'inventari (hotbar d'armes o bossa de cures):
// - Clic: tria / usa la casella
// - Arrossegar a una altra casella de la mateixa barra: les intercanvia
// - Arrossegar fora de la barra: tira l'objecte al terra
public class SlotInput : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int index;
    public object group;                       // Les caselles del mateix grup es poden intercanviar
    public Action<int> onClick;
    public Action<int> onDropOutside;
    public Action<int, int> onSwap;
    public Func<int, Sprite> getIcon;

    private static Image ghost;                // Icona que segueix el ratolí mentre arrossegues

    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left && !e.dragging)
            onClick?.Invoke(index);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if(MobileInput.Active) { e.pointerDrag = null; return; }
        Sprite icon = getIcon?.Invoke(index);
        if (icon == null) { e.pointerDrag = null; return; } // Casella buida: no s'arrossega res

        Canvas canvas = GetComponentInParent<Canvas>();
        GameObject go = new GameObject("Arrossegant", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(canvas.transform, false);
        ghost = go.GetComponent<Image>();
        ghost.sprite = icon;
        ghost.preserveAspect = true;
        ghost.raycastTarget = false;
        go.GetComponent<CanvasGroup>().alpha = 0.8f;
        ((RectTransform)go.transform).sizeDelta = ((RectTransform)transform).rect.size * 0.8f;
        ghost.transform.position = e.position;
    }

    public void OnDrag(PointerEventData e)
    {
        if (ghost != null) ghost.transform.position = e.position;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if(MobileInput.Active) return;
        if (ghost != null) Destroy(ghost.gameObject);
        ghost = null;

        GameObject over = e.pointerCurrentRaycast.gameObject;
        SlotInput target = over != null ? over.GetComponentInParent<SlotInput>() : null;

        if (target == null)
        {
            // Si l'has deixat damunt d'una altra UI (cors, monedes...), no fem res; si és el mapa, el tires
            if (over == null) onDropOutside?.Invoke(index);
        }
        else if (target != this && target.group == group)
            onSwap?.Invoke(index, target.index);
    }
}
