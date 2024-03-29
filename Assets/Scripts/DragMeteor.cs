using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragMeteor : MonoBehaviour
{
    public Game _game;
    private bool isDragging = false;
    private Vector3 offset;

    public void MouseUp()
    {
        isDragging = false;
        _game.ThrowMeteor();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;

            Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePos) + offset;
            transform.position = objPosition;
        }
    }

    public void Drag()
    {
        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
