using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Atn;
using PChecker.Actors.Events;

namespace PChecker.Feedback.EventMatcher;

public class Dfa
{
    // Start state
    public int InitState;
    // Set of final states
    public HashSet<int> FinalState;
    // Transition table
    public Dictionary<int, Dictionary<EventNode, int>> Transitions;

    private int CurrentState;

    public Dfa(int initState)
    {
        InitState = initState;
        FinalState = new();
        Transitions = new();
    }

    // public string Simulate(string @in)
    // {
    // }

    public void Show()
    {
        Console.Write("DFA start state: {0}\n", InitState);
        Console.Write("DFA final state(s): ");
        foreach (var i in FinalState)
        {
            Console.Write(i + " ");
        }
        Console.Write("\n\n");
        foreach (var kvp in Transitions)
        {
            foreach (var p in kvp.Value)
            {
                Console.Write("Trans[{0}, {1}] = {2}\n", kvp.Key, p.Key.EventName, p.Value);
            }

        }
    }


    public static Dfa SubsetConstruct(Nfa nfa)
    {
        int dfaState = 0;

        // Sets of NFA states which is represented by some DFA state
        HashSet<HashSet<int>> markedStates = new(new HashSetEqualityComparer());
        HashSet<HashSet<int>> unmarkedStates = new(new HashSetEqualityComparer());

        // Gives a number to each state in the DFA
        Dictionary<HashSet<int>, int> dfaStateNum = new( new HashSetEqualityComparer());

        HashSet<int> nfaInitial = new HashSet<int>();
        nfaInitial.Add(nfa.InitState);

        // Initially, EpsilonClosure(nfa.initial) is the only state in the DFAs states
        // and it's unmarked.
        HashSet<int> first = nfa.EpsilonClosure(nfaInitial);
        unmarkedStates.Add(first);

        // The initial dfa state
        int dfaInitial = dfaState++;
        dfaStateNum[first] = dfaInitial;
        Dfa dfa = new Dfa(dfaState++);

        while(unmarkedStates.Count != 0)
        {
            // Takes out one unmarked state and posteriorly mark it.
            HashSet<int> aState = unmarkedStates.First();

            // Removes from the unmarked set.
            unmarkedStates.Remove(aState);

            // Inserts into the marked set.
            markedStates.Add(aState);

            // If this state contains the NFA's final state, add it to the DFA's set of
            // final states.
            if(aState.Contains(nfa.FinalState))
                dfa.FinalState.Add(dfaStateNum[aState]);

            foreach (var eventNode in nfa.Inputs)
            {
                HashSet<int> next = nfa.EpsilonClosure(nfa.Move(aState, eventNode));

                if (!unmarkedStates.Contains(next) && !markedStates.Contains(next))
                {
                    unmarkedStates.Add(next);
                    dfaStateNum.Add(next, dfaState++);
                }

                if (!dfa.Transitions.ContainsKey(dfaStateNum[aState]))
                {
                    dfa.Transitions[dfaStateNum[aState]] = new Dictionary<EventNode, int>();
                }
                dfa.Transitions[dfaStateNum[aState]][eventNode] = dfaStateNum[next];
            }
        }

        return dfa;
    }
}