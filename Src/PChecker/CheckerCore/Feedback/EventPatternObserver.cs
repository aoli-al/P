using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using PChecker.Actors;
using PChecker.Actors.Events;
using PChecker.Actors.Logging;
using PChecker.Actors.Timers;
using PChecker.Feedback.EventMatcher;
using PChecker.SystematicTesting;

namespace PChecker.Feedback;

internal class EventPatternObserver: IActorRuntimeLog
{
    private LinkedList<string> _eventQueue = new();
    private HashSet<string> _interestingEvents = new() { "eBlockWorkItem" };
    public readonly Nfa Matcher;
    private bool _matched = false;
    private ControlledRuntime _runtime;

    public EventPatternObserver(Nfa matcher, ControlledRuntime runtime)
    {
        Matcher = matcher;
        _runtime = runtime;
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
        if (Matcher.CurrentStates.Count == 0)
        {
            _runtime.Stop();
        }
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

    public bool IsMatched()
    {
        return _matched;
    }
}