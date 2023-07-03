using System;
using System.Collections;
using System.Collections.Generic;
using PChecker.Actors;
using PChecker.Actors.Events;
using PChecker.Actors.Logging;
using PChecker.Actors.Timers;
using PChecker.Feedback.EventMatcher;

namespace PChecker.Feedback;

internal class EventPatternObserver: IActorRuntimeLog
{
    public readonly IMatcher Matcher;
    private bool _matched = false;
    public List<string> SavedEventTypes = new();

    public EventPatternObserver(IMatcher matcher)
    {
        Matcher = matcher;
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
        _matched |= Matcher.MatchOne(e);
        if (e.GetType().Name == "eBlockWorkItem")
        {
            var result = GetEventWithPayload(e);
            SavedEventTypes.Add(result["wType"]);
        }
        else if (Matcher.IsInterestingEvent(e))
        {
            SavedEventTypes.Add(e.GetType().Name);
        }
    }

    private Dictionary<string, string> GetEventWithPayload(Event e)
    {
        var method = e.GetType().GetMethod("get_Payload");
        var payload = method.Invoke(e, new object[] { });


        var fieldNames = payload.GetType().GetField("fieldNames");
        var names = new List<string>();
        foreach (var item in (IEnumerable) fieldNames.GetValue(payload))
        {
            names.Add((string) item);
        }
        var fieldValues = payload.GetType().GetField("fieldValues");
        var values = new List<string>();
        foreach (var item in (IEnumerable) fieldValues.GetValue(payload))
        {
            if (item != null)
            {
                values.Add(item.ToString());
            }
            else
            {
                values.Add("null");
            }
        }

        Dictionary<string, string> result = new();

        for (var i = 0; i < names.Count; i++)
        {
            result[names[i]] = values[i];
        }

        return result;
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

    public virtual bool IsMatched()
    {
        return _matched;
    }

    public void Reset()
    {
        _matched = false;
        SavedEventTypes = new();
        Matcher.Reset();
    }
}