public class Solution {
    public IList<IList<int>> ThreeSum(int[] nums) {
        int n = nums.Length;
        var result = new List<IList<int>>();

        Array.Sort(nums);

        int l, r, sum;

        foreach(var nr in nums)
            Console.Write(nr + " ");
        Console.WriteLine();

        // Outer loop, fix x
        for(int i = 0; i < n - 2; ++i)
        {
            if(nums[i] > 0 || (i > 0 && nums[i] == nums[i - 1]))
                continue;

            l = i + 1;
            r = n - 1;

            Console.WriteLine($"{nums[i]}, {nums[l]}, {nums[r]}");

            while(l < r)
            {
                sum = nums[i] + nums[l] + nums[r];
                // Found triplet
                if(sum == 0)
                {
                    result.Add(new List<int>{ nums[i], nums[l], nums[r] });
                    Console.WriteLine($"{nums[i]}, {nums[l]}, {nums[r]}");

                    // Dedupe
                    while(l < r && nums[l] == nums[l + 1])
                        l++;

                    // Dedupe
                    while(l < r && nums[r] == nums[r - 1])
                        r--;

                    // Advance
                    l++;
                    r--;
                }
                // Sum is less than 0, advance left to increase sum
                else if(sum < 0)
                {
                    l++;
                }
                // Sum is greater than 0, advance right to decrease sum
                else
                {
                    r--;
                }
            }
        }

        return result;
    }
}
