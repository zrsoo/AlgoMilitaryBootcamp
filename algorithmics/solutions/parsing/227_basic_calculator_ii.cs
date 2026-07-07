public class Solution {
    public int Calculate(string s) {
        s = s.Replace(" ", "");

        var nrs = new Stack<int>();

        int nr = 0;
        char sgn = '+';

        for(int i = 0; i < s.Length; ++i)
        {
            if(Char.IsDigit(s[i]))
            {
                nr = nr * 10 + s[i] - '0';
            }
            else
            {
                if(sgn == '+') nrs.Push(nr);
                if(sgn == '-') nrs.Push(-nr);
                if(sgn == '*') nrs.Push(nrs.Pop() * nr);
                if(sgn == '/') nrs.Push(nrs.Pop() / nr);

                nr = 0;
                sgn = s[i];
            }
        }
        // flush the final number (no trailing operator to trigger it in the loop)
        if(sgn == '+') nrs.Push(nr);
        if(sgn == '-') nrs.Push(-nr);
        if(sgn == '*') nrs.Push(nrs.Pop() * nr);
        if(sgn == '/') nrs.Push(nrs.Pop() / nr);

        return nrs.Sum();
    }
}
