using System;
using System.Collections.Generic;
using UnityEngine;

namespace Reclaim.Core
{
    /// <summary>
    /// Stores reversible actions for simple undo support.
    /// </summary>
    public class PlacementHistory : MonoBehaviour
    {
        private readonly Stack<Action> _undoStack = new Stack<Action>();

        public event Action<int> OnHistorySizeChanged;

        public int Count => _undoStack.Count;

        public void Record(Action undoAction)
        {
            if (undoAction == null)
            {
                return;
            }

            _undoStack.Push(undoAction);
            OnHistorySizeChanged?.Invoke(_undoStack.Count);
        }

        public void UndoLast()
        {
            if (_undoStack.Count == 0)
            {
                return;
            }

            Action action = _undoStack.Pop();
            action?.Invoke();
            OnHistorySizeChanged?.Invoke(_undoStack.Count);
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            OnHistorySizeChanged?.Invoke(_undoStack.Count);
        }
    }
}
