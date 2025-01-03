using System;
using System.Collections.Generic;
using System.Linq;

class LL1GrammarChecker
{
    static void Main(string[] args)
    {
        Console.WriteLine("Enter the number of productions:");
        int numProductions = int.Parse(Console.ReadLine());

        var grammar = new Dictionary<string, List<string>>();

        Console.WriteLine("Enter the productions in the form A->alpha (e.g., S->aB):");
        for (int i = 0; i < numProductions; i++)
        {
            string[] production = Console.ReadLine().Split("->", StringSplitOptions.RemoveEmptyEntries);
            string nonTerminal = production[0].Trim();
            string[] rules = production[1].Split('|');

            if (!grammar.ContainsKey(nonTerminal))
                grammar[nonTerminal] = new List<string>();

            grammar[nonTerminal].AddRange(rules.Select(r => r.Trim()));
        }

        Console.WriteLine("Calculating FIRST and FOLLOW sets...");

        var first = CalculateFirstSets(grammar);
        var follow = CalculateFollowSets(grammar, first);

        Console.WriteLine("FIRST sets:");
        foreach (var entry in first)
        {
            Console.WriteLine($"FIRST({entry.Key}) = {{ {string.Join(", ", entry.Value)} }}");
        }

        Console.WriteLine("FOLLOW sets:");
        foreach (var entry in follow)
        {
            Console.WriteLine($"FOLLOW({entry.Key}) = {{ {string.Join(", ", entry.Value)} }}");
        }

        if (IsLL1Grammar(grammar, first, follow))
        {
            Console.WriteLine("The grammar is LL(1).\n");
        }
        else
        {
            Console.WriteLine("The grammar is NOT LL(1).\n");
        }
    }

    static Dictionary<string, HashSet<string>> CalculateFirstSets(Dictionary<string, List<string>> grammar)
    {
        var first = new Dictionary<string, HashSet<string>>();

        foreach (var nonTerminal in grammar.Keys)
        {
            first[nonTerminal] = new HashSet<string>();
        }

        bool changed;
        do
        {
            changed = false;

            foreach (var nonTerminal in grammar.Keys)
            {
                foreach (var production in grammar[nonTerminal])
                {
                    foreach (char symbol in production)
                    {
                        string symStr = symbol.ToString();

                        if (!char.IsUpper(symbol)) // Terminal
                        {
                            if (!first[nonTerminal].Contains(symStr))
                            {
                                first[nonTerminal].Add(symStr);
                                changed = true;
                            }
                            break;
                        }
                        else // Non-terminal
                        {
                            foreach (var firstSymbol in first[symStr])
                            {
                                if (!first[nonTerminal].Contains(firstSymbol))
                                {
                                    first[nonTerminal].Add(firstSymbol);
                                    changed = true;
                                }
                            }

                            if (!first[symStr].Contains("ε"))
                                break;
                        }
                    }
                }
            }

        } while (changed);

        return first;
    }

    static Dictionary<string, HashSet<string>> CalculateFollowSets(Dictionary<string, List<string>> grammar, Dictionary<string, HashSet<string>> first)
    {
        var follow = new Dictionary<string, HashSet<string>>();

        foreach (var nonTerminal in grammar.Keys)
        {
            follow[nonTerminal] = new HashSet<string>();
        }

        follow[grammar.Keys.First()].Add("$"); // Start symbol

        bool changed;
        do
        {
            changed = false;

            foreach (var nonTerminal in grammar.Keys)
            {
                foreach (var production in grammar[nonTerminal])
                {
                    for (int i = 0; i < production.Length; i++)
                    {
                        char symbol = production[i];

                        if (char.IsUpper(symbol)) // Non-terminal
                        {
                            string symStr = symbol.ToString();
                            HashSet<string> trailer = new HashSet<string>(follow[nonTerminal]);

                            for (int j = i + 1; j < production.Length; j++)
                            {
                                char nextSymbol = production[j];
                                string nextSymStr = nextSymbol.ToString();

                                if (!char.IsUpper(nextSymbol)) // Terminal
                                {
                                    trailer.Clear();
                                    trailer.Add(nextSymStr);
                                    break;
                                }
                                else
                                {
                                    trailer.UnionWith(first[nextSymStr]);
                                    trailer.Remove("ε");

                                    if (!first[nextSymStr].Contains("ε"))
                                        break;
                                }
                            }

                            if (trailer.Count > 0)
                            {
                                int prevCount = follow[symStr].Count;
                                follow[symStr].UnionWith(trailer);
                                if (follow[symStr].Count > prevCount)
                                    changed = true;
                            }
                        }
                    }
                }
            }

        } while (changed);

        return follow;
    }

    static bool IsLL1Grammar(Dictionary<string, List<string>> grammar, Dictionary<string, HashSet<string>> first, Dictionary<string, HashSet<string>> follow)
    {
        foreach (var nonTerminal in grammar.Keys)
        {
            var productions = grammar[nonTerminal];

            // بررسی تداخل مجموعه‌های FIRST و FOLLOW برای تمامی تولیدات
            for (int i = 0; i < productions.Count; i++)
            {
                for (int j = i + 1; j < productions.Count; j++)
                {
                    var first1 = GetFirstSetForProduction(productions[i], first);
                    var first2 = GetFirstSetForProduction(productions[j], first);

                    // بررسی تداخل مستقیم در مجموعه‌های FIRST
                    if (first1.Intersect(first2).Any())
                        return false;

                    // بررسی FOLLOW در صورت وجود ε در یکی از مجموعه‌های FIRST
                    if (first1.Contains("ε") && follow[nonTerminal].Intersect(first2).Any())
                        return false;

                    if (first2.Contains("ε") && follow[nonTerminal].Intersect(first1).Any())
                        return false;
                }
            }
        }

        return true;
    }


    static HashSet<string> GetFirstSetForProduction(string production, Dictionary<string, HashSet<string>> first)
    {
        var result = new HashSet<string>();

        foreach (char symbol in production)
        {
            string symStr = symbol.ToString();

            if (!char.IsUpper(symbol)) // Terminal
            {
                result.Add(symStr);
                break;
            }
            else // Non-terminal
            {
                result.UnionWith(first[symStr]);
                if (!first[symStr].Contains("ε"))
                    break;
            }
        }

        return result;
    }
}