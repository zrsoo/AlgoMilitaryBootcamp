public class Solution {
    public int[] FindOrder(int numCourses, int[][] prerequisites) {
        var indeg = new int[numCourses];

        var adj = new List<int>[numCourses];
        for(int i = 0; i < numCourses; ++i)
            adj[i] = new List<int>();

        int depCount = prerequisites.Length;

        for(int i = 0; i < depCount; ++i)
        {
            indeg[prerequisites[i][0]]++;
            adj[prerequisites[i][1]].Add(prerequisites[i][0]);
        }

        var q = new Queue<int>();

        for(int i = 0; i < numCourses; ++i)
            if(indeg[i] == 0)
                q.Enqueue(i);

        var result = new int[numCourses];
        int index = 0;

        while(q.Count > 0)
        {
            int course = q.Dequeue();
            result[index++] = course;

            for(int i = 0; i < adj[course].Count; ++i)
            {
                indeg[adj[course][i]]--;

                if(indeg[adj[course][i]] == 0)
                    q.Enqueue(adj[course][i]);
            }
        }

        return index == numCourses ? result : new int[0];
    }
}
