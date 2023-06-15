using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Antlr4.Runtime.Atn;
using PChecker.Actors.Events;

namespace PChecker.Feedback.EventMatcher;

public class Nfa
{
    private int _initState;
    public int InitState
    {
        set
        {
            _initState = value;
            CurrentStates = new HashSet<int>() { value };
        }
        get
        {
            return _initState;
        }

    }
    public int FinalState;
    private int Size;
    // Inputs this NFA responds to
    public List<EventNode> Inputs;
    public HashSet<int> VisistedStates = new();

    public HashSet<String> InterestingEvents
    {
        get
        {
            return Inputs.Select(it => it.EventName).ToHashSet();
        }
    }
    public Dictionary<int, Dictionary<int, EventNode>> Transitions;
    public HashSet<int> CurrentStates = new();

    public Nfa(Nfa nfa)
    {
        InitState = nfa.InitState;
        FinalState = nfa.FinalState;
        Size = nfa.Size;
        Inputs = nfa.Inputs;
        Transitions = nfa.Transitions;
    }

    /// <summary>
    /// Constructed with the NFA size (amount of states), the initial state and the
    /// final state
    /// </summary>
    /// <param name="size_">Amount of states.</param>
    /// <param name="initState">Initial state.</param>
    /// <param name="finalState">Final state.</param>
    public Nfa(int size, int initState, int finalState)
    {
        InitState = initState;
        FinalState = finalState;
        Size = size;

        IsLegalState(InitState);
        IsLegalState(FinalState);

        Inputs = new();

        // Initializes transTable with an "empty graph", no transitions between its
        // states
        Transitions = new();
    }

    public bool IsLegalState(int s)
    {
        // We have 'size' states, numbered 0 to size-1
        if(s < 0 || s >= Size)
            return false;

        return true;
    }

    /// <summary>
    /// Adds a transition between two states.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="in"></param>
    public void AddTrans(int from, int to, EventNode @in)
    {
        IsLegalState(from);
        IsLegalState(to);

        if (!Transitions.ContainsKey(from))
        {
            Transitions[from] = new Dictionary<int, EventNode>();
        }

        Transitions[from][to] = @in;

        if(@in != EventNode.Epsilon)
            Inputs.Add(@in);
    }

    public void AppendEmptyState()
    {
        Size += 1;
    }

    public bool MatchOne(Event e)
    {
        if (!InterestingEvents.Contains(e.GetType().Name))
        {
            return false;
        }
        var endStates = new HashSet<int>();
        foreach (var state in CurrentStates)
        {
            if (Transitions.TryGetValue(state, out var transition))
            {
                foreach (var keyValuePair in transition)
                {
                    if (keyValuePair.Value == EventNode.Epsilon)
                    {
                        continue;
                    }

                    if (keyValuePair.Value.Match(e))
                    {
                        endStates.Add(keyValuePair.Key);
                        endStates.UnionWith(EpsilonClosure(new HashSet<int>() {keyValuePair.Key}));
                    }
                }
            }
        }
        CurrentStates = endStates;
        VisistedStates.UnionWith(endStates);
        if (endStates.Contains(FinalState))
        {
            return true;
        }
        return false;
    }

    public HashSet<int> EpsilonClosure(HashSet<int> states)
    {
        // Push all states onto a stack
        Stack<int> uncheckedStack = new(states);

        // Initialize EpsilonClosure(states) to states
        HashSet<int> epsilonClosure = states;

        while(uncheckedStack.Count != 0)
        {
            // Pop state t, the top element, off the stack
            int t = uncheckedStack.Pop();

            int i = 0;

            if (Transitions.TryGetValue(t, out var transition))
            {
                foreach (var keyValuePair in transition)
                {
                    if (keyValuePair.Value == EventNode.Epsilon)
                    {
                        if (!epsilonClosure.Contains(keyValuePair.Key))
                        {
                            epsilonClosure.Add(keyValuePair.Key);
                            uncheckedStack.Push(keyValuePair.Key);
                        }
                    }
                }
            }
        }

        return epsilonClosure;
    }




