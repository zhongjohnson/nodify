using System;
using Avalonia.Input;
using Avalonia;

namespace Nodify.Interactivity
{
    internal class StatefulFocusNavigator<TElement>
        where TElement : Visual, IKeyboardFocusTarget<TElement>
    {
        public delegate bool FindNextFocusTargetDelegate(TElement? currentElement, TraversalRequest request, out TElement? elementToFocus);

        private readonly WeakReference<TElement?> _previousFocusedElement = new WeakReference<TElement?>(null);
        private readonly WeakReference<TElement?> _lastFocusedElement = new WeakReference<TElement?>(null);
        private NavigationDirection? _previousFocusNavigationDirection;

        private readonly Action<IKeyboardFocusTarget<TElement>> _onFocus;

        public TElement? LastFocusedElement => _lastFocusedElement.TryGetTarget(out var target) ? target : null;

        public StatefulFocusNavigator(Action<IKeyboardFocusTarget<TElement>> onFocus)
        {
            _onFocus = onFocus;
        }

        public bool TryMoveFocus(TraversalRequest request, FindNextFocusTargetDelegate findNext)
        {
            // Avalonia doesn't have static Keyboard.FocusedElement - would need TopLevel context
            // For now, pass null as current target
            var currentTarget = default(TElement);

            // If the request is in the opposite direction of the last focus navigation, try to restore the previous focused container
            if (_previousFocusedElement.TryGetTarget(out var prevTarget)
                && _previousFocusNavigationDirection.HasValue
                && request.FocusNavigationDirection.IsOppositeOf(_previousFocusNavigationDirection.Value)
                && prevTarget!.Element is IInputElement prevInput && prevInput.Focus())
            {
                _previousFocusNavigationDirection = request.FocusNavigationDirection;
                _previousFocusedElement.SetTarget(currentTarget);
                _lastFocusedElement.SetTarget(prevTarget);

                _onFocus(prevTarget);
                return true;
            }
            else if (findNext(currentTarget, request, out var nextTarget) && nextTarget!.Element is IInputElement nextInput && nextInput.Focus())
            {
                _previousFocusNavigationDirection = request.FocusNavigationDirection;
                _previousFocusedElement.SetTarget(currentTarget);
                _lastFocusedElement.SetTarget(nextTarget);

                _onFocus(nextTarget);
                return true;
            }

            return false;
        }

        public bool TryRestoreFocus()
        {
            if (_lastFocusedElement.TryGetTarget(out var lastTarget))
            {
                if (lastTarget!.Element is IInputElement elementInput && elementInput.IsFocused)
                {
                    return true;
                }

                if (lastTarget.Element is IInputElement input && input.Focus())
                {
                    _onFocus.Invoke(lastTarget);
                    return true;
                }
            }

            return false;
        }
    }
}
