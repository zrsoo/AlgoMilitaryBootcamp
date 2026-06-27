public class Solution {
    public int CoinChange(int[] coins, int amount) {
        var dp = new int[amount + 1];
        Array.Fill(dp, amount + 1);
        dp[0] = 0;

        for(int a = 1; a <= amount; ++a)
            foreach(var c in coins)
                if(c <= a)
                    dp[a] = Math.Min(dp[a], dp[a - c] + 1);

        return dp[amount] > amount ? -1 : dp[amount];
    }
}
