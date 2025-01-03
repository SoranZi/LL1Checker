#region givern-data

class LL1Parser
{
    static void Main(string[] args)
    {
        // Define grammar
        // Example grammar with rules: S -> c ( S ) | ε
        Dictionary<string, List<string>> grammar = new Dictionary<string, List<string>>
        {
            { "S", new List<string> { "c(S)", "" } }
        };

        // Predefined precedence table
        Dictionary<string, string> precedenceTable = new Dictionary<string, string>
        {
            { "c", "(" },
            { "(", "S" },
            { "S", ")" },
            { ")", "$" }
        };

        // Check if the grammar is LL(1)
        bool isLL1 = IsLL1Grammar(grammar, precedenceTable);

        if (isLL1)
            Console.WriteLine("The given grammar is LL(1).\n");
        else
            Console.WriteLine("The given grammar is not LL(1).\n");
    }

    static bool IsLL1Grammar(Dictionary<string, List<string>> grammar, Dictionary<string, string> precedenceTable)
    {
        // Check LL(1) conditions:
        // 1. No overlapping FIRST and FOLLOW sets
        // 2. Each column in the parse table has a single entry

        foreach (var rule in grammar)
        {
            var nonTerminal = rule.Key;
            var productions = rule.Value;

            // Compute FIRST for each production
            HashSet<char> firstSet = new HashSet<char>();
            foreach (var production in productions)
            {
                if (!string.IsNullOrEmpty(production))
                {
                    char firstChar = production[0];
                    if (firstSet.Contains(firstChar))
                        return false; // Conflict in FIRST set
                    firstSet.Add(firstChar);
                }
            }

            // Check precedence table
            foreach (var entry in precedenceTable)
            {
                if (entry.Key.StartsWith(nonTerminal) && entry.Value.StartsWith(nonTerminal))
                {
                    return false; // Conflict in precedence table
                }
            }
        }

        return true;
    }
}

#endregion givern-data