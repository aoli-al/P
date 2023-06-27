using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Random;
using PChecker.SystematicTesting.Operations;
using PChecker.SystematicTesting.Strategies.Probabilistic;

namespace PChecker.SystematicTesting.Strategies.Feedback;

internal class UnbiasedSchedulingQLearning : RandomStrategy
{
    /// <summary>
    /// Map from program states to a map from next operations to their quality values.
    /// </summary>
    private readonly Dictionary<int, Dictionary<ulong, double>> OperationQTable;

    /// <summary>
    /// The path that is being executed during the current iteration. Each
    /// step of the execution is represented by an operation and a value
    /// represented the program state after the operation executed.
    /// </summary>
    private readonly LinkedList<(ulong op, AsyncOperationType type, int state, int enabledStates)> ExecutionPath;

    /// <summary>
    /// The previously chosen operation.
    /// </summary>
    private ulong PreviousOperation;

    /// <summary>
    /// The value of the learning rate.
    /// </summary>
    private readonly double LearningRate;

    /// <summary>
    /// The value of the discount factor.
    /// </summary>
    private readonly double Gamma;

    /// <summary>
    /// The op value denoting a true boolean choice.
    /// </summary>
    private readonly ulong TrueChoiceOpValue;

    /// <summary>
    /// The op value denoting a false boolean choice.
    /// </summary>
    private readonly ulong FalseChoiceOpValue;

    /// <summary>
    /// The op value denoting the min integer choice.
    /// </summary>
    private readonly ulong MinIntegerChoiceOpValue;

    /// <summary>
    /// The failure injection reward.
    /// </summary>
    private readonly double FailureInjectionReward;

    /// <summary>
    /// The basic action reward.
    /// </summary>
    private readonly double BasicActionReward;

    /// <summary>
    /// The number of explored executions.
    /// </summary>
    private int Epochs;

    private System.Random _random = new();

    private HashSet<string> _enabledOperators = new();
    private Dictionary<string, int> _machineStep = new();


    /// <summary>
    /// Initializes a new instance of the <see cref="QLearningStrategy"/> class.
    /// It uses the specified random number generator.
    /// </summary>
    public UnbiasedSchedulingQLearning(int maxSteps, IRandomValueGenerator random)
        : base(maxSteps, random)
    {
        this.OperationQTable = new Dictionary<int, Dictionary<ulong, double>>();
        this.ExecutionPath = new LinkedList<(ulong, AsyncOperationType, int, int)>();
        this.PreviousOperation = 0;
        this.LearningRate = 0.3;
        this.Gamma = 0.7;
        this.TrueChoiceOpValue = ulong.MaxValue;
        this.FalseChoiceOpValue = ulong.MaxValue - 1;
        this.MinIntegerChoiceOpValue = ulong.MaxValue - 2;
        this.FailureInjectionReward = -1000;
        this.BasicActionReward = -1;
        this.Epochs = 0;
    }

    /// <inheritdoc/>
    public override bool GetNextOperation(AsyncOperation current, IEnumerable<AsyncOperation> ops, out AsyncOperation next)
    {
        var enabledOperations = ops.Where(op => op.Status is AsyncOperationStatus.Enabled).ToList();
        if (!ops.Any(op => op.Status is AsyncOperationStatus.Enabled))
        {
            // Fail fast if there are no enabled operations.
            next = null;
            return false;
        }

        int newEnabled = 1;
        foreach (var op in enabledOperations)
        {
            _machineStep.TryAdd(op.Name, 0);
            var name = op.Name + _machineStep[op.Name];
            if (!_enabledOperators.Contains(name))
            {
                _enabledOperators.Add(name);
                newEnabled += 1;
            }
        }



        int state = this.CaptureExecutionStep(current, newEnabled);
        this.InitializeOperationQValues(state, ops);

        next = this.GetNextOperationByPolicy(state, ops);
        _machineStep[next.Name] += 1;
        this.PreviousOperation = next.Id;

        this.ScheduledSteps++;
        return true;
    }

