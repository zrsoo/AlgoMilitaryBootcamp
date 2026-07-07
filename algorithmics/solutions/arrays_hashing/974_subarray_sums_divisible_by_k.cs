public class Solution {
    public int SubarraysDivByK(int[] nums, int k) {
        int n = nums.Length;
        var modGroups = new int[k];
        modGroups[0] = 1;

        int result = 0;
        int prefixMod = 0;

        foreach(var nr in nums)
        {
            prefixMod = (prefixMod + nr % k + k) % k;
            result += modGroups[prefixMod];
            modGroups[prefixMod] += 1;
        }

        return result;
    }
}
