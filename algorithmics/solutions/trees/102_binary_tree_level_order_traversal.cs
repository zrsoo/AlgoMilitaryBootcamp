/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    public IList<IList<int>> LevelOrder(TreeNode root) {
        var res = new List<IList<int>>();

        var q = new Queue<TreeNode>();

        if(root != null)
            q.Enqueue(root);

        TreeNode node;
        while(q.Count > 0)
        {
            int levelSize = q.Count;
            var level = new List<int>();

            for(int i = 0; i < levelSize; ++i)
            {
                node = q.Dequeue();
                level.Add(node.val);

                if(node.left != null) q.Enqueue(node.left);
                if(node.right != null) q.Enqueue(node.right);
            }

            res.Add(level);
        }

        return res;
    }
}
