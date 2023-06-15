using Antlr4.Runtime;
using Antlr4.Runtime.Dfa;
using NUnit.Framework;
using PChecker.Feedback.EventMatcher;

namespace UnitTests.EventMatcher;

[TestFixture]
public class ParserTest
{
    [Test]
    public void TestEventParser()
    {
        string input = "(E1,E2, E3)| E4";

        var parser = new EventLangParser(new CommonTokenStream(new EventLangLexer(new AntlrInputStream(input))));
        var visitor = new EventLangVisitor();
        var node = visitor.Visit(parser.exp());

        var nfa = Nfa.TreeToNFA(node);
        nfa.Show();
        var dfa = Dfa.SubsetConstruct(nfa);
        dfa.Show();

    }
}