    /// <summary>
    /// Fills states 0 up to other.size with other's states.
    /// </summary>
    /// <param name="other"></param>
    public void FillStates(Nfa other)
    {
        foreach (var keyValuePair in other.Transitions)
        {
            if (!Transitions.ContainsKey(keyValuePair.Key))
            {
                Transitions[keyValuePair.Key] = new();
            }

            foreach (var eventNode in keyValuePair.Value)
            {
                Transitions[keyValuePair.Key][eventNode.Key] = eventNode.Value;
            }
        }

        foreach (var otherInput in other.Inputs)
        {
            Inputs.Add(otherInput);
        }
    }

    /// <summary>
    /// Renames all the NFA's states. For each nfa state: number += shift.
    /// Functionally, this doesn't affect the NFA, it only makes it larger and renames
    /// its states.
    /// </summary>
    /// <param name="shift"></param>
    public void ShiftStates(int shift)
    {
        int newSize = Size + shift;

        if(shift < 1)
            return;

        var newTransitions = new Dictionary<int, Dictionary<int, EventNode>>();


        foreach (var keyValuePair in Transitions)
        {
            newTransitions[keyValuePair.Key + shift] = new();
            foreach (var eventNode in keyValuePair.Value)
            {
                newTransitions[keyValuePair.Key + shift][eventNode.Key + shift] = eventNode.Value;
            }
        }
        // Updates the NFA members.
        Size = newSize;
        InitState += shift;
        FinalState += shift;
        Transitions = newTransitions;
    }

