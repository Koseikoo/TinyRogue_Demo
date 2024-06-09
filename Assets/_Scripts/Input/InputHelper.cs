using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using UnityEngine.EventSystems;

public static class InputHelper
{
    private static Vector2 _startPosition;
    private static Vector2 _touchPosition;
    private const float MinSwipeMagnitude = 20;
    private static float StationaryThreshold => .01f * Screen.height ;


    public static Vector2 SwipeVector => _touchPosition - _startPosition;
    public static Vector2 StartPosition => _startPosition;
    public static bool IsSwipe => SwipeVector.magnitude > MinSwipeMagnitude;

    public static bool StartedOverUI;
    public static void ProcessSwipeInput()
    {
        if (TouchStarted())
        {
            _startPosition = GetTouchPosition();
            _touchPosition = _startPosition;
            StartedOverUI = IsPointerOverUIObject();
        }

        if (IsTouching() || IsSwiping() || TouchEnded() || SwipeEnded())
        {
            _touchPosition = GetTouchPosition();
        }
    }
    
    public static bool TouchStarted()
    {
        if (Input.touchCount < 1)
            return false;

        Touch touch = Input.GetTouch(0);
        return touch.phase == TouchPhase.Began;
    }

    public static bool IsTouching()
    {
        if (Input.touchCount < 1)
            return false;

        Touch touch = Input.GetTouch(0);
        return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
    }

    public static bool TouchEnded()
    {
        if (Input.touchCount < 1)
            return false;

        Touch touch = Input.GetTouch(0);
        bool touchEnded = touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended;
        return touchEnded && !IsSwipe;
    }

    public static bool IsSwiping()
    {
        if (Input.touchCount < 1)
            return false;
        
        Touch touch = Input.GetTouch(0);
        bool swipeInProgress = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        return IsSwipe && swipeInProgress;
    }

    public static bool IsSwipeStationary()
    {
        if (Input.touchCount < 1)
            return false;
        
        Touch touch = Input.GetTouch(0);
        bool isSwipeStationary = touch.deltaPosition.magnitude < StationaryThreshold;
        return IsSwipe && isSwipeStationary;
    }

    public static bool SwipeEnded()
    {
        if (Input.touchCount < 1)
            return false;
        
        Touch touch = Input.GetTouch(0);
        bool touchEnded = touch.phase == TouchPhase.Ended;
        return touchEnded && IsSwipe;
    }

    public static Vector2 GetTouchPosition()
    {
        if (Input.touchCount < 1)
            return default;

        return Input.GetTouch(0).position;
    }

    private static bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}