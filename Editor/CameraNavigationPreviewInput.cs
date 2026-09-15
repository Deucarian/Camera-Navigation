using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.CameraNavigation.Editor
{
    internal sealed class CameraNavigationPreviewInput : IDisposable
    {
        private readonly VisualElement view;
        private readonly Func<bool> readFlyMode;
        private readonly Func<float> readPointerScale;
        private readonly Func<float> readScrollNormalization;
        private readonly HashSet<KeyCode> keys = new HashSet<KeyCode>();
        private Vector2 rotation, pan;
        private Vector2 previousPointer;
        private float zoom;
        private int pointer = -1;
        private int button;
        private bool shift, slow;

        internal CameraNavigationPreviewInput(VisualElement target, Func<bool> flyMode,
            Func<float> pointerScale, Func<float> scrollNormalization)
        {
            view = target;
            readFlyMode = flyMode;
            readPointerScale = pointerScale;
            readScrollNormalization = scrollNormalization;
            view.focusable = true;
            view.RegisterCallback<PointerDownEvent>(Down);
            view.RegisterCallback<PointerMoveEvent>(Move);
            view.RegisterCallback<PointerUpEvent>(Up);
            view.RegisterCallback<PointerCaptureOutEvent>(CaptureOut);
            view.RegisterCallback<WheelEvent>(Wheel);
            view.RegisterCallback<KeyDownEvent>(KeyDown);
            view.RegisterCallback<KeyUpEvent>(KeyUp);
            view.RegisterCallback<FocusOutEvent>(FocusOut);
            view.RegisterCallback<DetachFromPanelEvent>(Detach);
        }

        internal void Read(out DeucarianOrbitCameraInput orbit, out DeucarianFlyCameraInput fly)
        {
            var move = new Vector3(Axis(KeyCode.D, KeyCode.A), Axis(KeyCode.E, KeyCode.Q), Axis(KeyCode.W, KeyCode.S));
            if (move.sqrMagnitude > 1) move.Normalize();
            orbit = new DeucarianOrbitCameraInput(move, rotation, pan, zoom, shift, slow);
            fly = new DeucarianFlyCameraInput(rotation, move, zoom, shift, slow);
            rotation = pan = Vector2.zero;
            zoom = 0;
        }

        private float Axis(KeyCode positive, KeyCode negative) => (keys.Contains(positive) ? 1 : 0) - (keys.Contains(negative) ? 1 : 0);
        private void Down(PointerDownEvent evt)
        {
            if (evt.button < 0 || evt.button > 2 || pointer >= 0) return;
            pointer = evt.pointerId; button = evt.button; previousPointer = evt.position;
            shift = evt.shiftKey; slow = evt.ctrlKey;
            view.Focus(); view.CapturePointer(pointer); evt.StopPropagation(); evt.PreventDefault();
        }
        private void Move(PointerMoveEvent evt)
        {
            if (evt.pointerId != pointer) return;
            Vector2 current = evt.position;
            Vector2 delta = NormalizePointerDelta(current - previousPointer, readPointerScale?.Invoke() ?? .1f);
            previousPointer = current;
            shift = evt.shiftKey; slow = evt.ctrlKey;
            if (!readFlyMode() && (button == 2 || shift)) pan += new Vector2(delta.x, -delta.y);
            else rotation += new Vector2(delta.x, -delta.y);
            evt.StopPropagation();
        }
        private void Up(PointerUpEvent evt)
        {
            if (evt.pointerId != pointer) return;
            int captured = pointer; pointer = -1; view.ReleasePointer(captured); evt.StopPropagation();
        }
        private void Wheel(WheelEvent evt)
        {
            zoom += NormalizeScroll(evt.delta.y, readScrollNormalization?.Invoke() ?? 120f);
            shift = evt.shiftKey; slow = evt.ctrlKey;
            evt.StopPropagation(); evt.PreventDefault();
        }
        private static bool IsNavigationKey(KeyCode key) => key == KeyCode.W || key == KeyCode.A || key == KeyCode.S ||
            key == KeyCode.D || key == KeyCode.Q || key == KeyCode.E || key == KeyCode.LeftShift ||
            key == KeyCode.RightShift || key == KeyCode.LeftControl || key == KeyCode.RightControl;
        private void KeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Escape) { Clear(); evt.StopPropagation(); return; }
            if (!IsNavigationKey(evt.keyCode)) return;
            keys.Add(evt.keyCode); shift = evt.shiftKey; slow = evt.ctrlKey; evt.StopPropagation(); evt.PreventDefault();
        }
        private void KeyUp(KeyUpEvent evt)
        {
            if (!IsNavigationKey(evt.keyCode)) return;
            keys.Remove(evt.keyCode); shift = evt.shiftKey; slow = evt.ctrlKey; evt.StopPropagation();
        }
        private void CaptureOut(PointerCaptureOutEvent evt) { if (evt.pointerId == pointer) Clear(); }
        private void FocusOut(FocusOutEvent evt) => Clear();
        private void Detach(DetachFromPanelEvent evt) => Clear();

        internal static Vector2 NormalizePointerDelta(Vector2 delta, float scale)
            => delta * (float.IsNaN(scale) || float.IsInfinity(scale) ? .1f : Mathf.Max(0, scale));

        internal static float NormalizeScroll(float editorLines, float normalization)
        {
            if (float.IsNaN(normalization) || float.IsInfinity(normalization)) normalization = 120f;
            // UI Toolkit uses three lines per detent; the runtime Input System uses 120 units.
            return -editorLines * 40f / Mathf.Max(.0001f, normalization);
        }

        internal void Clear()
        {
            int captured = pointer; pointer = -1;
            if (captured >= 0 && view.HasPointerCapture(captured)) view.ReleasePointer(captured);
            keys.Clear(); rotation = pan = Vector2.zero; zoom = 0; shift = slow = false;
        }

        public void Dispose()
        {
            Clear();
            view.UnregisterCallback<PointerDownEvent>(Down);
            view.UnregisterCallback<PointerMoveEvent>(Move);
            view.UnregisterCallback<PointerUpEvent>(Up);
            view.UnregisterCallback<PointerCaptureOutEvent>(CaptureOut);
            view.UnregisterCallback<WheelEvent>(Wheel);
            view.UnregisterCallback<KeyDownEvent>(KeyDown);
            view.UnregisterCallback<KeyUpEvent>(KeyUp);
            view.UnregisterCallback<FocusOutEvent>(FocusOut);
            view.UnregisterCallback<DetachFromPanelEvent>(Detach);
        }
    }
}
