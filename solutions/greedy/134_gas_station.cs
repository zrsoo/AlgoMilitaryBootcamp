public class Solution {
    public int CanCompleteCircuit(int[] gas, int[] cost) {
        int n = gas.Length;

        var totalNet = 0;
        var net = new int[n];

        for(int i = 0; i < n; ++i)
        {
            net[i] = gas[i] - cost[i];
            totalNet += gas[i] - cost[i];
        }

        if(totalNet < 0)
            return -1;

        int tank = 0, pos = 0;

        for(int i = 0; i < n; ++i)
        {
            tank += net[i];

            if(tank < 0)
            {
                pos = i + 1;
                tank = 0;
            }
        }

        return pos;
    }
}