    /// <inheritdoc/>
    public override bool GetNextBooleanChoice(AsyncOperation current, int maxValue, out bool next)
    {
        next = _random.Next() > 0;
        return true;
        // int state = this.CaptureExecutionStep(current);
        // this.InitializeBooleanChoiceQValues(state);
        //
        // next = this.GetNextBooleanChoiceByPolicy(state);
        //
        // this.PreviousOperation = next ? this.TrueChoiceOpValue : this.FalseChoiceOpValue;
        // this.ScheduledSteps++;
        // return true;
    }

    /// <inheritdoc/>
    public override bool GetNextIntegerChoice(AsyncOperation current, int maxValue, out int next)
    {
        next = _random.Next(maxValue);
        return true;
    }

    /// <inheritdoc/>
    public override bool PrepareForNextIteration()
    {
        this.LearnQValues();
        this.ExecutionPath.Clear();
        this.PreviousOperation = 0;
        _enabledOperators.Clear();
        this.Epochs++;

        return base.PrepareForNextIteration();
    }

    /// <inheritdoc/>
    public override string GetDescription() => $"RL[seed '{RandomValueGenerator.Seed}']";

    /// <summary>
    /// Returns the next operation to schedule by drawing from the probability
    /// distribution over the specified state and enabled operations.
    /// </summary>
    private AsyncOperation GetNextOperationByPolicy(int state, IEnumerable<AsyncOperation> ops)
    {
        var opIds = new List<ulong>();
        var qValues = new List<double>();
        foreach (var pair in this.OperationQTable[state])
        {
            // Consider only the Q values of enabled operations.
            if (ops.Any(op => op.Id == pair.Key && op.Status == AsyncOperationStatus.Enabled))
            {
                opIds.Add(pair.Key);
                qValues.Add(pair.Value);
            }
        }

        int idx = this.ChooseQValueIndexFromDistribution(qValues);
        return ops.FirstOrDefault(op => op.Id == opIds[idx]);
    }

    /// <summary>
    /// Returns the next boolean choice by drawing from the probability
    /// distribution over the specified state and boolean choices.
    /// </summary>
    private bool GetNextBooleanChoiceByPolicy(int state)
    {
        double trueQValue = this.OperationQTable[state][this.TrueChoiceOpValue];
        double falseQValue = this.OperationQTable[state][this.FalseChoiceOpValue];

        var qValues = new List<double>(2)
        {
            trueQValue,
            falseQValue
        };

        int idx = this.ChooseQValueIndexFromDistribution(qValues);
        return idx == 0 ? true : false;
    }

    /// <summary>
    /// Returns the next integer choice by drawing from the probability
    /// distribution over the specified state and integer choices.
    /// </summary>
    private int GetNextIntegerChoiceByPolicy(int state, int maxValue)
    {
        var qValues = new List<double>(maxValue);
        for (ulong i = 0; i < (ulong)maxValue; i++)
        {
            qValues.Add(this.OperationQTable[state][this.MinIntegerChoiceOpValue - i]);
        }

        return this.ChooseQValueIndexFromDistribution(qValues);
    }

    /// <summary>
    /// Returns an index of a Q value by drawing from the probability distribution
    /// over the specified Q values.
    /// </summary>
    private int ChooseQValueIndexFromDistribution(List<double> qValues)
    {
        double sum = 0;
        for (int i = 0; i < qValues.Count; i++)
        {
            qValues[i] = Math.Exp(qValues[i]);
            sum += qValues[i];
        }

        for (int i = 0; i < qValues.Count; i++)
        {
            qValues[i] /= sum;
        }

        sum = 0;

        // First, change the shape of the distribution probability array to be cumulative.
        // For example, instead of [0.1, 0.2, 0.3, 0.4], we get [0.1, 0.3, 0.6, 1.0].
        var cumulative = qValues.Select(c =>
        {
            var result = c + sum;
            sum += c;
            return result;
        }).ToList();

        // Generate a random double value between 0.0 to 1.0.
        var rvalue = this.RandomValueGenerator.NextDouble();

        // Find the first index in the cumulative array that is greater
        // or equal than the generated random value.
        var idx = cumulative.BinarySearch(rvalue);

        if (idx < 0)
        {
            // If an exact match is not found, List.BinarySearch will return the index
            // of the first items greater than the passed value, but in a specific form
            // (negative) we need to apply ~ to this negative value to get real index.
            idx = ~idx;
        }

        if (idx > cumulative.Count - 1)
        {
            // Very rare case when probabilities do not sum to 1 because of
            // double precision issues (so sum is 0.999943 and so on).
            idx = cumulative.Count - 1;
        }

        return idx;
    }

