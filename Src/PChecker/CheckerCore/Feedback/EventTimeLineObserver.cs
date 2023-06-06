using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Actors;
using PChecker.Actors.Events;
using PChecker.Actors.Logging;
using PChecker.Actors.Timers;
using PChecker.Exceptions;

namespace PChecker.Feedback;

public class EventTimeLineObserver: IActorRuntimeLog
{
    public record EventData(string Name, int Count)
    {
        public int Count
        {
            get;
            set;
        } = Count;

        public override string ToString()
        {
            return $"({Name}, {Count})";
        }
    };
    public Dictionary<ActorId, LinkedList<EventData>> Timeline = new();
    private Dictionary<Event, ActorId> _eventMap = new();
    private LinkedList<Event> _eventQueue = new();
    private readonly int _numHashes = 10;
    private List<long> _coeffA = new();
    private List<long> _coeffB = new();
    private System.Random _random = new(0);
    private readonly long _prime = 4294967311;

    public EventTimeLineObserver()
    {
        for (int i = 0; i < _numHashes; i++)
        {
            _coeffA.Add(_random.Next(int.MaxValue));
            _coeffB.Add(_random.Next(int.MaxValue));
        }
    }
    public void OnCreateActor(ActorId id, string creatorName, string creatorType)
    {
    }

    public void OnCreateStateMachine(ActorId id, string creatorName, string creatorType)
    {
    }

    public void OnExecuteAction(ActorId id, string handlingStateName, string currentStateName, string actionName)
    {
    }

    public void OnSendEvent(ActorId targetActorId, string senderName, string senderType, string senderStateName, Event e,
        Guid opGroupId, bool isTargetHalted)
    {
    }

    public void OnRaiseEvent(ActorId id, string stateName, Event e)
    {
    }

    public void OnEnqueueEvent(ActorId id, Event e)
    {
    }

    public void OnDequeueEvent(ActorId id, string stateName, Event e)
    {
        Timeline.TryAdd(id, new());
        if (_eventQueue.Count > 15)
        {
            var first = _eventQueue.First!.Value;
            if (Timeline[_eventMap[first]].First.Value.Count == 1)
            {
                Timeline[_eventMap[first]].RemoveFirst();
            }
            else
            {

                Timeline[_eventMap[first]].First.Value.Count -= 1;
            }

            _eventQueue.RemoveFirst();
            _eventMap.Remove(first);
        }

        if (Timeline[id].Last?.Value.Name == e.GetType().Name)
        {
            Timeline[id].Last.Value.Count += 1;
        }
        else
        {
            Timeline[id].AddLast(new EventData(e.GetType().Name, 1));
        }
        _eventQueue.AddLast(e);
        _eventMap[e] = id;
    }

    public void OnReceiveEvent(ActorId id, string stateName, Event e, bool wasBlocked)
    {
    }

    public void OnWaitEvent(ActorId id, string stateName, Type eventType)
    {
    }

    public void OnWaitEvent(ActorId id, string stateName, params Type[] eventTypes)
    {
    }

    public void OnStateTransition(ActorId id, string stateName, bool isEntry)
    {
    }

    public void OnGotoState(ActorId id, string currentStateName, string newStateName)
    {
    }

    public void OnPushState(ActorId id, string currentStateName, string newStateName)
    {
    }

    public void OnPopState(ActorId id, string currentStateName, string restoredStateName)
    {
    }

    public void OnDefaultEventHandler(ActorId id, string stateName)
    {
    }

    public void OnHalt(ActorId id, int inboxSize)
    {
    }

    public void OnHandleRaisedEvent(ActorId id, string stateName, Event e)
    {
    }

    public void OnPopStateUnhandledEvent(ActorId id, string stateName, Event e)
    {
    }

    public void OnExceptionThrown(ActorId id, string stateName, string actionName, Exception ex)
    {
    }

    public void OnExceptionHandled(ActorId id, string stateName, string actionName, Exception ex)
    {
    }

    public void OnCreateTimer(TimerInfo info)
    {
    }

    public void OnStopTimer(TimerInfo info)
    {
    }

    public void OnCreateMonitor(string monitorType)
    {
    }

    public void OnMonitorExecuteAction(string monitorType, string stateName, string actionName)
    {
    }

    public void OnMonitorProcessEvent(string monitorType, string stateName, string senderName, string senderType,
        string senderStateName, Event e)
    {
    }

    public void OnMonitorRaiseEvent(string monitorType, string stateName, Event e)
    {
    }

    public void OnMonitorStateTransition(string monitorType, string stateName, bool isEntry, bool? isInHotState)
    {
    }

    public void OnMonitorError(string monitorType, string stateName, bool? isInHotState)
    {
    }

    public void OnRandom(object result, string callerName, string callerType)
    {
    }

    public void OnAssertionFailure(string error)
    {
    }

    public void OnStrategyDescription(string strategyName, string description)
    {
    }

    public void OnCompleted()
    {
    }

    public int GetCurrentTimeline()
    {
        var timelines = Timeline.Select(it => (it.Key.Type + ":" + string.Join(",", it.Value))).ToList();
        timelines.Sort();
        return string.Join(";", timelines).GetHashCode();
    }

    public string GetCurrentTimelineString()
    {
        var timelines = Timeline.Select(it => (it.Key.Type + ":" + string.Join(",", it.Value))).ToList();
        timelines.Sort();
        return string.Join(";", timelines);
    }

}