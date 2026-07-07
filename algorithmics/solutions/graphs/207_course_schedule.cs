public class Solution {
    public bool CanFinish(int numCourses, int[][] prerequisites) {
        var q = new Queue<int>();
        var indeg = new int[numCourses];

        var adj = new List<int>[numCourses];
        for(int i = 0; i < numCourses; ++i)
            adj[i] = new List<int>();

        int n = prerequisites.Length;

        for(int i = 0; i < n; ++i)
        {
            adj[prerequisites[i][1]].Add(prerequisites[i][0]);
            indeg[prerequisites[i][0]]++;
        }

        for(int i = 0; i < numCourses; ++i)
            if(indeg[i] == 0)
                q.Enqueue(i);

        int nrC = 0;

        int course;
        while(q.Count > 0)
        {
            course = q.Dequeue();
            nrC++;

            for(int i = 0; i < adj[course].Count; ++i)
            {
                indeg[adj[course][i]]--;

                if(indeg[adj[course][i]] == 0)
                    q.Enqueue(adj[course][i]);
            }
        }

        return nrC == numCourses;
    }
}
