using System;
using System.Collections.Generic;

namespace Nodify.UndoRedo
{
    public interface IAction
    {
        string? Label { get; }
        void Execute();
        void Undo();
    }

    public interface IActionsHistory
    {
        bool IsEnabled { get; set; }
        bool CanUndo { get; }
        bool CanRedo { get; }
        IAction? Current { get; }

        void ExecuteAction(IAction action);
        void Record(Action apply, Action unapply, string? label = null);
        void Record(IAction action);
        void Undo();
        void Redo();
        IDisposable Batch(string? label = null);
        void Clear();
    }

    public sealed class ActionsHistory : IActionsHistory
    {
        private readonly Stack<IAction> _undo = new Stack<IAction>();
        private readonly Stack<IAction> _redo = new Stack<IAction>();
        private readonly Stack<List<IAction>> _batchStack = new Stack<List<IAction>>();

        public static ActionsHistory Global { get; } = new ActionsHistory();

        public bool IsEnabled { get; set; } = true;

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;
        public IAction? Current => _undo.Count > 0 ? _undo.Peek() : null;

        public void ExecuteAction(IAction action)
        {
            if (!IsEnabled)
            {
                action.Execute();
                return;
            }

            action.Execute();
            Record(action);
        }

        public void Record(Action apply, Action unapply, string? label = null)
        {
            Record(new DelegateAction(apply, unapply, label));
        }

        public void Record(IAction action)
        {
            if (!IsEnabled)
            {
                return;
            }

            if (_batchStack.Count > 0)
            {
                _batchStack.Peek().Add(action);
                return;
            }

            _undo.Push(action);
            _redo.Clear();
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            var action = _undo.Pop();
            action.Undo();
            _redo.Push(action);
        }

        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            var action = _redo.Pop();
            action.Execute();
            _undo.Push(action);
        }

        public IDisposable Batch(string? label = null)
        {
            var batch = new List<IAction>();
            _batchStack.Push(batch);
            return new BatchScope(this, label, batch);
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }

        private void EndBatch(string? label, List<IAction> actions)
        {
            _batchStack.Pop();
            if (actions.Count == 0)
            {
                return;
            }

            Record(new BatchAction(label, actions));
        }

        private sealed class BatchScope : IDisposable
        {
            private readonly ActionsHistory _history;
            private readonly string? _label;
            private readonly List<IAction> _actions;
            private bool _disposed;

            public BatchScope(ActionsHistory history, string? label, List<IAction> actions)
            {
                _history = history;
                _label = label;
                _actions = actions;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _history.EndBatch(_label, _actions);
            }
        }
    }
}
