using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DebugInput : MonoBehaviour
{
    public UnityEvent testEvent_input_1;
    public UnityEvent testEvent_input_2;
    public UnityEvent testEvent_input_3;
    public UnityEvent testEvent_input_4;
    public UnityEvent testEvent_input_5;
    public UnityEvent testEvent_input_6;
    public UnityEvent testEvent_input_7;
    public UnityEvent testEvent_input_8;
    public UnityEvent testEvent_input_9;
    public UnityEvent testEvent_input_10;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.f1Key.wasPressedThisFrame) TestEvent(1);
            if (UnityEngine.InputSystem.Keyboard.current.f2Key.wasPressedThisFrame) TestEvent(2);
            if (UnityEngine.InputSystem.Keyboard.current.f3Key.wasPressedThisFrame) TestEvent(3);
            if (UnityEngine.InputSystem.Keyboard.current.f4Key.wasPressedThisFrame) TestEvent(4);
            if (UnityEngine.InputSystem.Keyboard.current.f5Key.wasPressedThisFrame) TestEvent(5);
            if (UnityEngine.InputSystem.Keyboard.current.f6Key.wasPressedThisFrame) TestEvent(6);
            if (UnityEngine.InputSystem.Keyboard.current.f7Key.wasPressedThisFrame) TestEvent(7);
            if (UnityEngine.InputSystem.Keyboard.current.f8Key.wasPressedThisFrame) TestEvent(8);
            if (UnityEngine.InputSystem.Keyboard.current.f9Key.wasPressedThisFrame) TestEvent(9);
            if (UnityEngine.InputSystem.Keyboard.current.f10Key.wasPressedThisFrame) TestEvent(10);
        }
    }

    private void TestEvent(int i)
    {
        if (i == 1) testEvent_input_1.Invoke();
        if (i == 2) testEvent_input_2.Invoke();
        if (i == 3) testEvent_input_3.Invoke();
        if (i == 4) testEvent_input_4.Invoke();
        if (i == 5) testEvent_input_5.Invoke();
        if (i == 6) testEvent_input_6.Invoke();
        if (i == 7) testEvent_input_7.Invoke();
        if (i == 8) testEvent_input_8.Invoke();
        if (i == 9) testEvent_input_9.Invoke();
        if (i == 10) testEvent_input_10.Invoke();
    }
}