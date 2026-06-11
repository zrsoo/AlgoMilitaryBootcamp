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
    public bool IsValidBST(TreeNode root) => Valid(root, long.MinValue, long.MaxValue);

    private bool Valid(TreeNode node, long low, long high)
    {
        if (node == null) return true;
        if (node.val <= low || node.val >= high) return false;
        return Valid(node.left,  low, node.val)   // left subtree: upper bound = node.val
            && Valid(node.right, node.val, high); // right subtree: lower bound = node.val
    }
}
