public class Solution {
    public int MyAtoi(string s) {
        s = s.Trim();

        if(s.Length == 0) return 0;

        if(s[0] != '+' && s[0] != '-' && (s[0] < '0' || s[0] > '9'))
            return 0;

        bool pos = false;

        if(s[0] == '-') pos = false;
        else pos = true;

        int idx = 0;
        if(s[idx] == '-' || s[idx] == '+') idx++;
        while(idx < s.Length && s[idx] == '0')
        {
            idx++;
        }

        if(idx == s.Length || (s[idx] < '0' || s[idx] > '9')) return 0;

        int nr = 0;
        while(idx < s.Length && s[idx] >= '0' && s[idx] <= '9')
        {
            try
            {
                checked
                {
                    nr = nr * 10 + (s[idx] - '0');
                }
                idx++;
            }
            catch(OverflowException ex)
            {
                return pos ? int.MaxValue : int.MinValue;
            }
        }

        return pos ? nr : -nr;
    }
}
