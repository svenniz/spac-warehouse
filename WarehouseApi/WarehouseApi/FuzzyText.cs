namespace WarehouseApi.FuzzyText
{
    /// <summary>
    /// This class does fuzzy text comparisons between individual strings
    /// It is able to return af difference score, or return which strings are equal within certain limits
    /// It is able to load a dictionary of common typos, which it will penalize less (if no such dictionary is supplied, no common typos are used)
    /// 
    /// WARNING, the Fuzzy text comparison function uses a RECURSIVE function to see how close to strings are to each other
    /// The performance is MUCH WORSE than a regular string comparison
    /// 
    /// </summary>
    public class FuzzyComparer
    {
        //A list of similar letters, which are forgivable if we swap, for example, the key 'a' likely refers to a set which contains s and z (since they are physically next to the a key)
        //The typos are loaded from a file at startup
        //BY THE WAY, in C# char is 16 bits, not 8 bit, so this can include characters like æ,ø and å
        private Dictionary<char, HashSet<char>> CommonTypos = new();

        //Use a file to load the fuzzy text comparer
        public FuzzyComparer(string? filename=null)
        {
            if (filename!=null)
            {
                //Error will fall through, if we load something which isn't there
                using (var s = new StreamReader(filename))
                {

                    for (string? line; (line = s.ReadLine()) != null;)
                    {
                        //Empty lines, or lines starting wih ## (comments) are skipped
                        if (line.Length <= 1 || (line[0] == '#' && line[1] == '#'))
                            continue;
                        //Make sure the first char exist in the dictionary
                        if (!CommonTypos.ContainsKey(line[0]))
                            CommonTypos.Add(line[0], new HashSet<char>());

                        for (int i = 1; i < line.Length; i++)
                        {
                            //If we haven't already added the swap of line[0] with line[i] do it now
                            if (!CommonTypos[line[0]].Contains(line[i]))
                                CommonTypos[line[0]].Add(line[i]);

                            //While we are at it, also add the swap of line[i] to line[0], we may have to add the key line[i]
                            if (!CommonTypos.ContainsKey(line[i]))
                                CommonTypos.Add(line[i], new HashSet<char>());
                            if (!CommonTypos[line[i]].Contains(line[0]))
                                CommonTypos[line[i]].Add(line[0]);
                        }
                    }
                }

            }
        }

        //Binary flags with options for fuzziness
        [Flags]
        public enum Options
        {
            None = 0,
            IgnoreExtraLength = 0b1000,//A difference in length count only as one difference, rather than 1 per char (Instead of counting tex as 2 chars different from texty we will count it as 1 different)
            IgnoreCommonTypos = 0b0100,//Anything in the list of common typos get 0 cost (text = t3xt) this AUTOMATICALLY ignores case
            IgnoreCase = 0b0010,//letters case do not matter (cAt = CaT), this is redundant if using IgnoreCommonTypos
            IgnoreDuplicates = 0b0001//Ignore duplication errors (telling = teling) Very common with dyslexic people
        }

        //This is what I consider reasonable levels, the examples include things which are the same under these rules (with Options.None, and the Danish Keyboard Dictionary I have included in the project)
        //The number is how large difference score we allow per 4 chars in the words (if the words have difference lengths, we use the score divided by the average length)
        public enum Level
        {
            Strict = 0,  //Exact matches only
            Moderate = 1,//A few typos are allowed, example:     Hallelujah = Haleluja
            Fuzzy = 2,   //Many typos are allowed, example:      Exquisite = Exvisit
            Crazy = 3,   //Absurdly many typos allowed, example: Excelsior = Ekselsjor
        }

        /// <summary>
        /// WARNING, this is uses a RECURSIVE function for Fuzzy text comparison, the performance cost is MUCH HIGHER than a normal comparison
        /// Return whether a = b given this fuzziness level, and with these options active
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="level"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Equals(string a, string b, Level level = Level.Fuzzy, Options options = Options.None)
        {

            return (Compare(a, b,((a.Length + b.Length) * ((int)level)/8+1), options) * 8) <= (a.Length + b.Length) * ((int)level);
        }

        /// <summary>
        /// WARNING, this is uses a RECURSIVE function for Fuzzy text comparison, the performance cost is MUCH HIGHER than a normal comparison
        /// Returns difference score between strings, given these options
        /// A positive number is always returned
        /// </summary>
        /// <param name="a">first string (order doesn't matter)</param>
        /// <param name="b">second string (order doesn't matter)</param>
        /// <param name="MaxCost">Max cost to return, if the cost exceeds this we will break the recursion early and return max cost+1, a smaller max cost results in better performance </param>
        /// <param name="options">binary flags for options</param>
        /// <returns></returns>
        public int Compare(string a, string b, int MaxCost, Options options = 0)
        {

            //Default settings, things which are harder to do by accident get twice the cost as easy typos

            //The easiest typos to do in my opinion are :
            int SwapCost = 1;//swapped two adjacent chars? (etxt instead of text)
            int TypoCost_similar = (Options.IgnoreCommonTypos == (options & Options.IgnoreCommonTypos)) ? 0 : 1;//Typed a char wrong using a very Common typo or replacement (t3xt instead of text), read from the dictionary of common typos
            int DuplicateCost = (Options.IgnoreDuplicates == (options & Options.IgnoreDuplicates)) ? 0 : 1;//Duplicated a char (teext instead of text), this is easy to do by accident (and dyslexic people have a hard time telling teling apart from telling)

            //Missing one or more chars at the end is easy, so it gets a score of 1 for each char you miss
            //By default, each char you overshoot gets a cost, but we can replace that with a fixed cost
            int ExtraLengthCost = 1;//Additional cost each char length difference (texty instead of tex would get this cost twice)
            int LengthCostBase = 0;//Base cost if one string is longer than the other (texty instead of tex would get this cost once)
            
            if (Options.IgnoreExtraLength == (options & Options.IgnoreExtraLength))
            {
                //If we are to ignore additional length, remove the cost per char, and set base to 1
                ExtraLengthCost = 0;
                LengthCostBase = 1;
            }
            
            //These things are harder to do by accident, so they increase the score twice as much
            int TypoCost_different = 2;//Typed a char wrong using an uncommon typo (tixt instead of text)
            int InsertCost = 2;//Inserted an additional char which is not a dublicate (tecxt instead of text)

            //If we ignore case, we simply make the strings lowercase before comparing
            bool ignoreCase = Options.IgnoreCase == (options & Options.IgnoreCase);
            //Use an internal version, which calls itself recursively, the 0s say we start at the beginning of the strings
            return Compare(ignoreCase ? a.ToLower() : a, ignoreCase ? b.ToLower() : b, 0, 0, 0, MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost);
        }

        /// <summary>
        /// Text comparison where we start with our id at some location ida or idb in strings a and b
        ///This is used to recursively compare the ends of the string, in order to find the
        ///The costs are given from elsewhere (wherever the options were interpreted)
        ///
        /// We may set a max length to check of both strings, which is useful when checking one much smaller string against a larger one
        /// </summary>
        /// <param name="a">first string</param>
        /// <param name="b">second string (order doesn't matter)</param>
        /// <param name="ida">index in string a we start</param>
        /// <param name="idb">index in string a we start</param>
        /// <param name="Cost">Starting cost (used to store the cost of the comparison before ida and idb in a recursive call)</param>
        /// <param name="MaxCost">Automatically break if cost is greater than this</param>
        /// <param name="SwapCost">cost if we detect a swap (telling <-> tleling)</param>
        /// <param name="TypoCost_similar">cost if we detect a common typo (telling <-> tell1ng)</param>
        /// <param name="TypoCost_different">cost if we detect an uncommon typo (telling <-> tellxng) </param>
        /// <param name="DuplicateCost">cost if we detect an extra duplicate letter, or a duplicate is missing (telling <-> teling) very common for dyslexic people </param>
        /// <param name="InsertCost">cost if an extra (not duplicate) letter has been inserted in either string (telling <-> telqling)</param>
        /// <param name="LengthCostBase">Cost for reaching the end of one word before the other</param>
        /// <param name="ExtraLengthCost">Additional cost for each char the one word is longer than the other</param>
        /// <returns></returns> string, in order to find the
        /// 
        private int Compare(string a, string b, int ida, int idb, int Cost,int MaxCost,int SwapCost, int TypoCost_similar, int TypoCost_different, int DuplicateCost, int InsertCost, int LengthCostBase, int ExtraLengthCost,int maxAlength=-1/*Only search until this point in string a or b*/,int maxBlength=-1)
        {
            //Break early, if we KNOW this will be rejected (This is arguably redundant, since I also check each time I launch a recursion)
            if (Cost > MaxCost)
                return MaxCost + 1;
            //Set the length to either whatever the user wants (where -1 means search all), or the largest allowable length
            maxAlength = (maxAlength == -1)? a.Length :Math.Min(a.Length,maxAlength);
            maxBlength = (maxBlength == -1)? b.Length :Math.Min(b.Length,maxBlength);

            for (; ida < maxAlength && idb < maxBlength; ida++, idb++)
            {

                
                {
                    char A = a[ida];
                    char B = b[idb];
                    //If we haven't overshot yet, is there any difference?
                    if (A != B)
                    {

                        //There are 4 other possibilities, we will recursively check all and return what we get


                        //We will be finding the smallest difference assuming either a swap, a single wrong character, or an insertion has taken place

                        //Start with a minCost above our cutoff, that way if ANY of our 4 possibilities result in a smaller score we will take that
                        int minCost = MaxCost + 1;

                        //We always check for typos, I enclose this in a scope to avoid reused name warnings
                        {
                            //Cost of this operation, it is the cost so far + either the cost of a common or uncommon typo
                            int thisCost = Cost
                                 //The condition checks if char A has known common typos, and if yes one of them is Char B
                                 + ((CommonTypos.ContainsKey(A) && CommonTypos[A].Contains(B)) ? TypoCost_similar : TypoCost_different);

                            //Now thisCost is the BEST CASE Total cost assuming this possibility
                            //It only makes sense to even check the actual cost, if there is any chance it is better
                            if (thisCost<minCost )
                                minCost = Math.Min(minCost, Compare(a, b, ida + 1, idb + 1,thisCost,MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost,maxAlength,maxBlength));
                        }

                        //Now check if a swap (where for example we wrote etxt instead of text) can have taken place
                        //Get the next letter in both strings (may be the end of the line, in that case use (char)0)
                        char nextA = ida + 1 < maxAlength ? a[ida + 1] : (char)0;
                        char nextB = idb + 1 < maxBlength ? b[idb + 1] : (char)0;
                        //If they are like this, we could be in a text<->etxt situation
                        if (nextA == B && nextB == A)
                        {
                            //Cost of this operation AND everything so far, in this case swap cost plus our cost
                            int thisCost = SwapCost+Cost;
                            //Again, only check if it might be better
                            if (thisCost<=minCost)
                                minCost = Math.Min(minCost, Compare(a, b, ida + 2, idb + 2,thisCost,MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost, maxAlength, maxBlength) + SwapCost);
                        }

                        if (ida + 1 < maxAlength )//If the id of a is not outside range, check if we have inserted a single char in a, the conditional uses dublicate cost if the previous thing in a was the same as this (for example a=teext and b=text)
                        {
                            //The cost depends if this is a duplication or another kind of insertion
                            int thisCost = Cost+(ida>0 && a[ida-1]==A? DuplicateCost : InsertCost);
                            //Again, only check if it might be better
                            if (thisCost<=minCost)
                                minCost = Math.Min(minCost, Compare(a, b, ida + 1, idb, thisCost, MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost, maxAlength, maxBlength));
                        }
                        if (idb + 1 < maxBlength)//Now check insertions in b
                        {

                            //The cost depends if this is a duplication or another kind of insertion
                            int thisCost = Cost+(idb>0 && b[idb-1]==B? DuplicateCost : InsertCost);
                            //Again, only check if it might be better
                            if (thisCost<=minCost)
                                minCost = Math.Min(minCost, Compare(a, b, ida, idb+1, thisCost, MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost, maxAlength, maxBlength));
                        }

                        return minCost;
                    }
                }
            }

            if (ida >= maxAlength)
            {
                return LengthCostBase + ExtraLengthCost * (maxBlength - idb)+Cost;
            }
            else if (idb >= maxBlength)
            {
                return LengthCostBase + ExtraLengthCost * (maxAlength - ida)+Cost;
            }


            return Cost;//If we get here, no differences were found and we should return the cost we started with
        }

        /// <summary>
        /// Checks if the shorter string is contained in the longer string, given this level and these options
        /// a perfect score is returned if the longer string contains the shorter string, regardless of how much else is in the longer string
        /// Also return the matching string so we can see how good (or bad) it is
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="MaxCost">Max cost to return, if the cost exceeds this we will break the recursion early and return max cost+1, a smaller max cost results in better performance </param>
        /// <param name="level"></param>
        /// <param name="bestmatch">Returned string</param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Contains(string a, string b, out string bestmatch/*This version returns the best match in the longer string*/, Level level = Level.Fuzzy, Options options = Options.None)
        {
            return ContainScore(a, b, ((int)level) * (a.Length)/8+1, out bestmatch, options) * 8 <= ((int)level) * (a.Length);
        }
        /// <summary>
        /// Checks if the shorter string is contained in the longer string, given this level and these options
        /// a perfect score is returned if the longer string contains the shorter string, regardless of how much else is in the longer string
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="MaxCost">Max cost to return, if the cost exceeds this we will break the recursion early and return max cost+1, a smaller max cost results in better performance </param>
        /// <param name="level"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Contains(string a, string b, Level level = Level.Fuzzy, Options options = Options.None)
        {
            //Swap the shortest string into a
            if (a.Length > b.Length)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            if (ContainScore(a, b,  (((int)level) * (a.Length))/4+1, out string bestmatch, options) * 4 <= ((int)level) * (a.Length))
            {
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// An alternative comparison, which checks if the smaller string is contained in b, returns a score which is how well the best matching part of the longer string matches the shorter string
        /// The score is similar to the normal compare function above, given the same options, higher score means worse match
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="MaxCost">Max cost to return, if the cost exceeds this we will break the recursion early and return max cost+1, a smaller max cost results in better performance </param>
        /// <param name="options"></param>
        /// <returns></returns>
        public int ContainScore(string a, string b,int MaxCost, out string bestmatch, Options options = 0)
        {
            //Swap the shortest string into a
            if (a.Length > b.Length)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            //Copy the otpions loading code from the Compare function
            //Default settings, things which are harder to do by accident get twice the cost as easy typos

            //The easiest typos to do IMHO are :
            int SwapCost = 1;//swapped two adjacent chars? (etxt instead of text)
            int TypoCost_similar = (Options.IgnoreCommonTypos == (options & Options.IgnoreCommonTypos)) ? 0 : 1;//Typed a char wrong using a very Common typo or replacement (t3xt instead of text)
            int DuplicateCost = (Options.IgnoreDuplicates == (options & Options.IgnoreDuplicates)) ? 0 : 1;//Duplicated a char (teext instead of text), this is easy to do by accident (and dyslexic people have a hard time telling teling apart from telling)

            //Missing one or more chars at the end is easy, so it gets a score of 1 for each char you miss
            int ExtraLengthCost = 1;//Additional cost each char length difference (texty instead of tex would get this cost twice)
            int LengthCostBase = 0;//Base cost if one string is longer than the other (texty instead of tex would get this cost once)

            if (Options.IgnoreExtraLength == (options & Options.IgnoreExtraLength))
            {
                //If we are to ignore additional length, remove the cost per char, and set base to 1
                ExtraLengthCost = 0;
                LengthCostBase = 1;
            }

            int TypoCost_different = 2;//Typed a char wrong using an uncommon typo (tixt instead of text)
            int InsertCost = 2;//Inserted an additional char which is not a dublicate (tecxt instead of text)

            bool ignoreCase = Options.IgnoreCase == (options & Options.IgnoreCase);
            //Use an internal version, which calls itself recursively

            //We are simply going to check every possible place the smaller string a could be in b, until we find the best match

            //Start by seeing how good a match we have, if we place a at the start of b like this
            //B:                Letters in B string, which does contain a string
            //A:                a string

            int bestScore = Compare(ignoreCase ? a.ToLower() : a, ignoreCase ? b.ToLower() : b, 0, 0, 0,MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost, a.Length, a.Length); ;
            int best_idb = 0;//Best starting id in string b

            //Now start moving a, one step at the time
            for (int start_idb = 1; start_idb + a.Length <= b.Length && bestScore != 0/*Quit when a perfect match is found*/; ++start_idb)
            {


                //If this is the best so far, also move the starting id of the best
                int thisScore = Compare(ignoreCase ? a.ToLower() : a, ignoreCase ? b.ToLower() : b, 0, start_idb, 0,MaxCost, SwapCost, TypoCost_similar, TypoCost_different, DuplicateCost, InsertCost, LengthCostBase, ExtraLengthCost, a.Length, a.Length + start_idb);
                Console.WriteLine($"try {a} in {b.Substring(start_idb,a.Length)} got score {thisScore} ? {bestScore} with max {MaxCost}");
                if (thisScore < bestScore)
                {
                    bestScore = thisScore;
                    best_idb = start_idb;
                }
                else
                {
                    Console.WriteLine($"{a} is not {b.Substring(start_idb,a.Length)} score {thisScore}");
                }

            }

            //Get the best string
            bestmatch = b.Substring(best_idb, a.Length);

            Console.WriteLine($"Best match of {a} in {b} is : {bestmatch} with {bestScore}");
            return bestScore;
        }
    }
}