    /// <summary>
    /// Returns a set of NFA states from which there is a transition on input symbol
    /// inp from some state s in states.
    /// </summary>
    /// <param name="states"></param>
    /// <param name="inp"></param>
    /// <returns></returns>
    public HashSet<int> Move(HashSet<int> states, EventNode inp)
    {
        HashSet<int> result = new();

        // For each state in the set of states
        foreach(int state in states)
        {
            if (Transitions.TryGetValue(state, out var transition))
            {
                foreach (var keyValuePair in transition)
                {
                    if (keyValuePair.Value == inp)
                    {
                        result.Add(keyValuePair.Key);
                    }
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Prints out the NFA.
    /// </summary>
    public void Show()
    {
        Console.WriteLine("This NFA has {0} states: 0 - {1}", Size, Size - 1);
        Console.WriteLine("The initial state is {0}", InitState);
        Console.WriteLine("The final state is {0}\n", FinalState);

        foreach (var keyValuePair in Transitions)
        {
            foreach (var eventNode in keyValuePair.Value)
            {
                Console.Write("Transition from {0} to {1} on input ", keyValuePair.Key, eventNode.Key);

                if(eventNode.Value == EventNode.Epsilon)
                    Console.Write("Epsilon\n");
                else
                    Console.Write("{0}\n", eventNode.Value);
            }
        }
        Console.Write("\n\n");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tree"></param>
    /// <returns></returns>
    public static Nfa TreeToNFA(BaseNode tree)
    {
        switch (tree)
        {
            case EventNode e:
                return BuildNFABasic(e);
            case BinaryExprNode e:
                if (e.Operator == BinaryExprNode.Op.ALTERNATION)
                {
                    return BuildNFAAlter(TreeToNFA(e.Left), TreeToNFA(e.Right));
                }
                return BuildNFAConcat(TreeToNFA(e.Left), TreeToNFA(e.Right));
            case UnaryExprNode e:
                if (e.Operator == UnaryExprNode.Op.STAR)
                {

                    return BuildNFAStar(TreeToNFA(e.Expression));
                }
                return BuildNFAAlter(TreeToNFA(e.Expression), BuildNFABasic(EventNode.Epsilon));
            default:
                return null;
        }
    }

    /////////////////////////////////////////////////////////////////
    //
    // NFA building functions
    //
    // Using Thompson Construction, build NFAs from basic inputs or
    // compositions of other NFAs.
    //

    /// <summary>
    /// Builds a basic, single input NFA
    /// </summary>
    /// <param name="in"></param>
    /// <returns></returns>
    public static Nfa BuildNFABasic(EventNode @in)
    {
        Nfa basic = new Nfa(2, 0, 1);

        basic.AddTrans(0, 1, @in);

        return basic;
    }

    /// <summary>
    /// Builds an alternation of nfa1 and nfa2 (nfa1|nfa2)
    /// </summary>
    /// <param name="nfa1"></param>
    /// <param name="nfa2"></param>
    /// <returns></returns>
    public static Nfa BuildNFAAlter(Nfa nfa1, Nfa nfa2)
    {
        // How this is done: the new nfa must contain all the states in
        // nfa1 and nfa2, plus a new initial and final states.
        // First will come the new initial state, then nfa1's states, then
        // nfa2's states, then the new final state

        // make room for the new initial state
        nfa1.ShiftStates(1);

        // make room for nfa1
        nfa2.ShiftStates(nfa1.Size);

        // create a new nfa and initialize it with (the shifted) nfa2
        Nfa newNFA = new Nfa(nfa2);

        // nfa1's states take their places in new_nfa
        newNFA.FillStates(nfa1);

        // Set new initial state and the transitions from it
        newNFA.AddTrans(0, nfa1.InitState, EventNode.Epsilon);
        newNFA.AddTrans(0, nfa2.InitState, EventNode.Epsilon);

        newNFA.InitState = 0;

        // Make up space for the new final state
        newNFA.AppendEmptyState();

        // Set new final state
        newNFA.FinalState = newNFA.Size - 1;

        newNFA.AddTrans(nfa1.FinalState, newNFA.FinalState, EventNode.Epsilon);
        newNFA.AddTrans(nfa2.FinalState, newNFA.FinalState, EventNode.Epsilon);

        return newNFA;
    }

    /// <summary>
    /// Builds an alternation of nfa1 and nfa2 (nfa1|nfa2)
    /// </summary>
    /// <param name="nfa1"></param>
    /// <param name="nfa2"></param>
    /// <returns></returns>
    public static Nfa BuildNFAConcat(Nfa nfa1, Nfa nfa2)
    {
        // How this is done: First will come nfa1, then nfa2 (its initial state replaced
        // with nfa1's final state)
        nfa2.ShiftStates(nfa1.Size - 1);

        // Creates a new NFA and initialize it with (the shifted) nfa2
        Nfa newNFA = new Nfa(nfa2);

        // nfa1's states take their places in newNFA
        // note: nfa1's final state overwrites nfa2's initial state,
        // thus we get the desired merge automagically (the transition
        // from nfa2's initial state now transits from nfa1's final state)
        newNFA.FillStates(nfa1);

        // Sets the new initial state (the final state stays nfa2's final state,
        // and was already copied)
        newNFA.InitState = nfa1.InitState;

        return newNFA;
    }

    /// <summary>
    /// Builds a star (kleene closure) of nfa (nfa*)
    /// How this is done: First will come the new initial state, then NFA, then the
    /// new final state
    /// </summary>
    /// <param name="nfa"></param>
    /// <returns></returns>
    public static Nfa BuildNFAStar(Nfa nfa)
    {
        // Makes room for the new initial state
        nfa.ShiftStates(1);

        // Makes room for the new final state
        nfa.AppendEmptyState();

        // Adds new transitions
        nfa.AddTrans(nfa.FinalState, nfa.InitState, EventNode.Epsilon);
        nfa.AddTrans(0, nfa.InitState, EventNode.Epsilon);
        nfa.AddTrans(nfa.FinalState, nfa.Size - 1, EventNode.Epsilon);
        nfa.AddTrans(0, nfa.Size - 1, EventNode.Epsilon);

        nfa.InitState = 0;
        nfa.FinalState = nfa.Size - 1;

        return nfa;
    }
}