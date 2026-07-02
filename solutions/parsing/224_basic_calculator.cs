public class Solution {
    public int Calculate(string s) {
        int acc = 0;
        bool flipped = false;
        char sgn = '+';
        var rsgn = new Stack<char>();

        int nr = 0;
        foreach(char c in s)
        {
            if(c == ' ') continue;

            if(Char.IsDigit(c))
            {
                nr = nr * 10 + (c - '0');
            }

            if(!Char.IsDigit(c)) 
            {
                if(sgn == '+')
                {
                    if(rsgn.Count > 0 && rsgn.Peek() == '-') { acc -= nr; }
                    else { acc += nr; }
                }
                else
                {
                    if(rsgn.Count > 0 && rsgn.Peek() == '-') { acc += nr; }
                    else { acc -= nr; }
                }

                nr = 0;
            }

            if(c == '+' || c == '-') sgn = c;

            if(c == '(') 
            {
                if(rsgn.Count > 0 && rsgn.Peek() == '-') 
                {
                    if(sgn == '+') rsgn.Push('-');
                    else rsgn.Push('+');
                }
                else rsgn.Push(sgn);
                
                if(sgn == '-') sgn = '+';
            }

            if(rsgn.Count > 0 && c == ')') rsgn.Pop();
        }

        if(sgn == '+')
        {
            if(rsgn.Count > 0 && rsgn.Peek() == '-') { acc -= nr; }
            else { acc += nr; }
        }
        else
        {
            if(rsgn.Count > 0 && rsgn.Peek() == '-') { acc += nr; }
            else { acc -= nr; }
        }

        return acc;
    }
}
