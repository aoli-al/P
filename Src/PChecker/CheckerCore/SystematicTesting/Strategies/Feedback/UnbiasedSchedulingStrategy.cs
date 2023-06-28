using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using PChecker.Generator;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Traces;

namespace PChecker.SystematicTesting.Strategies.Feedback;

internal class UnbiasedSchedulingStrategy<TInput, TSchedule> : FeedbackGuidedStrategy<TInput, TSchedule>
    where TInput: IInputGenerator<TInput>
    where TSchedule: IScheduleGenerator<TSchedule>
{
    private string _prefix = "";
    private Dictionary<string, int> _coverage = new();
    private Dictionary<string, StrategyGenerator> _responsible = new();
    private Dictionary<StrategyGenerator, int> _coveredPrefix = new();
    private Dictionary<string, Dictionary<string, bool>> _happensBefore = new();
    private HashSet<string> _allOperators = new();
    private string _prevActor = "";
    private bool _shouldSaveThisScheduling = false;
    private int _maxChoices;
    private int _currentChoices;
    private Dictionary<string, string> _tree;
    private Dictionary<string, int> _weight = new();
    private Dictionary<string, int> _eventLast = new();
    private bool _updated;
    private System.Random _random = new System.Random();


    private HashSet<string> _enabledOperators = new();
    private Dictionary<string, int> _machineStep = new();


    public override bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        // _currentChoices += ops.Count();
        // var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        // if (_prevActor == "")
        // {
        //     _prevActor = enabledOperations[0].Name + 0;
        //     _machineStep[enabledOperations[0].Name] = 0;
        // }
        // else
        // {
        //     foreach (var op in enabledOperations)
        //     {
        //         _machineStep.TryAdd(op.Name, 0);
        //         var name = op.Name + _machineStep[op.Name];
        //         if (!_enabledOperators.Contains(name))
        //         {
        //             _updated |= _allOperators.Add(name);
        //             _happensBefore.TryAdd(_prevActor, new());
        //             _updated |= _happensBefore[_prevActor].TryAdd(name, true);
        //             _enabledOperators.Add(name);
        //         }
        //     }
        // }
        //
        // if (enabledOperations.Count == 0)
        // {
        //     next = null;
        //     return false;
        // }


        // if (enabledOperations.Count == 1)
        // {
        //     next = enabledOperations[0];
        // }
        // else
        // {
        //     var subTreeSizes = new List<int>();
        //     foreach (var op in enabledOperations)
        //     {
        //         var name = op.Name + _machineStep[op.Name];
        //         if (!_weight.ContainsKey(name))
        //         {
        //             var visited = new HashSet<string>();
        //             var worklist = new List<string>() { name };
        //             var levelMark = name;
        //             var nextLevelMark = "";
        //             var depth = 1;
        //             var weight = 0;
        //             while (worklist.Count != 0)
        //             {
        //                 var item = worklist.First();
        //                 worklist.RemoveAt(0);
        //                 if (visited.Contains(item))
        //                 {
        //                     continue;
        //                 }
        //                 weight += depth;
        //                 if (levelMark == name)
        //                 {
        //                     depth += 1;
        //                     levelMark = nextLevelMark;
        //                 }
        //                 visited.Add(item);
        //                 if (_happensBefore.TryGetValue(item, out var constraints))
        //                 {
        //                     foreach (var key in constraints.Keys)
        //                     {
        //                         worklist.Add(key);
        //                     }
        //
        //                     nextLevelMark = worklist.Last();
        //                 }
        //             }
        //
        //             _weight[name] = weight;
        //         }
        //         subTreeSizes.Add(_weight[name]);
        //     }
        //
        //     var total = subTreeSizes.Sum();
        //     var choice = _random.NextDouble();
        //     AsyncOperation? selected = null;
        //     for (var i = 0; i < enabledOperations.Count; i++)
        //     {
        //         var currentProb = (subTreeSizes[i] * 1.0d) / total;
        //         if (choice < currentProb)
        //         {
        //             selected = enabledOperations[i];
        //             break;
        //         }
        //         choice -= currentProb;
        //     }
        //
        //     if (selected == null)
        //     {
        //         selected = enabledOperations.Last();
        //     }
        //
        //     next = selected;
        // }

        // if (_prefix.Length != 0)
        // {
        //     if (enabledOperations.Count > _coverage.GetValueOrDefault(_prefix, 1))
        //     {
        //         _shouldSaveThisScheduling = true;
        //         _coverage[_prefix] = enabledOperations.Count;
        //         var prev = _responsible.GetValueOrDefault(_prefix, null);
        //         if (prev != null && _coveredPrefix.ContainsKey(prev))
        //         {
        //             _coveredPrefix[prev] -= 1;
        //             if (_coveredPrefix[prev] == 0)
        //             {
        //                 _coveredPrefix.Remove(prev);
        //                 SavedGenerators.Remove(prev);
        //             }
        //         }
        //         _responsible[_prefix] = Generator;
        //         _coveredPrefix.TryAdd(Generator, 0);
        //         _coveredPrefix[Generator] += 1;
        //     }
        // }
        var result = base.GetNextOperation(current, ops, out next);
        // if (result)
        // {
        if (result)
        {
            if (next is ActorOperation actor)
            {
                _machineStep.TryAdd(actor.Actor.Id.Type, 0);
                _prevActor = actor.Actor.Id.Type + _machineStep[actor.Actor.Id.Type];
                _machineStep[actor.Actor.Id.Type] += 1;
                // if (_prevActor.Contains("KFCClient"))
                // if (_prevActor.Contains("Client"))
                if (_prevActor.Contains("KFCClient0"))
                // if (_prevActor.Contains("SplitWorker") || _prevActor.Contains("CoalesceWorker") || _prevActor.Contains("KFCClient"))
                // if (true)
                {
                    _eventLast.TryAdd(_prevActor, Int32.MaxValue);
                    // _eventLast.TryAdd(_prevActor, 0);
                    if (ScheduledSteps < _eventLast[_prevActor])
                    {
                        _shouldSaveThisScheduling = true;
                        _eventLast[_prevActor] = ScheduledSteps;
                        var prev = _responsible.GetValueOrDefault(_prevActor, null);
                        if (prev != null && _coveredPrefix.ContainsKey(prev))
                        {
                            _coveredPrefix[prev] -= 1;
                            if (_coveredPrefix[prev] == 0)
                            {
                                _coveredPrefix.Remove(prev);
                                SavedGenerators.Remove(prev);
                            }
                        }
                        _responsible[_prevActor] = Generator;
                        _coveredPrefix.TryAdd(Generator, 0);
                        _coveredPrefix[Generator] += 1;
                    }
                }
            }
        }

        // }
        //
        return result;
    }

    public UnbiasedSchedulingStrategy(CheckerConfiguration checkerConfiguration, TInput input, TSchedule schedule) : base(checkerConfiguration, input, schedule)
    {
    }

    public override void ObserveRunningResults(ControlledRuntime runtime)
    {
        // if (_currentChoices > _maxChoices)
        // {
        //     _maxChoices = _currentChoices;
        //     SavedGenerators.Clear();
        //     SavedGenerators.Add(Generator);
        // }
        if (_shouldSaveThisScheduling)
        {
            SavedGenerators.Add(Generator);
            LastSavedSchedule = new(runtime.EventPatternObserver.SavedEventTypes);
        }
        // base.ObserveRunningResults(runtime);
    }

    public void dfs(string parent, HashSet<string> visited)
    {
        foreach (var op in _allOperators)
        {
            if (visited.Contains(op))
            {
                continue;
            }

            bool satisfied = true;
            if (_happensBefore.TryGetValue(op, out var constraints))
            {
                foreach (var prev in constraints.Keys)
                {
                    if (!visited.Contains(prev))
                    {
                        satisfied = false;
                    }
                }
            }
        }
    }

    public override bool PrepareForNextIteration()
    {

        if (_updated)
        {
            _weight.Clear();
        }


        _prefix = "";
        _currentChoices = 0;
        _updated = false;
        _enabledOperators = new HashSet<string>();
        _machineStep = new();
        _prevActor = "";
        ScheduledSteps = 0;
        _shouldSaveThisScheduling = false;
        return base.PrepareForNextIteration();
    }
}