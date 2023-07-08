using System;
using System.Collections.Generic;
using System.Linq;
using PChecker.Matcher;
using Plang.CSharpRuntime.Values;
using Plang.CSharpRuntime;

namespace PImplementation {

    class OpMatcher : ElementMatcher
    {
        private Type _op;
        public OpMatcher(Type op)
        {
            _op = op;
        }
        public override bool Matches(EventObj e)
        {
            return e.Event.GetType() == _op;
        }
    }
    partial class GlobalFunctions
    {
        public static (IMatcher<List<EventObj>>, HashSet<Type>) EventSeqPatternMatcher()
        {
            var interestingEvents = new HashSet<Type>();
            interestingEvents.Add(typeof(eEventA));
            interestingEvents.Add(typeof(eEventB));
            interestingEvents.Add(typeof(eEventC));
            var a = new OpMatcher(typeof(eEventA));
            var b = new OpMatcher(typeof(eEventB));
            var c = new OpMatcher(typeof(eEventC));
            return (new ExistBeforeFirst(c, b).And(new ExistBeforeFirst(b, a)), interestingEvents);
        }
    }
}