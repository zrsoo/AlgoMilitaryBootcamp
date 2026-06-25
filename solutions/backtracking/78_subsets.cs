public class Solution {
    public IList<IList<int>> Subsets(int[] nums) {        
        var res = new List<IList<int>>();
        var curr = new List<int>();

        Recurse(0, nums, curr, res);

        return res;
    }

    private void Recurse(int i, int[] nums, List<int> curr, List<IList<int>> result)
    {
        result.Add(new List<int>(curr));

        for(int start = i; start < nums.Length; ++start)
        {
            curr.Add(nums[start]);
            Recurse(start + 1, nums, curr, result);
            curr.RemoveAt(curr.Count - 1);
        }
    }
}