    /// <summary>
    /// Captures metadata related to the current execution step, and returns
    /// a value representing the current program state.
    /// </summary>
    private int CaptureExecutionStep(AsyncOperation current, int numEnabled)
    {
        int state = current.HashedProgramState;

        // Update the execution path with the current state.
        this.ExecutionPath.AddLast((this.PreviousOperation, current.Type, state, numEnabled));

        return state;
    }

    /// <summary>
    /// Initializes the Q values of all enabled operations that can be chosen
    /// at the specified state that have not been previously encountered.
    /// </summary>
    private void InitializeOperationQValues(int state, IEnumerable<AsyncOperation> ops)
    {
        if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
        {
            qValues = new Dictionary<ulong, double>();
            this.OperationQTable.Add(state, qValues);
        }

        foreach (var op in ops)
        {
            // Assign the same initial probability for all new enabled operations.
            if (op.Status == AsyncOperationStatus.Enabled && !qValues.ContainsKey(op.Id))
            {
                qValues.Add(op.Id, 0);
            }
        }
    }

    /// <summary>
    /// Initializes the Q values of all boolean choices that can be chosen
    /// at the specified state that have not been previously encountered.
    /// </summary>
    private void InitializeBooleanChoiceQValues(int state)
    {
        if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
        {
            qValues = new Dictionary<ulong, double>();
            this.OperationQTable.Add(state, qValues);
        }

        if (!qValues.ContainsKey(this.TrueChoiceOpValue))
        {
            qValues.Add(this.TrueChoiceOpValue, 0);
        }

        if (!qValues.ContainsKey(this.FalseChoiceOpValue))
        {
            qValues.Add(this.FalseChoiceOpValue, 0);
        }
    }

    /// <summary>
    /// Initializes the Q values of all integer choices that can be chosen
    /// at the specified state that have not been previously encountered.
    /// </summary>
    private void InitializeIntegerChoiceQValues(int state, int maxValue)
    {
        if (!this.OperationQTable.TryGetValue(state, out Dictionary<ulong, double> qValues))
        {
            qValues = new Dictionary<ulong, double>();
            this.OperationQTable.Add(state, qValues);
        }

        for (ulong i = 0; i < (ulong)maxValue; i++)
        {
            ulong opValue = this.MinIntegerChoiceOpValue - i;
            if (!qValues.ContainsKey(opValue))
            {
                qValues.Add(opValue, 0);
            }
        }
    }

    /// <summary>
    /// Learn Q values using data from the current execution.
    /// </summary>
    private void LearnQValues()
    {
        var pathBuilder = new System.Text.StringBuilder();

        int idx = 0;
        var node = this.ExecutionPath.First;
        while (node != null && node.Next != null)
        {
            pathBuilder.Append($"{node.Value.op},");

            var (_, _, state, _) = node.Value;
            var (nextOp, nextType, nextState, nextEnabled) = node.Next.Value;

            // Compute the max Q value.
            double maxQ = double.MinValue;
            foreach (var nextOpQValuePair in this.OperationQTable[nextState])
            {
                if (nextOpQValuePair.Value > maxQ)
                {
                    maxQ = nextOpQValuePair.Value;
                }
            }

            // Compute the reward. Program states that are visited with higher frequency result into lesser rewards.
            double reward = (nextType == AsyncOperationType.InjectFailure ?
                this.FailureInjectionReward : this.BasicActionReward) / nextEnabled;
            if (reward > 0)
            {
                // The reward has underflowed.
                reward = double.MinValue;
            }

            // Get the operations that are available from the current execution step.
            var currOpQValues = this.OperationQTable[state];
            if (!currOpQValues.ContainsKey(nextOp))
            {
                currOpQValues.Add(nextOp, 0);
            }

            // Update the Q value of the next operation.
            // Q = [(1-a) * Q]  +  [a * (rt + (g * maxQ))]
            currOpQValues[nextOp] = ((1 - this.LearningRate) * currOpQValues[nextOp]) +
                                    (this.LearningRate * (reward + (this.Gamma * maxQ)));

            node = node.Next;
            idx++;
        }
    }

}