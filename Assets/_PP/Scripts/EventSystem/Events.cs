using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DeltaReality.Utils
{
    /// <summary>
    /// Main class that handles all events in project
    /// </summary>
    public partial class Events : MonoBehaviour
    {
        #region regular events (with parameters)
        public delegate void EventDelegate<T>(T e) where T : BaseEvent;
        private delegate void EventDelegate(BaseEvent e);

        private static Dictionary<Category, Dictionary<Type, EventDelegate>> _delegates = new Dictionary<Category, Dictionary<Type, EventDelegate>>();
        private static Dictionary<Category, Dictionary<Delegate, EventDelegate>> _delegateLookup = new Dictionary<Category, Dictionary<Delegate, EventDelegate>>();

        private static ConcurrentQueue<BaseEvent> _concurrentQueue = new ConcurrentQueue<BaseEvent>();
        #endregion

        #region simple events (no parameters)
        public delegate void SimpleEventDelegate();

        private static Dictionary<Simple, SimpleEventDelegate> _simpleDelegates = new Dictionary<Simple, SimpleEventDelegate>();
        private static Dictionary<Delegate, SimpleEventDelegate> _simpleDelegateLookup = new Dictionary<Delegate, SimpleEventDelegate>();

        private static ConcurrentQueue<Simple> _simpleConcurrentQueue = new ConcurrentQueue<Simple>();
        #endregion

        private static Events _instance;
        private static Thread _unityThread;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _instance = new GameObject(nameof(Events)).AddComponent<Events>();
            DontDestroyOnLoad(_instance);
            _unityThread = Thread.CurrentThread;
            _instance.StartCoroutine(RaiseThreaded());
        }

        private static IEnumerator RaiseThreaded()
        {
            while (Application.isPlaying)
            {
                while (_concurrentQueue.TryDequeue(out BaseEvent _event))
                {
                    Invoke(_event);
                }
                while (_simpleConcurrentQueue.TryDequeue(out Simple _event))
                {
                    Invoke(_event);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        #region regular event handling (events with parameters)
        public static void Raise(BaseEvent baseEvent)
        {
            if (_unityThread.Equals(Thread.CurrentThread))
            {
                Invoke(baseEvent);
            }

            else
            {
                _concurrentQueue.Enqueue(baseEvent);
            }
        }

        private static void Invoke(BaseEvent baseEvent)
        {
            if (!_delegates.ContainsKey(baseEvent.Category))
                return;
            if (_delegates[baseEvent.Category].TryGetValue(baseEvent.GetType(), out EventDelegate del))
            {
                del.Invoke(baseEvent);
            }
        }

        public static void AddListener<T>(EventDelegate<T> del) where T : BaseEvent
        {
            #if DeltaReality_CUSTOM_EVENT_CATEGORIES
            Category eventCategory = Activator.CreateInstance<T>().Category;
            #else
            Category eventCategory = Category.Default;
            #endif

            if (!_delegates.ContainsKey(eventCategory))
            {
                _delegates.Add(eventCategory, new Dictionary<Type, EventDelegate>());
            }

            if (!_delegateLookup.ContainsKey(eventCategory))
            {
                _delegateLookup.Add(eventCategory, new Dictionary<Delegate, EventDelegate>());
            }

            // Early-out if we've already registered this delegate
            if (_delegateLookup[eventCategory].ContainsKey(del))
            {
                Debug.Log($"Trying to add duplicate delegate{del}");
                return;
            }
            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            void internalDelegate(BaseEvent baseEvent) => del((T)baseEvent);
            _delegateLookup[eventCategory][del] = internalDelegate;

            if (_delegates[eventCategory].TryGetValue(typeof(T), out EventDelegate temporaryDelegate))
            {
                _delegates[eventCategory][typeof(T)] = temporaryDelegate += internalDelegate;
            }
            else
            {
                _delegates[eventCategory][typeof(T)] = internalDelegate;
            }
        }

        public static void RemoveListener<T>(EventDelegate<T> del) where T : BaseEvent
        {
            #if DeltaReality_CUSTOM_EVENT_CATEGORIES
            Category eventCategory = Activator.CreateInstance<T>().Category;
            #else
            Category eventCategory = Category.Default;
            #endif

            if (!_delegateLookup.ContainsKey(eventCategory) || !_delegates.ContainsKey(eventCategory))
                return;
            if (_delegateLookup[eventCategory].TryGetValue(del, out EventDelegate internalDelegate))
            {
                if (_delegates[eventCategory].TryGetValue(typeof(T), out EventDelegate temporaryDelegate))
                {
                    temporaryDelegate -= internalDelegate;
                    if (temporaryDelegate == null)
                    {
                        _delegates[eventCategory].Remove(typeof(T));
                    }
                    else
                    {
                        _delegates[eventCategory][typeof(T)] = temporaryDelegate;
                    }
                }
                _delegateLookup[eventCategory].Remove(del);
            }
        }
        #endregion

        #region simple event handling (no parameters)
        public static void Raise(Simple _event)
        {
            if (_unityThread.Equals(Thread.CurrentThread))
            {
                Invoke(_event);
            }

            else
            {
                _simpleConcurrentQueue.Enqueue(_event);
            }
        }

        private static void Invoke(Simple _event)
        {
            if (_simpleDelegates.TryGetValue(_event, out SimpleEventDelegate del))
            {
                del.Invoke();
            }
        }

        public static void AddListener(Simple eventType, SimpleEventDelegate del)
        {
            // Early-out if we've already registered this delegate
            if (_simpleDelegateLookup.ContainsKey(del))
            {
                Debug.Log($"Trying to add duplicate delegate{del}");
                return;
            }
            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            void internalDelegate() => del();
            _simpleDelegateLookup[del] = internalDelegate;

            if (_simpleDelegates.TryGetValue(eventType, out SimpleEventDelegate temporaryDelegate))
            {
                _simpleDelegates[eventType] = temporaryDelegate += internalDelegate;
            }
            else
            {
                _simpleDelegates[eventType] = internalDelegate;
            }
        }

        public static void RemoveListener(Simple eventType, SimpleEventDelegate del)
        {
            if (_simpleDelegateLookup.TryGetValue(del, out SimpleEventDelegate internalDelegate))
            {
                if (_simpleDelegates.TryGetValue(eventType, out SimpleEventDelegate temporaryDelegate))
                {
                    temporaryDelegate -= internalDelegate;
                    if (temporaryDelegate == null)
                    {
                        _simpleDelegates.Remove(eventType);
                    }
                    else
                    {
                        _simpleDelegates[eventType] = temporaryDelegate;
                    }
                }
                _simpleDelegateLookup.Remove(del);
            }
        }
        #endregion
    }
}